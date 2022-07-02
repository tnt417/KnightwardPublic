using TonyDev.Game.Core.Entities.Player;
using UnityEngine;

namespace TonyDev.Game.Core.Combat
{
    public class PlayerSlashController : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private Animator combatAnimator;
        [SerializeField] private GameObject slashObject;
        [SerializeField] private SpriteRenderer slashSpriteRenderer;
        [SerializeField] private float attackTimerMax;
        //
    
        private DamageComponent _slashDamageComponent;
        private Camera _mainCamera;
        private float _attackTimer;
        private const float SlashRadius = 1f;
    
        private void Start() //Called on start
        {
            //Initialize variables
            _mainCamera = Camera.main;
            _slashDamageComponent = slashObject.GetComponent<DamageComponent>();
        }

        private void Update()
        {
            _attackTimer += Time.deltaTime; //Tick the attack timer.
        
            //TEMPORARY: Set some variabes to scale off of player stats
            slashObject.transform.localScale = Vector3.one * PlayerStats.AoEMultiplier; //TODO
            _slashDamageComponent.knockbackMultiplier = PlayerStats.KnockbackMultiplier; //TODO make all these have a PlayerCombat.Get method
            _slashDamageComponent.damageMultiplier = PlayerStats.OutgoingDamageMultiplierWithCrit;
            _slashDamageComponent.damageCooldown = 0.333333f / PlayerStats.AttackSpeedMultiplier;
            //
        
            if (Input.GetMouseButton(0) && _attackTimer >= attackTimerMax / PlayerStats.AttackSpeedMultiplier) Attack(); //If left mouse button down and timer ready, attack.
        }

        private void Attack()
        {
            _attackTimer = 0; //Reset attack timer
            UpdateSlashDirection(); //Rotate the slash object to face the cursor
            combatAnimator.SetFloat("attackSpeed", PlayerStats.AttackSpeedMultiplier); //Update the animation speed.
            combatAnimator.Play("SwordSlash"); //Play the slashing sprite animation
        }

        private void UpdateSlashDirection() //Rotate the slash object to face the cursor
        {
            //Do some math to rotate the slash to be SlashRadius away from the player, facing the cursor
            Vector2 myPosition = transform.position;
            var direction = ((Vector2)_mainCamera.ScreenToWorldPoint(Input.mousePosition) - myPosition).normalized;
            _slashDamageComponent.KnockbackVector = direction;
            slashObject.transform.position = myPosition + direction*SlashRadius;
            slashObject.transform.right = direction;
            //
        }

        private void OnDisable()
        {
            slashSpriteRenderer.sprite = null; //Done to prevent attack animation sprites from sticking around when this is disabled
            slashObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
