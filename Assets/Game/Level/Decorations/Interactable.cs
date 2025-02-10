using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TonyDev.Game.Level.Decorations
{
    public enum InteractType
    {
        Interact,
        Scrap,
        Purchase,
        Pickup,
        Repair,
        None
    }
    
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] public UnityEvent<InteractType> onInteract = new();
        [SerializeField] public int cost;
        [SerializeField] protected bool scaleCost;
        [SerializeField] public string label;

        [FormerlySerializedAs("IsInteractable")] public bool isInteractable = true;
        
        protected Dictionary<Key, InteractType> _interactKeys = new ();

        private static bool _canInteract = true;
        
        public static async UniTask DisableInteractablesForSeconds(float seconds)
        {
            _canInteract = false;
            await UniTask.Delay(TimeSpan.FromSeconds(seconds));
            _canInteract = true;
        }
        
        // public void AddInteractKey(Key keyCode, InteractType type)
        // {
        //     if (!(_interactKeys.ContainsKey(keyCode) && _interactKeys[keyCode] == type)) //If key hasn't been added yet for this interact type, add it
        //     {
        //         _interactKeys[keyCode] = type;
        //         RebuildControlLabel();
        //     }
        // }
        
        public void SetInteractKey(Key keyCode, InteractType type)
        {
            _interactKeys[keyCode] = type;
            RebuildControlLabel();
        }

        private string _controlText = "";
        
        private void RebuildControlLabel()
        {
            StringBuilder sb = new();
            foreach (var (key, value) in _interactKeys)
            {
                if (value == InteractType.None) continue;
                sb.AppendLine("[" + Enum.GetName(typeof(Key), key) + "] " +
                              Enum.GetName(typeof(InteractType), value));
            }
            
            _controlText = sb.ToString();
        }

        protected void Start()
        {
            SetInteractKey(Key.E, InteractType.Interact);

            SetCost((int)(scaleCost ? cost*ItemGenerator.DungeonInteractMultiplier : cost));
            SetLabel(label, true);

            onInteract.AddListener(OnInteract);

            Active = false;
        }

        private bool _costChanged;
        
        public virtual void SetCost(int newCost)
        {
            if (newCost == cost) return;
            cost = newCost;
            _costChanged = true;
        }

        private bool _labelChanged;

        public void SetLabel(string newLabel, bool controls)
        {
            StringBuilder sb = new();

            sb.Append(newLabel + "\n");
            sb.Append(controls ? _controlText : "");

            label = sb.ToString();
            _labelChanged = true;
        }

        protected void Update()
        {
            if (!_canInteract && Current == this)
            {
                Active = false;
                Current = null;
                Indicator.Instance.UpdateCurrentInteractable(null);
            }
            
            if (!Active) return;
            
            foreach (var (_, value) in _interactKeys.Where(kv => Active && GameManager.GameControlsActive && Keyboard.current[kv.Key].wasPressedThisFrame && isInteractable))
            {
                if (value == InteractType.Interact && cost != 0)
                {
                    if(GameManager.Money < cost)
                    {
                        ObjectSpawner.SpawnTextPopup(transform.position, "You can't afford this!", Color.red, 0.8f);
                        continue;
                    }
                    else
                    {
                        ObjectSpawner.SpawnTextPopup(transform.position, "Purchased!", Color.green, 0.8f);
                        GameManager.Money -= cost;
                    }
                }

                onInteract?.Invoke(value);
            }
        }

        protected virtual void OnInteract(InteractType type)
        {
            PlayInteractSound();
        }

        protected void PlayInteractSound()
        {
            SoundManager.PlaySoundPitchVariant("interact", 0.5f, GameManager.MainCamera.transform.position, 0.95f, 1.05f);
        }

        private bool _active = true;
        public bool Active;
        public bool overrideCurrent;
        private int _cost;
        private UnityEvent<InteractType> _onInteract;
        private string _label;

        public static Interactable Current;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (overrideCurrent) return;
            
            if (Current != null && Current.overrideCurrent)
            {
                Active = false;
                TryCallToUpdate();
                return;
            }
            
            if (!other.isTrigger || !other.CompareTag("Player") || !isInteractable || !Player.LocalInstance.playerMovement.DoMovement) return;
        
            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;

            if (Current == null)
            {
                Current = this;
                Active = true;
                TryCallToUpdate();
                return;
            }
            
            if (Current != this)
            {
                if (Vector2.Distance(other.transform.position, Current.transform.position) <
                    Vector2.Distance(other.transform.position, transform.position)) // If the current one is closer, don't activate.
                {
                    Active = false;
                }
                else
                {
                    Current.Active = false;
                    Current = this;
                    Active = true;
                }
            }
            
            TryCallToUpdate();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (overrideCurrent) return;
            
            if (Current != null && Current.overrideCurrent)
            {
                TryCallToUpdate();
                return;
            }
            
            if (!other.isTrigger || !other.CompareTag("Player") || !Player.LocalInstance.playerMovement.DoMovement) return;
            
            if (!isInteractable)
            {
                Active = false;
                if (Current == this) Current = null;
                TryCallToUpdate();
                return;
            }
            
            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;

            if (Current == null)
            {
                Active = true;
                Current = this;
                TryCallToUpdate();
                return;
            }
            
            if (Vector2.Distance(other.transform.position, Current.transform.position) <
                Vector2.Distance(other.transform.position, transform.position)) // If the current one is closer, don't activate.
            {
                Active = false;
            }
            else
            {
                Current.Active = false;
                Active = true;
                Current = this;
            }
            
            TryCallToUpdate();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (overrideCurrent) return;
            
            if (Current != null && Current.overrideCurrent)
            {
                Active = false;
                TryCallToUpdate();
                return;
            }
            
            if (!other.isTrigger || !other.CompareTag("Player")) return;
            
            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;

            Active = false;

            if (Current == this)
            {
                Current = null;
            }
            
            TryCallToUpdate();
        }
        
        protected void TryCallToUpdate()
        {
            if (Current == this)
            {
                Indicator.Instance.UpdateCurrentInteractable(this);
            }
            else if (Current == null)
            {
                Indicator.Instance.UpdateCurrentInteractable(null);
            }
        }
    }
}