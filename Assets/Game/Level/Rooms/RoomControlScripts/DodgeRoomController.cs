using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.Serialization;

namespace TonyDev
{

    [Serializable]
    public class ProjectileOrigin
    {
        public ProjectileData projectile;
        public Transform originPoint;
        public int shootAngleDegrees;
        public float fireInterval;
        //public int range; //TODO: Range doesn't work rn
        [NonSerialized] public float IntervalTimer;

        public void Shoot(GameEntity owner)
        {
            IntervalTimer = 0;
            
            ObjectSpawner.SpawnProjectile(owner, originPoint.transform.position,
                new Vector2(Mathf.Cos(shootAngleDegrees * Mathf.Deg2Rad),
                    Mathf.Sin(shootAngleDegrees * Mathf.Deg2Rad)), projectile, false);
        }

        public bool Update()
        {
            IntervalTimer += Time.deltaTime;

            return IntervalTimer > fireInterval;
        }
    }
    
    public class DodgeRoomController : NetworkBehaviour
    {

        [SerializeField] private ProjectileOrigin[] projectileOriginPoints;
        [SerializeField] private float countdownTime;
        [SerializeField] private TMP_Text countdownText;

        private float _timer;
        private Room _room;
        private bool _timerFinished = false;
        private Enemy _dummyEnemy;

        public override void OnStartServer()
        {
            var src = new CancellationTokenSource();
            src.RegisterRaiseCancelOnDestroy(this);

            ExecuteBehavior().AttachExternalCancellation(src.Token);
        }

        private async UniTask ExecuteBehavior()
        {
            await UniTask.WaitUntil(() =>
            {
                _room = RoomManager.Instance.GetRoomFromID(netId);
                return _room != null;
            });
            
            _dummyEnemy = ObjectSpawner.SpawnEnemy(ObjectFinder.GetPrefab("DummyEnemy"), transform.position, _room.netIdentity);
        }

        private void Update()
        {
            if (_room == null) return;
            
            if (_room.PlayerCount > 0 && !_timerFinished)
            {
                foreach (var po in projectileOriginPoints)
                {
                    po.IntervalTimer += Time.deltaTime;
                
                    if(po.Update()) po.Shoot(_dummyEnemy);
                }
                
                _timer += Time.deltaTime;
                countdownText.text = ((int) (countdownTime - _timer)).ToString();
                
                if(_timer >= countdownTime) OnTimerFinish();
            }
        }

        private void OnTimerFinish()
        {
            _timerFinished = true;
            _dummyEnemy.Die();
        }
    }
}
