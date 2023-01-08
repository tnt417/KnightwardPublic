using System.Collections;
using System.Collections.Generic;
using Mirror;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using UnityEngine;

namespace TonyDev
{
    public class TimedItemRoomController : NetworkBehaviour
    {
        [SerializeField] private float startTimer;
        [SerializeField] private float multiplier;
        [SerializeField] private float maxRolls;
        [SerializeField] private Transform itemSpawnPos;
        
        private float _currentTimer;

        private float _timer = Mathf.Infinity;

        private int _rolls;

        private GroundItem _previousGroundItem;

        private Room _room;

        public override void OnStartServer()
        {
            _room = GetComponent<Room>();
            _currentTimer = startTimer;
        }

        private bool _started;

        private bool _done;
        
        [ServerCallback]
        private void Update()
        {
            if (_done) return;
            
            if (_room.PlayerCount > 0)
            {
                _started = true;
            }

            if (!_started) return;
            
            _timer += Time.deltaTime;

            if (_timer >= _currentTimer)
            {
                _timer = 0;
                _currentTimer *= multiplier;
                _rolls++;

                if (_previousGroundItem == null)
                {
                    _previousGroundItem = ObjectSpawner.SpawnGroundItem(ItemGenerator.GenerateItem(0), 0, itemSpawnPos.position, netIdentity);
                    _previousGroundItem.onPickupServer.AddListener(() =>
                    {
                        _done = true;
                    });
                }
                else
                {
                    _previousGroundItem.CmdSetItem(ItemGenerator.GenerateItem(0));
                }

                if (_rolls >= maxRolls)
                {
                    Destroy(this);
                }
            }
        }
    }
}
