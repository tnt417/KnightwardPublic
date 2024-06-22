using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Global
{
    public class SmoothCameraFollow : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private Animator animator;
        [SerializeField] private float followSpeed;
        [SerializeField] private AudioSource shakeSound;

        [FormerlySerializedAs("camera")] [SerializeField]
        private Camera cam;
        //

        private Rect _cameraBounds = Rect.zero;
        private float _z;
        private static Transform PlayerTransform => Player.LocalInstance.transform;

        private static SmoothCameraFollow _instance;

        private void Awake()
        {
            _instance = this;
            
            ShakeMultiplier = PlayerPrefs.GetFloat("shake", 0.5f);
        }

        private void Start()
        {
            _z = transform.position.z; //Set the initial z level of the camera so it can be kept constant
            SceneManager.sceneLoaded +=
                ClearCameraBounds; //Set ClearCameraBounds to be called every time a new scene is loaded.
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

            return new Rect(center.x - width / 2, center.y - height / 2, width, height);
        }

        private void ClearCameraBounds(Scene scene, LoadSceneMode loadSceneMode)
        {
            _cameraBounds = Rect.zero; //Clears the camera bounds
        }

        private Vector2 _crystalPos;

        private bool _viewCrystal = false;

        public void OnViewCrystal(InputValue value)
        {
            if (!GameManager.GameControlsActive)
            {
                _viewCrystal = false;
                return;
            }

            _viewCrystal = value.isPressed;
        }

        private bool _viewedCrystalLast;

        public static float ShakeMultiplier = 0.5f;

        public static void SetShakeMultiplier(float newValue)
        {
            ShakeMultiplier = newValue;
            PlayerPrefs.SetFloat("shake", newValue);
        }
            
        public static void Shake(float intensity, float speed)
        {
            _instance.animator.speed = speed;
            _instance.animator.transform.localScale = intensity * Vector3.one * ShakeMultiplier;
            _instance.animator.Play("CameraShake");
            _instance.shakeSound.volume = intensity / 50 + Random.Range(0f, 0.1f);
            _instance.shakeSound.pitch = Mathf.Clamp01(speed / 4f) + 0.8f + Random.Range(-0.1f, 0.1f);
            _instance.shakeSound.Play();
        }

        private Vector3 _cameraOffset = Vector2.zero;

        private Vector2 _lagBehind = Vector2.zero;

        private void Update()
        {
            if (Player.LocalInstance == null) return;

            if (_crystalPos == default && Crystal.Instance != null) _crystalPos = Crystal.Instance.transform.position;

            var playerSpeed = Player.LocalInstance.Stats.GetStat(Stat.MoveSpeed);

            var playerInput = Player.LocalInstance.playerMovement.currentMovementInput;

            _lagBehind += playerInput * Time.deltaTime * playerSpeed + GameManager.MouseDirection.normalized * Time.deltaTime;

            _lagBehind = Vector2.ClampMagnitude(_lagBehind, 0.1f * playerSpeed);

            var playerPos = (Vector2) PlayerTransform.position + _lagBehind;

            if (_fixateNext)
            {
                var fixatePos = new Vector3(
                    playerPos.x,
                    playerPos.y, _z);
                transform.position = fixatePos;
                _fixateNext = false;
                return;
            }

            var trackCrystal = _viewCrystal || GameManager.Instance.doCrystalFocusing;
            Vector2 newPos;
            if (GameManager.GamePhase == GamePhase.Dungeon &&
                trackCrystal) //This is done to prevent me from being dizzy.
            {
                newPos = _crystalPos;
            }
            else
                newPos = Vector2.Lerp(transform.position - _cameraOffset,
                    trackCrystal
                        ? _crystalPos
                        : playerPos, followSpeed * Time.deltaTime); //Lerp towards the player's position. Or the crystal's if the key is held.

            if (GameManager.GamePhase == GamePhase.Dungeon && _viewedCrystalLast &&
                !trackCrystal) //This is done to prevent me from being dizzy.
            {
                newPos = playerPos;
            }

            _viewedCrystalLast = trackCrystal;

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
                newPos.x = Mathf.Clamp(newPos.x, _cameraBounds.x + cameraWidthInWorldCoords / 2,
                    _cameraBounds.x + _cameraBounds.width - cameraWidthInWorldCoords / 2);
                newPos.y = Mathf.Clamp(newPos.y, _cameraBounds.y + cameraHeightInWorldCoords / 2,
                    _cameraBounds.y + _cameraBounds.height - cameraHeightInWorldCoords / 2);
                //
            }

            transform.position = new Vector3(newPos.x, newPos.y, _z) + _cameraOffset; //Update the position
        }
    }
}