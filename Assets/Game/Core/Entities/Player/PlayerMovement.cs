using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;

namespace TonyDev.Game.Core.Entities.Player
{
    public struct GameForce //Custom force implementation to allow more specific control of the player's movement.
    {
        public GameForce(Vector2 direction, float force, float unitsRemaining)
        {
            InitialUnits = unitsRemaining;
            UnitsRemaining = unitsRemaining;
            InitialForce = force;
            Force = force;
            Direction = direction;
        }

        public readonly Vector2 Direction; //Direction of the force
        public readonly float InitialForce; //The force at the creation of the force
        public float Force; //The current force
        public float UnitsRemaining; //The units remaining until the force is removed
        public readonly float InitialUnits; //The UnitsRemaining at the creation of the force
    }
    
    public class PlayerMovement : NetworkBehaviour
    {
        //Editor variables
        [SerializeField] private Rigidbody2D rb2D;

        [SerializeField] private float speedMultiplier;
        //

        [NonSerialized] public bool DoMovement = true;

        public Action<Vector2> OnPlayerMove;

        public Vector2 currentMovementInput;

        private List<GameForce> _forceVectors = new();

        private void FixedUpdate()
        {
            if (!DoMovement || !isOwned) return;

            //dx and dy are either 1, 0, or -1 depending on keys being pressed
            var dx = _overrideInput != Vector2.zero ? _overrideInput.x : currentMovementInput.x; //(Input.GetKey(KeyCode.A) && GameManager.GameControlsActive ? -1 : 0) +
            //(Input.GetKey(KeyCode.D) && GameManager.GameControlsActive ? 1 : 0);
            var dy = _overrideInput != Vector2.zero ? _overrideInput.y : currentMovementInput.y; //(Input.GetKey(KeyCode.S) && GameManager.GameControlsActive ? -1 : 0) +
            //(Input.GetKey(KeyCode.W) && GameManager.GameControlsActive ? 1 : 0);

            //Dampen forces over time according to a curve. Force has a floor to ensure that it always completes its path. Trim forces with no units remaining.
            _forceVectors = _forceVectors.Select(x =>
            {
                x.Force = x.InitialForce *
                          Mathf.Clamp(-Mathf.Pow(2f * (x.UnitsRemaining / x.InitialUnits) - 1f, 16f) + 1f, 0.2f,
                              x.InitialForce);
                x.UnitsRemaining -= x.Force * Time.fixedDeltaTime;
                return x;
            }).Where(x => x.UnitsRemaining > 0).ToList();

            //Sum force times direction for every force. Added to player movement.
            var forceSum = _forceVectors.Any()
                ? _forceVectors.Select(x => x.Force * x.Direction).Aggregate((x, y) => x + y)
                : Vector2.zero;

            var movement = /*Time.fixedDeltaTime **/ (new Vector2(dx, dy).normalized * GetSpeedMultiplier() + forceSum);

            //Move the rigidbody to the correct position
            //rb2D.MovePosition((Vector2) transform.position + movement);

            rb2D.AddForce(movement);
            
            OnPlayerMove?.Invoke(movement);

            if (Player.LocalInstance.playerAnimator == null) return;

            //Set animation directions based on the keys that were pressed.
            if (dy > 0) Player.LocalInstance.playerAnimator.PlayerAnimState = PlayerAnimState.Up;
            if (dy < 0) Player.LocalInstance.playerAnimator.PlayerAnimState = PlayerAnimState.Down;
            if (dx > 0) Player.LocalInstance.playerAnimator.PlayerAnimState = PlayerAnimState.Right;
            if (dx < 0) Player.LocalInstance.playerAnimator.PlayerAnimState = PlayerAnimState.Left;
            if (dy == 0 && dx == 0) Player.LocalInstance.playerAnimator.PlayerAnimState = PlayerAnimState.Idle;
            //
        }

        private Vector2 _overrideInput = Vector2.zero;
        
        public void OnMove(InputValue value)
        {
            if (!GameManager.GameControlsActive)
            {
                currentMovementInput = Vector2.zero;
                return;
            }
            if (!isOwned) return;
            currentMovementInput = value.Get<Vector2>();
        }

        public void Dash(float distance, Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                direction = currentMovementInput.normalized;
            }
            AddForce(direction, distance);
        }

        public void BetweenRoomMove(Direction direction)
        {
            BetweenRoomMoveTask(direction).Forget();
        }
        
        private async UniTask BetweenRoomMoveTask(Direction direction)
        {
            var startRoom = RoomManager.Instance.currentActiveRoom;

            RoomManager.Instance.stopRoomChange = true;
            
            _overrideInput = direction switch
            {
                Direction.Up => Vector2.up,
                Direction.Down => Vector2.down,
                Direction.Left => Vector2.left,
                Direction.Right => Vector2.right,
                _ => Vector2.zero
            };

            await UniTask.WaitUntil(() => RoomManager.Instance.currentActiveRoom != startRoom);

            float dist = 0;
            Vector2 lastPos = Player.LocalInstance.transform.position;

            await UniTask.WaitUntil(() =>
            {
                Vector2 curPos = Player.LocalInstance.transform.position;

                dist += Vector2.Distance(lastPos, curPos);

                lastPos = curPos;

                return dist >= 2.0f;
            });

            _overrideInput = Vector2.zero;

            RoomManager.Instance.stopRoomChange = false;
        }

        private void AddForce(Vector2 direction, float units)
        {
            _forceVectors.Add(new GameForce(direction, 15f, units));
        }

        public float GetSpeedMultiplier()
        {
            return speedMultiplier * PlayerStats.Stats.GetStat(Stat.MoveSpeed);
        }
    }
}