using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;

namespace TonyDev.Game.Core.Behavior
{
    public abstract class GameBehavior : MonoBehaviour
    {
        protected CancellationTokenSource DestroyToken;

        private void OnEnable()
        {
            DestroyToken = new CancellationTokenSource();
        }

        [ServerCallback]
        protected void Start()
        {
            StartTask().Forget();
        }

        private async UniTask StartTask()
        {
            await UniTask.WaitUntil(() => NetworkClient.ready);
            
            DestroyToken.RegisterRaiseCancelOnDestroy(this);
            ExecuteBehavior().AttachExternalCancellation(DestroyToken.Token);
        }

        protected virtual async UniTask ExecuteBehavior()
        {
            
        }
    }
}
