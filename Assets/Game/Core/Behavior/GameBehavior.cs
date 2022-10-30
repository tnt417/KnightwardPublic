using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TonyDev.Game.Core.Behavior
{
    public abstract class GameBehavior : MonoBehaviour
    {
        protected CancellationTokenSource DestroyToken;
        
        protected void Start()
        {
            DestroyToken = new CancellationTokenSource();

            ExecuteBehavior().AttachExternalCancellation(DestroyToken.Token);
        }

        private void OnDestroy()
        {
            DestroyToken.Cancel();
        }

        protected virtual async UniTask ExecuteBehavior()
        {
            
        }
    }
}
