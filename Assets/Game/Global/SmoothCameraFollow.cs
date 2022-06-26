using System;
using TonyDev.Game.Core.Entities.Player;
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
    
        [NonSerialized] public Rect CameraBounds = Rect.zero;
        private float _z;
        private Transform _playerTransform;
    
        private void Start()
        {
            _playerTransform = Player.Instance.transform; //Initialize the playerTransform variable
            _z = transform.position.z; //Set the initial z level of the camera so it can be kept constant
            SceneManager.sceneLoaded += ClearCameraBounds; //Set ClearCameraBounds to be called every time a new scene is loaded.
        }

        private void ClearCameraBounds(Scene scene, LoadSceneMode loadSceneMode)
        {
            CameraBounds = Rect.zero; //Clears the camera bounds
        }

        private void Update()
        {
            var newPos = Vector2.Lerp(transform.position, _playerTransform.position, followSpeed * Time.deltaTime); //Lerp towards the player's position
            if (CameraBounds != Rect.zero) //If CameraBounds is not cleared,
            {
                //Do some math to figure out the bounds of the camera in world coordinates
                var cameraRect = camera.pixelRect;
                var cameraTopRight = camera.ScreenToWorldPoint(new Vector2(cameraRect.xMax, cameraRect.yMax));
                var cameraBottomLeft = camera.ScreenToWorldPoint(Vector3.zero);
                var cameraWidthInWorldCoords = cameraTopRight.x - cameraBottomLeft.x;
                var cameraHeightInWorldCoords = cameraTopRight.y - cameraBottomLeft.y;
                //
            
                //Clamp the camera's position based on the bounds, offset to be based on the camera's border, not the camera's center.
                newPos.x = Mathf.Clamp(newPos.x, CameraBounds.x + cameraWidthInWorldCoords / 2, CameraBounds.x + CameraBounds.width - cameraWidthInWorldCoords / 2);
                newPos.y = Mathf.Clamp(newPos.y, CameraBounds.y + cameraHeightInWorldCoords / 2, CameraBounds.y + CameraBounds.height - cameraHeightInWorldCoords / 2);
                //
            }

            transform.position = new Vector3(newPos.x, newPos.y, _z); //Update the position
        }
    }
}
