using System;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.Global
{
    public class SmoothCameraFollow : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private float followSpeed;
        [SerializeField] private new Camera camera;
        //
    
        private Rect _cameraBounds = Rect.zero;
        private float _z;
        private Transform _playerTransform;
        private Transform _crystalTransform;
    
        private void Start()
        {
            _playerTransform = Player.Instance.transform; //Initialize the playerTransform variable
            _z = transform.position.z; //Set the initial z level of the camera so it can be kept constant
            SceneManager.sceneLoaded += ClearCameraBounds; //Set ClearCameraBounds to be called every time a new scene is loaded.
            _crystalTransform = FindObjectOfType<Crystal>().transform; //Set our crystal object so we can watch it
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

        private Rect FixCameraRect(Rect rect)
        {
            
            //Do some math to figure out the bounds of the camera in world coordinates
            var cameraRect = camera.pixelRect;
            var cameraTopRight = camera.ScreenToWorldPoint(new Vector2(cameraRect.xMax, cameraRect.yMax));
            var cameraBottomLeft = camera.ScreenToWorldPoint(Vector3.zero);
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
            var trackCrystal = Input.GetKey(KeyCode.LeftAlt);
            Vector2 newPos;
            if (GameManager.GamePhase == GamePhase.Dungeon && trackCrystal) //This is done to prevent me from being dizzy.
            {
                newPos = _crystalTransform.position;
            }else newPos = Vector2.Lerp(transform.position, trackCrystal ? _crystalTransform.position : _playerTransform.position
                , followSpeed * Time.deltaTime); //Lerp towards the player's position. Or the crystal's if the key is held.

            if (GameManager.GamePhase == GamePhase.Dungeon && Input.GetKeyUp(KeyCode.LeftAlt)) //This is done to prevent me from being dizzy.
            {
                newPos = _playerTransform.position;
            }

            if (_cameraBounds != Rect.zero && !trackCrystal) //If CameraBounds is not cleared,
            {
                //Do some math to figure out the bounds of the camera in world coordinates
                var cameraRect = camera.pixelRect;
                var cameraTopRight = camera.ScreenToWorldPoint(new Vector2(cameraRect.xMax, cameraRect.yMax));
                var cameraBottomLeft = camera.ScreenToWorldPoint(Vector3.zero);
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
