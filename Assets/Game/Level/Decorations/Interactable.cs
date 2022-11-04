using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirror;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Rooms;
using TonyDev.Game.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TonyDev.Game.Level.Decorations
{
    public enum InteractType
    {
        Interact,
        Scrap,
        Purchase,
        Pickup
    }
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] public UnityEvent<InteractType> onInteract = new();
        [SerializeField] protected int cost;
        [SerializeField] protected bool scaleCost;
        [SerializeField] private string label;

        protected bool IsInteractable = true;

        private GameObject _indicatorObject;
        protected Indicator Indicator;
        protected Dictionary<KeyCode, InteractType> _interactKeys = new ();

        public void AddInteractKey(KeyCode keyCode, InteractType type)
        {
            if (!(_interactKeys.ContainsKey(keyCode) && _interactKeys[keyCode] == type)) //If key hasn't been added yet for this interact type, add it
            {
                _interactKeys[keyCode] = type;
                RebuildControlLabel();
            }
        }
        
        public void OverrideInteractKey(KeyCode keyCode, InteractType type)
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
                sb.AppendLine("[" + Enum.GetName(typeof(KeyCode), key) + "] " +
                              Enum.GetName(typeof(InteractType), value));
            }
            
            _controlText = sb.ToString();
        }
        
        protected void Start()
        {
            _indicatorObject = Instantiate(ObjectFinder.GetPrefab("indicator"), transform);
            Indicator = _indicatorObject.GetComponent<Indicator>();
            
            AddInteractKey(KeyCode.E, InteractType.Interact);

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
            foreach (var kc in _interactKeys.Where(kv => Active && GameManager.GameControlsActive && Input.GetKeyDown(kv.Key) && IsInteractable))
            {
                if (kc.Value == InteractType.Interact && cost != 0)
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

                onInteract?.Invoke(kc.Value);
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
        }

        private bool _active = true;

        private bool Active
        {
            set
            {
                if (_active == value) return;
                _active = value;
                SetActive(value);
            }
            get => _active;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;

            if (!other.isTrigger || !other.CompareTag("Player") || !IsInteractable) return;
            Active = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!IsInteractable) Active = false;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;

            if (!other.isTrigger || !other.CompareTag("Player")) return;
            Active = false;
        }

        private void SetActive(bool active)
        {
            _indicatorObject.SetActive(active);
            foreach (var img in _indicatorObject.GetComponentsInChildren<Image>())
                img.enabled = active;
        }
    }
}