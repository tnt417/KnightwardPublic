using System;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace TonyDev.Game.Global
{
    public class SmoothCameraFollow : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private float followSpeed;
        [FormerlySerializedAs("camera")] [SerializeField] private Camera cam;
        //
    
        private Rect _cameraBounds = Rect.zero;
        private float _z;
        private static Transform PlayerTransform => Player.LocalInstance.transform;
        private static Transform CrystalTransform => Crystal.Instance.transform;
    
        private void Start()
        {
            _z = transform.position.z; //Set the initial z level of the camera so it can be kept constant
            SceneManager.sceneLoaded += ClearCameraBounds; //Set ClearCameraBounds to be called every time a new scene is loaded.
            _resolution = new Vector2Int(Screen.width, Screen.height);
        }

        private Vector2Int _resolution;

        private void LateUpdate()
        {
            if (_resolution.x != Screen.width || _resolution.y != Screen.height)
            {
                SetCameraBounds(_cameraBounds); //Re-set on resolution change
 
                _resolution.x = Screen.width;
                _resolution.y = Screen.height;
            }
        }

        public void SetCameraBounds(Rect bounds)
        {
            //if it is bigger than the camera or zero, set it
            if (bounds == Rect.zero)
            {
                _cameraBounds = bounds;
                return;
            }
            
            //otherwise center it...
            _cameraBounds = FixCameraRect(bounds);
        }

        private bool _fixateNext = true;
        
        public void FixateOnPlayer()
        {
            _fixateNext = true;
        }
        
        private Rect FixCameraRect(Rect rect)
        {
            if (rect == Rect.zero) return Rect.zero;
            
            //Do some math to figure out the bounds of the camera in world coordinates
            var cameraRect = cam.pixelRect;
            var cameraTopRight = cam.ScreenToWorldPoint(new Vector2(cameraRect.xMax, cameraRect.yMax));
            var cameraBottomLeft = cam.ScreenToWorldPoint(Vector3.zero);
            var cameraWidthInWorldCoords = cameraTopRight.x - cameraBottomLeft.x;
            var cameraHeightInWorldCoords = cameraTopRight.y - cameraBottomLeft.y;
            //
            
            var center = rect.center;
            var width = rect.width > cameraWidthInWorldCoords ? rect.width : cameraWidthInWorldCoords;
            var height = rect.height > cameraHeightInWorldCoords ? rect.height : cameraHeightInWorldCoords;

            return new Rect(center.x - width/2, center.y - height/2, width, height);
        }
        
        private void ClearCameraBounds(Scene scene, LoadSceneMode loadSceneMode)
        {
            _cameraBounds = Rect.zero; //Clears the camera bounds
        }

        private void Update()
        {

            if (Player.LocalInstance == null) return;

            if (_fixateNext)
            {
                var fixatePos = new Vector3(PlayerTransform.position.x, PlayerTransform.position.y, _z);
                transform.position = fixatePos;
                _fixateNext = false;
                return;
            }
            
            var trackCrystal = Input.GetKey(KeyCode.LeftAlt);
            Vector2 newPos;
            if (GameManager.GamePhase == GamePhase.Dungeon && trackCrystal) //This is done to prevent me from being dizzy.
            {
                newPos = CrystalTransform.position;
            }else newPos = Vector2.Lerp(transform.position, trackCrystal ? CrystalTransform.position : PlayerTransform.position
                , followSpeed * Time.deltaTime); //Lerp towards the player's position. Or the crystal's if the key is held.

            if (GameManager.GamePhase == GamePhase.Dungeon && Input.GetKeyUp(KeyCode.LeftAlt)) //This is done to prevent me from being dizzy.
            {
                newPos = PlayerTransform.position;
            }

            if (_cameraBounds != Rect.zero && !trackCrystal) //If CameraBounds is not cleared,
            {
                //Do some math to figure out the bounds of the camera in world coordinates
                var cameraRect = cam.pixelRect;
                var cameraTopRight = cam.ScreenToWorldPoint(new Vector2(cameraRect.xMax, cameraRect.yMax));
                var cameraBottomLeft = cam.ScreenToWorldPoint(Vector3.zero);
                var cameraWidthInWorldCoords = cameraTopRight.x - cameraBottomLeft.x;
                var cameraHeightInWorldCoords = cameraTopRight.y - cameraBottomLeft.y;
                //
            
                //Clamp the camera's position based on the bounds, offset to be based on the camera's border, not the camera's center.
                newPos.x = Mathf.Clamp(newPos.x, _cameraBounds.x + cameraWidthInWorldCoords / 2, _cameraBounds.x + _cameraBounds.width - cameraWidthInWorldCoords / 2);
                newPos.y = Mathf.Clamp(newPos.y, _cameraBounds.y + cameraHeightInWorldCoords / 2, _cameraBounds.y + _cameraBounds.height - cameraHeightInWorldCoords / 2);
                //
            }

            transform.position = new Vector3(newPos.x, newPos.y, _z); //Update the position
        }
    }
}
