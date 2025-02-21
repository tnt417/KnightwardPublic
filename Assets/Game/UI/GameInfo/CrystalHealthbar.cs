using Cysharp.Threading.Tasks;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.UI.Healthbar;
using UnityEngine;

namespace TonyDev
{
    public class CrystalHealthbar : Healthbar
    {
        public Animator animator;
        
        public new void Start()
        {
            WaitForCrystal().Forget();
        }

        private void OnVisiblilityChange(bool visible)
        {
            animator.Play(visible ? "CrystalHealthbarHide" : "CrystalHealthbarShow");
        }

        private async UniTask WaitForCrystal()
        {
            await UniTask.WaitUntil(() => Crystal.Instance != null);
            AttachedDamageable = Crystal.Instance; //Initialize the IDamageable component
            base.Start();

            await UniTask.WaitForSeconds(1f);
            Crystal.Instance.OnVisibilityChange += OnVisiblilityChange;
        }
    }
}
