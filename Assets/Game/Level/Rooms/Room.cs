using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms.RoomControlScripts;
using TonyDev.Game.UI.Minimap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace TonyDev.Game.Level.Rooms
{
    public static class ReadWriteRooms
    {
        public static void WriteRoomArray(this NetworkWriter writer, Room[,] value)
        {
            var isNull = value == null;
            writer.WriteBool(isNull);

            if (isNull) return;

            var dimension0 = value.GetLength(0);
            var dimension1 = value.GetLength(1);

            writer.WriteInt(dimension0);
            writer.WriteInt(dimension1); //write the number of dimensions of the 2nd dimension

            for (var i = 0; i < dimension0; i++)
            {
                for (var j = 0; j < dimension1; j++)
                {
                    var netId = value[i, j]?.netIdentity.netId;
                    writer.WriteUIntNullable(netId);
                }
            }
        }

        public static Room[,] ReadRoomArray(this NetworkReader reader)
        {
            var isNull = reader.ReadBool();

            if (isNull) return null;

            var dimension0 = reader.ReadInt();
            var dimension1 = reader.ReadInt();

            var rooms = new Room[dimension0, dimension1];

            for (var i = 0; i < dimension0; i++)
            {
                for (var j = 0; j < dimension1; j++)
                {
                    var netId = reader.ReadUIntNullable();
                    if (!netId.HasValue) continue;
                    rooms[i, j] = RoomManager.Instance.RoomIdentities[netId.Value];
                }
            }

            return rooms;
        }

        public static void WriteMap(this NetworkWriter writer, Map value)
        {
            writer.WriteRoomArray(value.Rooms);
            writer.WriteVector2Int(value.StartingRoomPos);
            writer.Write(value.RoomConnections);
        }

        public static Map ReadMap(this NetworkReader reader, Map value)
        {
            var rooms = reader.ReadRoomArray();
            var startingRoomPos = reader.ReadVector2Int();
            var roomConnections = reader.Read<RoomConnection[]>();
            return new Map(rooms, startingRoomPos, roomConnections);
        }
    }

    public class Room : NetworkBehaviour
    {
        public bool started = false;

        //Editor variables
        [SerializeField] private RoomDoor[] roomDoors;
        [SerializeField] private GameObject spawnPrefabOnClear;
        [SerializeField] private Transform entryPointUp;
        [SerializeField] private Transform entryPointDown;
        [SerializeField] private Transform entryPointLeft;
        [SerializeField] private Transform entryPointRight;

        [SerializeField] public float timeMultiplier = 1f;

        public UnityEvent onRoomClearServer;
        public UnityEvent onRoomClearGlobal;

        public Sprite minimapIcon;

        [field: SyncVar(hook = nameof(PlayerCountHook))]
        public int PlayerCount { get; private set; }

        private void PlayerCountHook(int oldPlayerCount, int newPlayerCount)
        {
            //MinimapManager.Instance.UpdatePlayerCount(RoomManager.Instance.Map.GetRoomPos(this), newPlayerCount);
        }

        //
        private List<Direction> _openDirections;
        public Rect RoomRect => FindRoomRect();

        private readonly SyncDictionary<Direction, bool> _openDoorsDictionary = new();

        //The child GameObjects of this room, as dictated by IHideable
        public List<GameObject> roomChildObjects = new();

        public IEnumerable<GameEntity> ContainedEntities =>
            GameManager.EntitiesReadonly.Where(e => e.CurrentParentIdentity == netIdentity);

        [Command(requiresAuthority = false)]
        public void CmdSetPlayerCount(int playerCount)
        {
            PlayerCount = playerCount;
        }

        private void OnOpenDoorsDictionaryChange(SyncDictionary<Direction, bool>.Operation op, Direction key, bool open)
        {
            roomDoors.FirstOrDefault(rd => rd.direction == key)?.SetOpen(open);
        }

        [Command(requiresAuthority = false)]
        private void CmdSetDoorOpen(Direction direction, bool open)
        {
            _openDoorsDictionary[direction] = open;
        }

        [ClientCallback]
        private void CheckRoomVisibility(NetworkIdentity newRoom)
        {
            if (Player.LocalInstance == null || this == null) return;
            SetVisibility(newRoom == netIdentity);
        }

        private void SetVisibility(bool visible)
        {
            foreach (var rend in GetComponentsInChildren<Renderer>())
                rend.enabled = visible;
            foreach (var img in GetComponentsInChildren<Image>())
                img.enabled = visible;
            foreach (var l2d in GetComponentsInChildren<Light2DBase>())
                l2d.enabled = visible;
            foreach (var roomDoor in GetComponentsInChildren<RoomDoor>())
                roomDoor.SetHostVisibility(visible);
        }

        public override void OnStartServer()
        {
            started = true;
            
            GameManager.OnEnemyAdd += RegisterEntityTeamListener;
            
            RoomLockCheckTask().Forget();
        }

        public int EnemyCount =>
            GameManager.EntitiesReadonly.Count(ge => ge is Enemy && ge.CurrentParentIdentity == netIdentity && ge.Team == Team.Enemy);
        
        private async UniTask RoomLockCheckTask()
        {
            while (this != null)
            {
                await UniTask.WaitUntil(() => this == null || this != null && PlayerCount > 0);
                if (this != null)
                {
                    CheckShouldLockDoors();
                }
                await UniTask.WaitForFixedUpdate();
            }
        }
        
        public override void OnStartClient()
        {
            _openDoorsDictionary.OnChange += OnOpenDoorsDictionaryChange;
            Player.LocalPlayerChangeIdentity += CheckRoomVisibility;
        }
        

        private bool _initialized;
        
        private void Update()
        {
            if (!_initialized && netId != 0)
            {
                RoomManager.Instance.RoomIdentities[netId] = this;
                _initialized = true;
            }
        }

        private void RegisterEntityTeamListener(GameEntity ge)
        {
            if (ge.CurrentParentIdentity != netIdentity) return;
        }

        private bool _destroyed;

        private void OnDestroy()
        {
            _destroyed = true;
            GameManager.OnEnemyAdd -= RegisterEntityTeamListener;
            Player.LocalPlayerChangeIdentity -= CheckRoomVisibility;
        }

        //Returns a rect, representing the room's position and shape in the world based on its tilemaps
        private Rect FindRoomRect()
        {
            float xMin = 0;
            float yMin = 0;
            float xMax = 0;
            float yMax = 0;

            foreach (var tm in GetComponentsInChildren<Tilemap>())
            {
                tm.CompressBounds();
                if (xMin > tm.cellBounds.xMin) xMin = tm.cellBounds.xMin;
                if (yMin > tm.cellBounds.yMin) yMin = tm.cellBounds.yMin;
                if (xMax < tm.cellBounds.xMax) xMax = tm.cellBounds.xMax;
                if (yMax < tm.cellBounds.yMax) yMax = tm.cellBounds.yMax;
            }

            var pos = transform.position;

            return new Rect(pos.x + xMin, pos.y + yMin, xMax - xMin, yMax - yMin);
        }

        public Vector2 GetSpawnpoint(Direction fromDirection)
        {
            return fromDirection switch
            {
                Direction.Up => entryPointDown.position,
                Direction.Down => entryPointUp.position,
                Direction.Right => entryPointLeft.position,
                Direction.Left => entryPointRight.position,
                _ => transform.position
            };
        }

        [ServerCallback]
        public void
            SetOpenDirections(
                List<Direction> directions) //Opens doors based on the provided list and updates this class' open directions list.
        {
            if (this == null) return;
            _openDirections = directions;
            foreach (var d in directions)
            {
                CmdSetDoorOpen(d, true);
            }
        }

        public Vector2 GetLocalPlayerPositionNormalized()
        {
            var playerPos = Player.LocalInstance.transform.position;
            var roomRect = RoomRect;
            return new Vector2((playerPos.x-roomRect.xMin) / roomRect.width, (playerPos.y-roomRect.yMin) / roomRect.height);
        }

        public List<Direction> GetDoorDirections()
        {
            return roomDoors.Select(rd => rd.direction).ToList();
        }

        [SyncVar] public bool cleared = false;

        [ServerCallback]
        private void CheckShouldLockDoors()
        {
            if (this == null) return;

            var enemySpawners = GetComponentsInChildren<EnemySpawner>();

            var shouldLock = EnemyCount > 0
                             || enemySpawners.Any(es => es != null && es.CurrentlySpawning); //Check if there are any alive enemies in our room or if our spawner is spawning.

            if (shouldLock)
            {
                cleared = false;
                //CmdOnLock(); //Lock doors while enemies are alive in the room.
                LockAllDoors();
            }
            else
            {
                CmdOnClear();
            }
        }

        public void OnEnemySpawn()
        {
            LockAllDoors();
        }

        // [Command(requiresAuthority = false)]
        // private void CmdOnLock()
        // {
        //     if (this == null || !enabled) return;
        //
        //     cleared = false;
        // }

        [Command(requiresAuthority = false)]
        private void CmdOnClear()
        {
            if (this == null || !enabled) return;

            cleared = true;

            onRoomClearServer?.Invoke();
            RpcBroadcastClear();
            OpenAllDoors(); //Otherwise, open/close the doors as normal.
        }

        [ClientRpc]
        private void RpcBroadcastClear()
        {
            onRoomClearGlobal?.Invoke();
        }

        [ServerCallback]
        private void OpenAllDoors()
        {
            if (this == null || _openDirections == null || !enabled) return;
            foreach (var rd in roomDoors)
            {
                if (_openDirections.Contains(rd.direction)) CmdSetDoorOpen(rd.direction, true);
                else CmdSetDoorOpen(rd.direction, false);
            }
        }

        [ServerCallback]
        private void LockAllDoors() //Closes all doors
        {
            if (this == null) return;
            foreach (var rd in roomDoors)
            {
                CmdSetDoorOpen(rd.direction, false);
            }
        }

        #region Pathfinding

        public Pathfinding RoomPathfinding;

        [SerializeField] private Tilemap wallTilemap;

        private void OnEnable()
        {
            if (wallTilemap == null)
            {
                //Debug.LogWarning("No floor tilemap found!");
                return;
            }

            RoomPathfinding = new Pathfinding(wallTilemap);
        }

        #endregion
    }

    public class Pathfinding
    {
        public static Pathfinding ArenaPathfinding;

        public static void CreateArenaPathfinding(Tilemap arenaTilemap)
        {
            ArenaPathfinding = new Pathfinding(arenaTilemap);
        }

        private const int MoveStraightCost = 10;
        private const int MoveDiagonalCost = 14;

        private PathNode[,] _grid;
        private Tilemap _tilemap;

        private int tilemapWidth;
        private int tilemapHeight;

        private HashSet<PathNode> _closedNodes = new();
        
        public Pathfinding(Tilemap obstacleTilemap)
        {
            _tilemap = obstacleTilemap;

            _tilemap.CompressBounds();

            tilemapWidth = _tilemap.cellBounds.size.x;
            tilemapHeight = _tilemap.cellBounds.size.y;

            _grid = new PathNode[tilemapWidth, tilemapHeight];

            for (var x = 0; x < tilemapWidth; x++)
            {
                for (var y = 0; y < tilemapHeight; y++)
                {
                    _grid[x, y] = new PathNode(_grid, x, y);
                    
                    if (_tilemap.GetTile(new Vector3Int(x, y) + _tilemap.cellBounds.min) != null)
                    {
                        _closedNodes.Add(_grid[x, y]);
                    }
                }
            }
        }

        public List<Vector2> GetPath(Vector2 startPos, Vector2 endPos)
        {

            // Convert input coordinates into tile coordinates.
            var startTile = _tilemap.WorldToCell(startPos) - _tilemap.cellBounds.min;
            var endTile = _tilemap.WorldToCell(endPos) - _tilemap.cellBounds.min; // - _tilemap.cellBounds.min;

            var startNode = _grid[Mathf.Clamp(startTile.x, 0, _grid.GetLength(0)), Mathf.Clamp(startTile.y, 0, _grid.GetLength(1))];
            var endNode = _grid[Mathf.Clamp(endTile.x, 0, _grid.GetLength(0)), Mathf.Clamp(endTile.y, 0, _grid.GetLength(1))];

            var openList = new HashSet<PathNode>() { startNode };//(new ByFCost());
            var closedList = new HashSet<PathNode>();

            foreach (var node in _closedNodes)
            {
                closedList.Add(node);
            }

            for (var x = 0; x < tilemapWidth; x++)
            {
                for (var y = 0; y < tilemapHeight; y++)
                {
                    var pathNode = _grid[x, y];
                    pathNode.gCost = int.MaxValue;
                    pathNode.CalculateFCost();
                    pathNode.cameFromNode = null;

                    // My code
                    if (_closedNodes.Contains(pathNode))
                    {
                        if (endNode == pathNode)
                        {
                            endNode = GetNearestNeighbor(endNode, endPos);
                        }

                        if (startNode == pathNode)
                        {
                            startNode = GetNearestNeighbor(startNode, startPos);
                        }
                    }
                    // End my code
                }
            }
            
            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (openList.Count > 0)
            {
                var currentNode = LowestFCostNode(openList);
                if (currentNode == endNode)
                {
                    return CalculatePath(endNode).Select(pn =>
                        (Vector2) _tilemap.CellToWorld(new Vector3Int(pn.X, pn.Y) + _tilemap.cellBounds.min) +
                        new Vector2(0.5f, 0.5f)).ToList();
                }

                openList.Remove(currentNode);
                
                closedList.Add(currentNode);

                foreach (var neighborNode in GetNeighborList(currentNode))
                {
                    if (closedList.Contains(neighborNode)) continue;

                    var tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);
                    if (tentativeGCost < neighborNode.gCost)
                    {
                        neighborNode.cameFromNode = currentNode;
                        neighborNode.gCost = tentativeGCost;
                        neighborNode.hCost = CalculateDistanceCost(neighborNode, endNode);
                        neighborNode.CalculateFCost();

                        openList.Add(neighborNode);
                    }
                }
            }

            // Out of nodes on the open list
            return new List<Vector2>(); // No path
        }

        private PathNode GetNearestNeighbor(PathNode current, Vector2 worldPos)
        {
            return GetNeighborList(current)
                .Where(pn => _tilemap.GetTile(new Vector3Int(pn.X, pn.Y) + _tilemap.cellBounds.min) == null)
                .OrderBy(pn => Vector2.Distance(worldPos - new Vector2(0f, 0.3f),
                    (Vector2) _tilemap.CellToWorld(new Vector3Int(pn.X, pn.Y) +
                                                   _tilemap.cellBounds.min) + new Vector2(0.5f, 0.5f)))
                .FirstOrDefault();
        }

        private HashSet<PathNode> GetNeighborList(PathNode currentNode)
        {
            var neighborList = new HashSet<PathNode>();

            if (currentNode.X - 1 >= 0)
            {
                neighborList.Add(_grid[currentNode.X - 1, currentNode.Y]); // Left
                if (currentNode.Y - 1 >= 0) neighborList.Add(_grid[currentNode.X - 1, currentNode.Y - 1]); // Left down
                if (currentNode.Y + 1 < _grid.GetLength(1))
                    neighborList.Add(_grid[currentNode.X - 1, currentNode.Y + 1]); // Left up
            }

            if (currentNode.X + 1 < _grid.GetLength(0))
            {
                neighborList.Add(_grid[currentNode.X + 1, currentNode.Y]); // Right
                if (currentNode.Y - 1 >= 0) neighborList.Add(_grid[currentNode.X + 1, currentNode.Y - 1]); // Right down
                if (currentNode.Y + 1 < _grid.GetLength(1))
                    neighborList.Add(_grid[currentNode.X + 1, currentNode.Y + 1]); // Right up
            }

            if (currentNode.Y - 1 >= 0) neighborList.Add(_grid[currentNode.X, currentNode.Y - 1]); // Down
            if (currentNode.Y + 1 < _grid.GetLength(1)) neighborList.Add(_grid[currentNode.X, currentNode.Y + 1]); // Up

            return neighborList;
        }

        private List<PathNode> CalculatePath(PathNode endNode)
        {
            var path = new List<PathNode> {endNode};
            var currentNode = endNode;

            while (currentNode.cameFromNode != null)
            {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }

            path.Reverse();

            return path;
        }

        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            var xDistance = Mathf.Abs(a.X - b.X);
            var yDistance = Mathf.Abs(a.Y - b.Y);
            var remaining = Mathf.Abs(xDistance - yDistance);

            return MoveDiagonalCost * Mathf.Min(xDistance, yDistance) + MoveStraightCost * remaining;
        }

        private PathNode LowestFCostNode(HashSet<PathNode> pathNodeList)
        {
            return pathNodeList.OrderBy(pn => pn.fCost).FirstOrDefault();
        }
    }

    public class ByFCost : IComparer<PathNode>
    {
        public int Compare(PathNode x, PathNode y)
        {
            return x.fCost == y.fCost ? 0 : x.fCost > y.fCost ? 1 : -1;
        }
    }

    public class PathNode
    {
        private PathNode[,] _grid;

        public PathNode(PathNode[,] grid, int x, int y)
        {
            _grid = grid;
            X = x;
            Y = y;
        }

        public int X;
        public int Y;

        public int gCost;
        public int hCost;
        public int fCost;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        public PathNode cameFromNode;
    }
}