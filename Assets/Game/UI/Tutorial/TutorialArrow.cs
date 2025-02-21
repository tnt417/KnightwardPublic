using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Crystal;
using TonyDev.Game.UI.Inventory;
using TonyDev.Game.UI.Tower;
using UnityEngine;

namespace TonyDev.Game.UI.Tutorial
{
    public class TutorialArrow : MonoBehaviour
    {
        
        // 1. Tower build button
        // 2. Ballista tower
        // 3. Regen tower
        // 4. Dungeon ladder
        
        // 5. Prompt user "Press [ALT] to view crystal" once they enter the dungeon
        // 6. 
        private int _step;

        public GameObject arrowObject;
        public Animator animator;

        [Header("Transforms")] public RectTransform towerInventoryTransform;
        public RectTransform crystalUiTransform;
        public Transform dungeonLadderTransform;

        public TMP_Text tutorialText;
        public Animator tutorialTextAnimator;

        private Func<Vector2> _goalPos;
        private float _goalRot;
        
        private void Awake()
        {
            if(!GameManager.IsDemo){ Destroy(gameObject); }
        }

        private void Start()
        {
            DoTutorial().Forget();
        }

        private Vector2 _posVelocity = Vector2.zero;
        private float _rotVelocity = 0f;
        
        private void Update()
        {
            if (_goalPos == null) return;
            transform.position = _goalPos();
                //Vector2.SmoothDamp(transform.position, _goalPos.Invoke(), ref _posVelocity, 0.001f);
            transform.rotation = Quaternion.Euler(0, 0,
                Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, _goalRot, ref _rotVelocity, 0.01f));
        }
        
        private async UniTask DoTutorial()
        {
            if(PlayerPrefs.GetInt("demo_tutorial_completed", 0) == 1) return;
            
            await UniTask.WaitUntil(() => GameManager.Instance != null);
            
            GameManager.Instance.SetUi("off");
            
            await UniTask.WaitUntil(() => Player.LocalInstance != null);
            
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            
            tutorialTextAnimator.Play("TextShow");

            var storedPlayerPos = Player.LocalInstance.transform.position;
            
            tutorialText.text = "Welcome to the Knightward Demo! Use [WASD] or [Arrows] to move.";
            
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            await UniTask.WaitUntil(() => Vector2.Distance(storedPlayerPos, Player.LocalInstance.transform.position) > 0.5f);
            SoundManager.PlaySoundPitchVariant("success", 0.5f, Player.LocalInstance.transform.position, 1.05f, 1.15f);
            
            tutorialTextAnimator.Play("TextHide");
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            
            tutorialTextAnimator.Play("TextShow");
            
            tutorialText.text = "Your only goal is to defend this giant crystal by whatever means necessary. Hold [LEFT-CLICK] to attack.";

            var closestSlime = GameManager.GetEntitiesInRange(Player.LocalInstance.transform.position, 20f).First(ge => ge is Enemy);
            
            _goalPos = () => (Vector2) closestSlime.transform.position + new Vector2(0, 0.7f);
            _goalRot = -90f;

            await UniTask.WaitUntil(() => closestSlime == null || !closestSlime.IsAlive);
            
            SoundManager.PlaySoundPitchVariant("success", 0.5f, Player.LocalInstance.transform.position, 1.05f, 1.15f);
            
            _goalPos = () => new Vector2(100000f, 100000f);
            
            tutorialTextAnimator.Play("TextHide");
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            
            GameManager.Instance.SetUi("on");
            
            tutorialTextAnimator.Play("TextShow");
            
            tutorialText.text = "Luckily you have some towers that will be enough to protect the crystal for you! ...for now.";
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            // step 1
            _goalPos = () =>
            {
                // Get button's screen position
                var screenPos = RectTransformUtility.WorldToScreenPoint(null, towerInventoryTransform.transform.position);

                // Convert screen position to world position
                Vector2 worldPos = GameManager.MainCamera.ScreenToWorldPoint(new Vector2(screenPos.x, screenPos.y)); // Adjust depth if needed

                return worldPos + new Vector2(0, 0.9f);
            };
            
            _goalRot = -90f;

            await UniTask.WaitUntil(() => InventoryUIController.ActivePanel == "Tower");
            
            tutorialTextAnimator.Play("TextHide");
            
            // step 2
            var starterTowerTransform = TowerUIController.Instance.GetTowerTransformFromIndex(0);

            _goalPos = () =>
            {
                // Get button's screen position
                var screenPos =
                    RectTransformUtility.WorldToScreenPoint(null, starterTowerTransform.transform.position);

                // Convert screen position to world position
                Vector2 worldPos =
                    GameManager.MainCamera.ScreenToWorldPoint(new Vector2(screenPos.x,
                        screenPos.y)); // Adjust depth if needed

                return worldPos + new Vector2(-2.7f, 0f);
            };

            _goalRot = 0f;

            await UniTask.WaitUntil(() => TowerPlacementManager.Instance.Placing);
            
            _goalPos = () => (Vector2) Crystal.Instance.transform.position + new Vector2(3f, 1f);

            _goalRot = -90f;
            
            await UniTask.WaitUntil(() => !TowerPlacementManager.Instance.Placing);
            
            tutorialTextAnimator.Play("TextShow");
            tutorialTextAnimator.Play("TextEmphasize");
            
            SoundManager.PlaySoundPitchVariant("success", 0.5f, Player.LocalInstance.transform.position, 1.05f, 1.15f);
            tutorialText.text = "Good job!";
            
            //  step 3
            var starterTowerTransform2 = TowerUIController.Instance.GetTowerTransformFromIndex(0);

            _goalPos = () =>
            {
                // Get button's screen position
                var screenPos =
                    RectTransformUtility.WorldToScreenPoint(null, starterTowerTransform2.transform.position);

                // Convert screen position to world position
                Vector2 worldPos =
                    GameManager.MainCamera.ScreenToWorldPoint(new Vector2(screenPos.x,
                        screenPos.y)); // Adjust depth if needed

                return worldPos + new Vector2(-2.7f, 0f);
            };

            _goalRot = 0f;

            await UniTask.WaitUntil(() => TowerPlacementManager.Instance.Placing);
            
            tutorialTextAnimator.Play("TextHide");
            
            _goalPos = () => (Vector2) Crystal.Instance.transform.position + new Vector2(3f, 0f);

            _goalRot = -90f;
            
            await UniTask.WaitUntil(() => !TowerPlacementManager.Instance.Placing);
            
            // step 4
            
            tutorialTextAnimator.Play("TextShow");
            tutorialTextAnimator.Play("TextEmphasize");
            
            SoundManager.PlaySoundPitchVariant("success", 0.5f, Player.LocalInstance.transform.position, 1.05f, 1.15f);
            tutorialText.text = "Great job! Now you should be safe to explore the dungeon for a bit.";
            
            _goalPos = () => (Vector2) Player.LocalInstance.transform.position + new Vector2(-8f, 0f);

            _goalRot = 180f;

            await UniTask.WaitUntil(() =>
                Vector2.Distance(Player.LocalInstance.transform.position, dungeonLadderTransform.position) < 10f);
            
            tutorialTextAnimator.Play("TextHide");
            
            _goalPos = () => (Vector2) dungeonLadderTransform.position + new Vector2(0f, 1f);

            _goalRot = -90f;
            
            await UniTask.WaitUntil(() =>
                Vector2.Distance(Player.LocalInstance.transform.position, dungeonLadderTransform.position) < 1f);
            
            _goalPos = () => new Vector2(100f, 100f);

            await UniTask.WaitUntil(() => GameManager.GamePhase == GamePhase.Dungeon);

            tutorialTextAnimator.Play("TextShow");
            tutorialText.text = "Press [ALT] to view the crystal";
            
            await UniTask.WaitUntil(() => SmoothCameraFollow.FocusedCrystalLast);
            
            SoundManager.PlaySoundPitchVariant("success", 0.5f, Crystal.Instance.transform.position, 1.05f, 1.15f);
            tutorialTextAnimator.Play("TextShow");
            tutorialTextAnimator.Play("TextEmphasize");
            tutorialText.text = "Good job!";
            
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            tutorialTextAnimator.Play("TextHide");
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            await UniTask.WaitUntil(() => !SmoothCameraFollow.FocusedCrystalLast);
            
            
            // Show the minimap off
            tutorialTextAnimator.Play("TextShow");
            tutorialText.text = "Your crystal's health and surroundings are displayed here";
            
            _goalPos = () =>
            {
                // Get button's screen position
                var screenPos = RectTransformUtility.WorldToScreenPoint(null, crystalUiTransform.transform.position);

                // Convert screen position to world position
                Vector2 worldPos = GameManager.MainCamera.ScreenToWorldPoint(new Vector2(screenPos.x, screenPos.y)); // Adjust depth if needed

                return worldPos + new Vector2(2.5f, -2.5f);
            };
            
            _goalRot = 45f+90f;
            
            await UniTask.Delay(TimeSpan.FromSeconds(4f));
            
            _goalPos = () => new Vector2(100000f, 100000f);
            
            tutorialTextAnimator.Play("TextShow");
            
            tutorialText.text = "Now go explore, and don't forget to keep your crystal safe!";

            await UniTask.Delay(TimeSpan.FromSeconds(4f));
            
            tutorialTextAnimator.Play("TextHide");

            bool showedDeath = false;
            bool showedCrystal = false;
            
            PlayerPrefs.SetInt("demo_tutorial_completed", 1);
            
            // After dying
            while (true)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);

                if (Crystal.Instance.CurrentHealth/Crystal.Instance.MaxHealth < 0.5f && !showedCrystal)
                {
                    showedCrystal = true;
                    
                    tutorialTextAnimator.Play("TextShow");

                    tutorialText.text =
                        "Your crystal is starting to get low on health. Watch out for a green healing chalice at the end of each dungeon floor!";
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(5f));
                    
                    tutorialTextAnimator.Play("TextHide");
                }
                
                if (Player.LocalInstance.playerDeath.dead && !showedDeath)
                {
                    showedDeath = true;
                    
                    tutorialTextAnimator.Play("TextShow");

                    tutorialText.text =
                        "Oh no! Unlike other rogue-likes, dying isn't a huge deal in Knightward. Go back to where you died to get back most of the money you had.";
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(5f));
                    
                    tutorialTextAnimator.Play("TextHide");
                }
            }
        }
    }
}
