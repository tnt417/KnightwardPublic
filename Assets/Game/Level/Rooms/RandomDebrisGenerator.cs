using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace TonyDev
{
    public class RandomDebrisGenerator : MonoBehaviour
    {
        // Crates:
        // Crates should be put mostly near walls, even more so in corners
        // Crates of similar types should be closer to each other but can still mix and match
        // Cluster of crates should have 1-3 crates
        // Can also have treasure mixed in with crates, though less common
        // Gore can be more in the middle of the room

        public GameObject[] possibleCrates;
        public GameObject[] possibleTreasure;
        public GameObject[] possibleGore;

        public Tilemap wallTilemap;
        public Tilemap floorTilemap;

        public int islandGenerations = 10;
        public int islandSize = 2;
        public float generateProbability = 0.4f;
        
        private void Start()
        {
            wallTilemap.CompressBounds();

            var localBounds = wallTilemap.localBounds;
            var tmPos = wallTilemap.transform.position;

            for (var i = 0; i < islandGenerations; i++)
            {
                var x = (int)(Random.Range(localBounds.min.x, localBounds.max.x) + tmPos.x);
                var y = (int)(Random.Range(localBounds.min.y, localBounds.max.y) + tmPos.y);

                var circleCoords = GetCircleCoords(new Vector2Int(x, y), islandSize);

                var chosenWallPoint = new List<Vector2Int>();
                
                foreach (var point in circleCoords)
                {
                    var tile = wallTilemap.GetTile(wallTilemap.WorldToCell(new Vector3(point.x, point.y, 0)));
                    
                    if (tile != null)
                    {
                        chosenWallPoint.Add(point);
                    }
                }

                foreach (var wallPoint in chosenWallPoint)
                {

                    if (Random.Range(0f, 1f) > generateProbability) continue;

                    var worldToCell = floorTilemap.WorldToCell(new Vector3(wallPoint.x, wallPoint.y, 0));
                    
                    var tile = floorTilemap.GetTile(worldToCell);

                    if (tile != null)
                    {
                        var floorPos = floorTilemap.CellToWorld(worldToCell);
                        
                        var newX = floorPos.x + Random.Range(0.0f, 0.2f) * (Random.Range(0f, 1f) > 0.5f ? -1f : 1f);
                        var newY = floorPos.y + Random.Range(0.0f, 0.2f) * (Random.Range(0f, 1f) > 0.5f ? -1f : 1f);

                        Instantiate(possibleCrates[0], new Vector2(newX + 0.5f, newY + 0.5f), Quaternion.identity);
                    }
                }
            }
        }
        
        private List<Vector2Int> GetCircleCoords(Vector2Int center, int radius)
        {
            var tiles = new List<Vector2Int>();

            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    var tilePosition = center + new Vector2Int(x, y);
                    
                    if (Vector2.Distance(center, tilePosition) <= radius)
                    {
                        tiles.Add(tilePosition);
                    }
                }
            }
            return tiles;
        }
    }
}
