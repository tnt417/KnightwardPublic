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
        Repair
    }
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] public UnityEvent<InteractType> onInteract = new();
        [SerializeField] protected int cost;
        [SerializeField] protected bool scaleCost;
        [SerializeField] private string label;

        [FormerlySerializedAs("IsInteractable")] public bool isInteractable = true;

        private GameObject _indicatorObject;
        protected Indicator Indicator;
        protected Dictionary<Key, InteractType> _interactKeys = new ();

        public void AddInteractKey(Key keyCode, InteractType type)
        {
            if (!(_interactKeys.ContainsKey(keyCode) && _interactKeys[keyCode] == type)) //If key hasn't been added yet for this interact type, add it
            {
                _interactKeys[keyCode] = type;
                RebuildControlLabel();
            }
        }
        
        public void OverrideInteractKey(Key keyCode, InteractType type)
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
                sb.AppendLine("[" + Enum.GetName(typeof(Key), key) + "] " +
                              Enum.GetName(typeof(InteractType), value));
            }
            
            _controlText = sb.ToString();
        }

        protected void PlayInteractSound()
        {
            SoundManager.PlaySound("interact",0.5f, transform.position);
        }

        protected void Start()
        {
            _indicatorObject = Instantiate(ObjectFinder.GetPrefab("indicator"), transform);
            Indicator = _indicatorObject.GetComponent<Indicator>();
            
            AddInteractKey(Key.E, InteractType.Interact);

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

            if (_costChanged)
            {
                Indicator.SetCost(cost);
                _costChanged = false;
            }

            if (_labelChanged)
            {
                Indicator.SetLabel(label);
                _labelChanged = false;
            }
        }

        protected virtual void OnInteract(InteractType type)
        {
            PlayInteractSound();
        }

        private bool _active = true;

        public bool Active
        {
            set
            {
                if (_active == value) return;
                _active = value;
                SetActive(value);
            }
            get => _active;
        }

        public bool overrideCurrent;
        
        public static Interactable Current;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (overrideCurrent) return;
            
            if (Current != null && Current.overrideCurrent)
            {
                Active = false;
                return;
            }
            
            if (!other.isTrigger || !other.CompareTag("Player") || !isInteractable || !Player.LocalInstance.playerMovement.DoMovement) return;
        
            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;

            if (Current == null)
            {
                Current = this;
                Active = true;
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
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (overrideCurrent) return;
            
            if (Current != null && Current.overrideCurrent)
            {
                Active = false;
                return;
            }
            
            if (!other.isTrigger || !other.CompareTag("Player") || !Player.LocalInstance.playerMovement.DoMovement) return;
            
            if (!isInteractable)
            {
                Active = false;
                if (Current == this) Current = null;
                return;
            }
            
            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;

            if (Current == null)
            {
                Active = true;
                Current = this;
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
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (overrideCurrent) return;
            
            if (Current != null && Current.overrideCurrent)
            {
                Active = false;
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
        }

        private void SetActive(bool active)
        {
            if (_indicatorObject == null)
            {
                SetActiveOnceIndicator(active).Forget();
                return;
            }
            
            _indicatorObject.SetActive(active);
            foreach (var img in _indicatorObject.GetComponentsInChildren<Image>())
                img.enabled = active;
        }

        private async UniTask SetActiveOnceIndicator(bool active)
        {
            await UniTask.WaitUntil(() => _indicatorObject != null);
            SetActive(active);
        }
    }
}