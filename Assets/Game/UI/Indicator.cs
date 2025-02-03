using System;
using Edgegap;
using TMPro;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations;
using UnityEngine;

namespace TonyDev.Game.UI
{
    public class Indicator : MonoBehaviour
    {
        public static Indicator Instance { get; private set; }
        
        [SerializeField] private Animator animator;

        private void Awake()
        {
            Instance = this;
        }

        [Header("Label")]
        [SerializeField] private TMP_Text labelText;
        
        [Header("Cost")]
        [SerializeField] private TMP_Text costLabel;
        [SerializeField] private GameObject costObject;

        [SerializeField] private TMP_Text countLabel;
        
        private Transform _newTargetPos;

        private void Start()
        {
            Player.LocalPlayerChangeIdentity += (_) => TeleportToPlayer();
        }

        private void OnDestroy()
        {
            Player.LocalPlayerChangeIdentity -= (_) => TeleportToPlayer();
        }

        private void TeleportToPlayer()
        {
            transform.position = Player.LocalInstance.transform.position;
        }

        private void Update()
        {
            if (_newTargetPos == null)
            {
                return;
            }
            
            transform.position = Vector2.Lerp(transform.position, _newTargetPos.position, Time.deltaTime * 30f);
        }

        public void UpdateCurrentInteractable(Interactable interactable)
        {
            SetOpen(interactable != null);

            if (interactable == null)
            {
                _newTargetPos = null;
                return;
            }
         
            _newTargetPos = interactable.transform;
            
            var cost = interactable.cost;

            costObject.SetActive(cost != 0);
            costLabel.text = cost < 0 ? "+" : "" + cost;

            if (interactable is InteractableItem ii)
            {
                var count = ii.stackCount;
                
                if (count == 1)
                {
                    countLabel.enabled = false;
                }
                else
                {
                    countLabel.enabled = true;
                    countLabel.text = "x" + count;
                }
            }else
            {
                countLabel.enabled = false;
            }

            labelText.text = interactable.label;
        }
        
        
        private void SetOpen(bool open)
        {
            animator.SetBool("open", open);
        }
    }
}
