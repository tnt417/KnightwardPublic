using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Level.Decorations.Crystal;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace TonyDev.Game.Global
{
    public class SmoothCameraFollow : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private Animator animator;
        [SerializeField] private float followSpeed;

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

        private CancellationTokenSource _source = new CancellationTokenSource();
        
        public static void Shake(float intensity, float speed)
        {
            _instance.animator.Play("CameraShake");
            return;
            _instance._source.Cancel();
            _instance.ShakeTask(intensity, speed).AttachExternalCancellation(_instance._source.Token);
        }

        private Vector3 _cameraOffset = Vector2.zero;

        private async UniTask ShakeTask(float intensity, float speed)
        {
            var origPos = Vector3.zero;
            _cameraOffset = origPos;

            Debug.Log("A");

            var topRightTarget = origPos + new Vector3(intensity, intensity);
            while (_cameraOffset != topRightTarget)
            {
                _cameraOffset = Vector3.MoveTowards(_cameraOffset, topRightTarget,
                    speed * Time.deltaTime * intensity);
                await UniTask.DelayFrame(1);
            }

            Debug.Log("B");

            var botRightTarget = origPos + new Vector3(intensity, -intensity);
            while (_cameraOffset != botRightTarget)
            {
                _cameraOffset = Vector3.MoveTowards(_cameraOffset, botRightTarget,
                    speed * Time.deltaTime * intensity);
                await UniTask.DelayFrame(1);
            }

            Debug.Log("C");

            var botLeftTarget = origPos + new Vector3(-intensity, -intensity);
            while (_cameraOffset != botLeftTarget)
            {
                _cameraOffset = Vector3.MoveTowards(_cameraOffset, botLeftTarget,
                    speed * Time.deltaTime * intensity);
                await UniTask.DelayFrame(1);
            }

            Debug.Log("D");

            var topLeftTarget = origPos + new Vector3(-intensity, intensity);
            while (_cameraOffset != topLeftTarget)
            {
                _cameraOffset = Vector3.MoveTowards(_cameraOffset, topLeftTarget,
                    speed * Time.deltaTime * intensity);
                await UniTask.DelayFrame(1);
            }

            Debug.Log("E");

            while (_cameraOffset != origPos)
            {
                _cameraOffset = Vector3.MoveTowards(_cameraOffset, origPos, speed * Time.deltaTime * intensity);
                await UniTask.DelayFrame(1);
            }
        }

        private void Update()
        {
            if (Player.LocalInstance == null) return;

            if (_crystalPos == default && Crystal.Instance != null) _crystalPos = Crystal.Instance.transform.position;

            if (_fixateNext)
            {
                var fixatePos = new Vector3(PlayerTransform.position.x, PlayerTransform.position.y, _z);
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
                    trackCrystal ? _crystalPos : PlayerTransform.position
                    , followSpeed * Time.deltaTime); //Lerp towards the player's position. Or the crystal's if the key is held.

            if (GameManager.GamePhase == GamePhase.Dungeon && _viewedCrystalLast &&
                !trackCrystal) //This is done to prevent me from being dizzy.
            {
                newPos = PlayerTransform.position;
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