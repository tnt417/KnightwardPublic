using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.Level.Rooms
{
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance; //Singleton instance
        
        //Editor variables
        [SerializeField] private Room currentActiveRoom;
        [SerializeField] private RoomGenerator roomGenerator;
        [SerializeField] private GameObject minimapObject;
        //
        
        private SmoothCameraFollow _smoothCameraFollow;
        
        //Room-related variables
        public int MapSize => roomGenerator.MapSize; //The shared width/height of the map
        private Vector2Int _currentActiveRoomIndex;
        public Room[,] Rooms { get; private set; }
        public bool InStartingRoom => _currentActiveRoomIndex == Vector2Int.one * roomGenerator.mapRadius;
        //
        
        private void Awake()
        {
            //Singleton code
            if (Instance == null && Instance != this) Instance = this;
            else Destroy(this);
            //
        }
        private void Start()
        {
            _smoothCameraFollow = FindObjectOfType<SmoothCameraFollow>(); //Initialize the camera follow variables
            StartRoomPhase(); //Run starting code
        }

        private void Update()
        {
            minimapObject.SetActive(GameManager.GamePhase == GamePhase.Dungeon);
            MinimapManager.Instance.SetPlayerPosition(Player.Instance.transform.position, roomGenerator.roomOffset);
        }
    
        private void StartRoomPhase()
        {
            Rooms = roomGenerator.Generate(); //Randomly generate rooms
            DoDoorClosing(); //Close doors that need closing
            MinimapManager.Instance.UpdateMinimap(); //Update the minimap now that we generated rooms
        }

        public void ShiftActiveRoom(Direction direction) //Moves a player to the next room in a direction.
        {
            Player.Instance.playerMovement.DoMovement = false;
            StartCoroutine(MovePlayerToNextRoom(direction));
        }

        private IEnumerator MovePlayerToNextRoom(Direction direction) //Moves a player to the next room in a direction.
        {
            TransitionController.Instance.FadeInOut(); //Transition to make it less jarring.
            yield return new WaitUntil(() => TransitionController.Instance.OutTransitionDone); //Wait until the transition is over
        
            var dx = 0; //delta x
            var dy = 0; //delta y
        
            switch (direction) //Set the delta variables based on the direction
            {
                case Direction.Left:
                    dx = -1;
                    break;
                case Direction.Right:
                    dx = 1;
                    break;
                case Direction.Up:
                    dy = 1;
                    break;
                case Direction.Down:
                    dy = -1;
                    break;
            }
        
            SetActiveRoom(_currentActiveRoomIndex.x + dx, _currentActiveRoomIndex.y + dy); //Set the active room to the new room
        
            //Move the player into the newly activated room
            var playerTransform = Player.Instance.transform;
            playerTransform.transform.Translate(dx * 4, dy * 4, 0);
            Player.Instance.playerMovement.DoMovement = true;
            //
        }

        public void SetActiveRoom(int x, int y)
        {
            var newRoom = Rooms[x, y]; //Get the new room from the array
            if (newRoom == null) return; //If it's null, do nothing
        
            if(currentActiveRoom != null) currentActiveRoom.gameObject.SetActive(false); //Deactivate the current room
            currentActiveRoom = newRoom; //Update currentActiveRoom variable
            currentActiveRoom.gameObject.SetActive(true); //Activate the new room
            MinimapManager.Instance.UncoverRoom(new Vector2Int(x, y)); //Uncover the room on the minimap
            _smoothCameraFollow.CameraBounds = currentActiveRoom.RoomRect; //Update the camera bounds
            _currentActiveRoomIndex = new Vector2Int(x, y); //Update currentActiveRoomIndex variable
        }

        //Performs all actions necessary to disable the room phase.
        public void DeactivateRoomPhase()
        {
            _currentActiveRoomIndex = Vector2Int.zero;
            if(_smoothCameraFollow != null) _smoothCameraFollow.CameraBounds = Rect.zero;
            if (currentActiveRoom != null)
            {
                currentActiveRoom.gameObject.SetActive(false);
                currentActiveRoom = null;
            }
        }

        private void DoDoorClosing()
        {
            for (var i = 0; i < roomGenerator.MapSize; i++) //Loop through all the rooms
            {
                for (var j = 0; j < roomGenerator.MapSize; j++)
                {
                    if (Rooms[i, j] == null) continue; //If there's no room, move on
                
                    //Set open directions based on room adjacency
                    var openDirections = new List<Direction>();
                    if(CheckIfRoomAtPos(i-1, j)) openDirections.Add(Direction.Left);
                    if(CheckIfRoomAtPos(i+1, j)) openDirections.Add(Direction.Right);
                    if(CheckIfRoomAtPos(i, j-1)) openDirections.Add(Direction.Down);
                    if(CheckIfRoomAtPos(i, j+1)) openDirections.Add(Direction.Up);
                    Rooms[i, j].SetOpenDirections(openDirections);
                    //
                }
            }
        }

        private bool CheckIfRoomAtPos(int x, int y) //Returns whether or not there is a room at the grid position provided.
        {
            if (x < 0 || y < 0 || x > roomGenerator.MapSize-1 || y > roomGenerator.MapSize-1) return false;
            return Rooms[x, y] != null;
        }
    }
}
