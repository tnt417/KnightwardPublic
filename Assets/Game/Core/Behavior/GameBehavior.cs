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
            ExecuteBehavior().AttachExternalCancellation(DestroyToken.Token);
        }

        [ServerCallback]
        private void OnDestroy()
        {
            DestroyToken.Cancel();
        }

        protected virtual async UniTask ExecuteBehavior()
        {
            
        }
    }
}
