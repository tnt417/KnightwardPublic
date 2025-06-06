using System.Collections.Generic;
using Mirror;
using TonyDev.Game.Core.Entities.Enemies.ScriptableObjects;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies
{
    public enum EnemyAnimationState
    {
        Hurt,
        Move,
        Stop,
        Attack,
        Die
    }
    
    public class EnemyAnimator : NetworkBehaviour
    {
        //Editor Variables
        [SerializeField] private Animator animator;
        //
        
        private readonly Dictionary<EnemyAnimationState, string> _animationPairs = new();

        [Command(requiresAuthority = false)]
        private void CmdPlayAnimation(EnemyAnimationState state, NetworkIdentity exclude)
        {
            if (!_animationPairs.ContainsKey(state))
            {
                ////Debug.LogWarning("Invalid anim state! " + gameObject + ", " + Enum.GetName(typeof(EnemyAnimationState), state));
                return;
            }
            RpcPlayAnimation(state, exclude);
        }

        public void PlayAnimationGlobal(EnemyAnimationState state)
        {
            PlayAnim(state);
            CmdPlayAnimation(state, NetworkClient.localPlayer);
        }

        [ClientRpc]
        private void RpcPlayAnimation(EnemyAnimationState state, NetworkIdentity exclude)
        {
            if (NetworkClient.localPlayer == exclude) return;
            
            PlayAnim(state);
        }

        private void PlayAnim(EnemyAnimationState state)
        {
            if (!_animationPairs.ContainsKey(state))
            {
                return;
            }
            animator.Play(_animationPairs[state]);
        }

        public void Set(EnemyData enemyData)
        {
            IEnumerable<EnemyAnimationPairs> animationPairs = enemyData.animations;
            foreach (var ap in animationPairs)
            {
                _animationPairs.Add(ap.enemyAnimationState, ap.AnimationName);
            }
        }
    }
}