using System.Collections;
using System.Collections.Generic;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level;
using TonyDev.Game.Level.Rooms;
using UnityEngine;

namespace TonyDev
{
    public class LadderController : MonoBehaviour
    {
        [SerializeField] private GameObject indicator;
        private Player _player;
        private RoomManager _roomManager;
        private void Awake()
        {
            _player = FindObjectOfType<Player>();
        }
        private void Update()
        {
            var isPlayerInRange = Vector2.Distance(transform.position, _player.transform.position) < 1.5f;
            indicator.SetActive(isPlayerInRange);
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(Use());
            }
        }

        private IEnumerator Use()
        {
            TransitionController.Instance.FadeInOut();
            yield return new WaitUntil(() => TransitionController.Instance.OutTransitionDone);
            GameManager.DungeonFloor += 1;
            RoomManager.Instance.ResetRooms();
            RoomManager.Instance.TeleportPlayerToStart();
        }
    }
}
