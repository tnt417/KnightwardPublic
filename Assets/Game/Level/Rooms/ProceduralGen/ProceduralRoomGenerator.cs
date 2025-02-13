using System;
using System.Collections.Generic;
using Mirror.BouncyCastle.Utilities;
using TonyDev.Game.Global;
using Unity.Mathematics.Geometry;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Math = System.Math;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Rooms.ProceduralGen
{
    [ExecuteInEditMode]
    public class ProceduralRoomGenerator : MonoBehaviour
    {
        public ProceduralGenSettings defaultSettings;
        public GameObject emptyRoomPrefab;

        private GameObject _generatedRoomObject = null;
        
        private const int MAX_TILES_X = 100;
        private const int MAX_TILES_Y = 100;

        public int floorGenIterations = 1;
        
        [ExecuteInEditMode]
        public void Generate(ProceduralGenSettings settings, Vector2 spawnPos)
        {
            if (_generatedRoomObject != null)
            {
                DestroyImmediate(_generatedRoomObject);
            }

            //Random.InitState(0);
            
            // Generate random weights based on provided settings
            var curWeights = GenWeights.GenerateRandom(settings.weightsBounds);
            
            _generatedRoomObject = Instantiate(emptyRoomPrefab, spawnPos, Quaternion.identity);
            
            var references = _generatedRoomObject.GetComponent<RoomGenReferences>();

            // Step A: Creates basic floor based on weights
            CreateFloor(references.floorTilemap, curWeights);
            
            references.floorTilemap.RefreshAllTiles();
        }

        [ExecuteInEditMode]
        public void Regen()
        {
            Generate(defaultSettings, Vector2.zero);
        }

        private Vector3 _gizmoPos = Vector3.zero;
        
        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(_gizmoPos, 0.4f);
        }

        #region Floor Shape
        
        private void CreateFloor(Tilemap floorTm, GenWeights curWeights)
        {
            // may need to store last generated shape for path purposes

            // Iteratively generate, returning based on the size weight
            
            var genIterations = 0;
            var genShapeLast = false;
            var curInterestPoint = new Vector2Int(0, 0);

            _gizmoPos = floorTm.GetCellCenterWorld((Vector3Int)curInterestPoint);
            
            var lastSize = new Vector2Int(0, 0);
            while (true)
            {
                // Alternate between generating shapes (rooms) and hallways
                if (!genShapeLast)
                {
                    var genRounded = Random.Range(0f, 1f) > curWeights.roomRigidity;

                    // Combine weights together to form a composite for room shape generation
                    var compositeRoomSizeWeight = curWeights.roomOpenness * curWeights.roomArea;
                    var compositeRoomSizeWeightX = curWeights.roomHorizontalBias * compositeRoomSizeWeight;
                    var compositeRoomSizeWeightY = curWeights.roomVerticalBias * compositeRoomSizeWeight;

                    if (genRounded)
                    {
                        var radius = (int)(Random.Range(6f, 9f) * compositeRoomSizeWeight);

                        lastSize = Vector2Int.one * radius;

                        InsertCircle(floorTm, defaultSettings.baseFloorTile, curInterestPoint, radius);
                    }
                    else
                    {
                        var width = (int)(Random.Range(20f, 30f) * compositeRoomSizeWeightX);
                        var height = (int)(Random.Range(20f, 30f) * compositeRoomSizeWeightY);

                        lastSize = new Vector2Int(width, height);

                        InsertRect(floorTm, defaultSettings.baseFloorTile, curInterestPoint, lastSize);
                    }

                    genShapeLast = true;
                }
                else
                {
                    // Hall generation:
                    // Randomize distance and direction (with a chance to just go straight in one of four directions)
                    // in radians, then randomize whether we got left then up or up the left
                    // Once we reach this spot, we will generate a new shape
                    var hallDist = (int)(Random.Range(9f, 18f) * curWeights.roomSprawl);
                    Vector2 hallDir;
                    
                    // TODO: 50/50 chance right now to do an angle vs a cardinal direction: make setting in future
                    if (Random.Range(0, 1f) > 0.5f)
                    {
                        hallDir = GameTools.Rotate(Vector2.right, Random.Range(0, 2 * Mathf.PI));
                    }
                    else
                    {
                        var cardinal = Random.Range(0, 4);
                        
                        var cardinalRadians = cardinal switch
                        {
                            0 => 0f,
                            1 => 0.5f * Mathf.PI,
                            2 => Mathf.PI,
                            3 => 1.5f * Mathf.PI,
                            _ => 0f
                        };
                        
                        hallDir = GameTools.Rotate(Vector2.right, cardinalRadians);
                    }
                    
                    Debug.Log("Hall dist" + hallDist + "Hall dir" + hallDir);

                    var endPointNonInt = (curInterestPoint + hallDir * hallDist);
                    var endPoint = new Vector2Int((int)endPointNonInt.x, (int)endPointNonInt.y);

                    var hallThickness = Mathf.Lerp(2f, 4f, curWeights.roomOpenness * curWeights.roomArea);
                    
                    // Crawl in x direction
                    while(Math.Abs(curInterestPoint.x - endPoint.x) > 0)
                    {
                        curInterestPoint += (endPoint.x > curInterestPoint.x ? Vector2Int.right : Vector2Int.left);
                        
                        for (var i = 0; i < hallThickness; i++)
                        {
                            var offset = (i % 2 == 0 ? -1 : 1) * new Vector2Int(0, i/2);
                            floorTm.SetTile((Vector3Int) (curInterestPoint + offset), defaultSettings.baseFloorTile);
                        }
                    }
                    
                    // Crawl in y direction
                    while(Math.Abs(curInterestPoint.y - endPoint.y) > 0)
                    {
                        curInterestPoint += (endPoint.y > curInterestPoint.y ? Vector2Int.up : Vector2Int.down);

                        for (var i = 0; i < hallThickness; i++)
                        {
                            var offset = (i % 2 == 0 ? -1 : 1) * new Vector2Int(i/2, 0);
                            floorTm.SetTile((Vector3Int) (curInterestPoint + offset), defaultSettings.baseFloorTile);
                        }
                    }
                    
                    genShapeLast = false;
                }
                
                genIterations++;
                
                if (genIterations % 2 == 1) // Only return on odd iterations to prevent dead end hallways
                {
                    var threshold = Mathf.Lerp(1f, 0.1f, Mathf.InverseLerp(0f, 1f, curWeights.roomArea)); 
                    if (Random.Range(0f, 1f) > threshold * (1f - (genIterations / 10f)))
                    {
                        return;
                    }
                }

            }
        }

        
        // Sets tiles in a rect for the provided Tilemap
        private void InsertRect(Tilemap input, TileBase tile, Vector2Int center, Vector2Int dimensions)
        {
            var radiusX = dimensions.x / 2;
            
            var startPointX = center.x - radiusX;
            var endPointX = startPointX + dimensions.x;
            
            var radiusY = dimensions.y / 2;
            
            var startPointY = center.y - radiusY;
            var endPointY = startPointY + dimensions.y;

            for (var x = startPointX; x < endPointX; x++)
            {
                for (var y = startPointY; y < endPointY; y++)
                {
                    input.SetTile(new Vector3Int(x, y, 0), tile);
                }   
            }
        }
        
        // Sets tiles in a circle for the provided Tilemap
        private void InsertCircle(Tilemap input, TileBase tile, Vector2Int center, int r) //Vector2Int dimensions)
        {
            var radius = new Vector2Int(r, r);
            
            var radiusX = radius.x;
            
            var startPointX = center.x - radiusX;
            var endPointX = startPointX + 2*radiusX;
            
            var radiusY = radius.y;
            
            var startPointY = center.y - radiusY;
            var endPointY = startPointY + 2*radiusY;

            for (var x = startPointX; x < endPointX; x++)
            {
                for (var y = startPointY; y < endPointY; y++)
                {
                    if (Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center) <= r)
                    {
                        input.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                }   
            }
        }
        
        #endregion
        
        #region Walls

        private void AddBorderWalls(Tilemap wallTm, Tilemap floorTm)
        {
            
        }
        
        #endregion
    }
}
