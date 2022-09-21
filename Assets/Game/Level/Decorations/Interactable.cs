using Mirror;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TonyDev.Game.Level.Decorations
{
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] public UnityEvent onInteract = new ();
        [SerializeField] private int cost;
        [SerializeField] private string label;

        protected bool IsInteractable = true;

        private GameObject _indicatorObject;
        private Indicator _indicator;
        
        private void Awake()
        {
            _indicatorObject = Instantiate(ObjectFinder.GetPrefab("indicator"), transform);
            _indicator = _indicatorObject.GetComponent<Indicator>();
            
            _indicator.SetCost(cost);
            _indicator.SetLabel(label);
            
            onInteract.AddListener(OnInteract);
        }

        public void SetCost(int newCost)
        {
            cost = newCost;
            _indicator.SetCost(cost);
        }
        
        public void SetLabel(string newLabel)
        {
            label = newLabel;
            _indicator.SetLabel(newLabel);
        }

        private void Start()
        {
            Active = false;
        }

        private void Update()
        {
            if (Active && GameManager.GameControlsActive && Input.GetKeyDown(KeyCode.E) && IsInteractable)
            {
                onInteract?.Invoke();
            }
        }
        
        protected virtual void OnInteract(){}

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
