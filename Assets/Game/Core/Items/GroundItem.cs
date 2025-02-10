using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Decorations;
using TonyDev.Game.Level.Decorations.Chests;
using TonyDev.Game.Level.Rooms;
using TonyDev.Game.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = System.Random;

namespace TonyDev.Game.Core.Items
{
    public class GroundItem : NetworkBehaviour, IHideable
    {
        //Editor variables
        [SerializeField] private int rarityBoost;
        [SerializeField] private Animator animator;

        [SerializeField] private SpriteRenderer spriteRenderer;
        //

        private InteractableItem _interactable;

        private bool _pickupAble = true;

        [FormerlySerializedAs("onPickup")] public UnityEvent onPickupServer = new();
        public UnityEvent onPickupGlobal = new();

        private void Awake()
        {
            spriteRenderer.sharedMaterial =
                new Material(spriteRenderer
                    .sharedMaterial); //Create a copy of the renderer's material to allow temporary editing.

            _interactable = GetComponent<InteractableItem>();

            _interactable.SetInteractKey(Key.F, cost < 0 ? InteractType.Scrap : InteractType.None);

            _interactable.onInteract.AddListener((type) =>
            {
                if (cost > GameManager.Money)
                {
                    ObjectSpawner.SpawnTextPopup(Player.LocalInstance.transform.position, "You can't afford this!",
                        Color.red, 0.5f);
                    return;
                }
                
                Debug.Log("On interact!");

                if (type is InteractType.Purchase or InteractType.Pickup)
                {
                    CmdRequestPickup(GameManager.Money);
                }
                else if (type == InteractType.Scrap)
                {
                    CmdRequestSell();
                }
            });
        }

        private float _birthTime = Mathf.Infinity;

        public override void OnStartClient()
        {
            _birthTime = Time.time;
        }

        public override void OnStartServer()
        {
            if (Item == null) CmdSetItem(ItemGenerator.GenerateItem(rarityBoost));
        }

        [field: SyncVar(hook = nameof(OnItemChangeHook))]
        public Item Item { get; private set; }

        private void OnItemChangeHook(Item oldItem, Item newItem)
        {
            //Debug.Log("Item change hook called");
            ItemChangeTask(oldItem, newItem).Forget();
            _interactable.SetCount(newItem.stackCount);
        }

        private async UniTask ItemChangeTask(Item oldItem, Item newItem)
        {
            
            if (oldItem != null && Time.time - _birthTime > 0.5f)
            {
                animator.Play("GroundItemPickup");
                await UniTask.Delay(250);
                spriteRenderer.sprite = newItem.uiSprite; //Update the sprite
                UpdateOutlineColor(); //Update the outline color

                animator.Play("GroundItemSpawn");
                await UniTask.Delay(250);
                _pickupPending = false;
                return;
            }
            
            _pickupPending = false;

            spriteRenderer.sprite = newItem.uiSprite; //Update the sprite
            UpdateOutlineColor(); //Update the outline color
        }

        //Set the GroundItem's item.
        [Command(requiresAuthority = false)]
        public void CmdSetItem(Item newItem)
        {
            if (newItem == null)
            {
                ItemDestroyTask().Forget();
                return;
            }
            
            //Debug.Log("Command setting new non-null item");

            Item = newItem; //Update the item
        }

        [Server]
        private async UniTask ItemDestroyTask()
        {
            RpcPlayPickup();
            _interactable.Active = false;
            _interactable.enabled = false;
            await UniTask.Delay(250);
            NetworkServer.Destroy(gameObject);
        }

        [ClientRpc]
        private void RpcPlayPickup()
        {
            animator.Play("GroundItemPickup");
        }

        [SerializeField] [SyncVar(hook = nameof(OnCostChangeHook))]
        private int cost;

        // [SerializeField] [SyncVar(hook = nameof(OnSellPriceChangeHook))]
        // private int sellPrice;

        private void OnCostChangeHook(int oldCost, int newCost)
        {
            _interactable.SetInteractKey(Key.F, newCost < 0 ? InteractType.Scrap : InteractType.None);

            _interactable.SetInteractKey(Key.E, newCost <= 0 ? InteractType.Pickup : InteractType.Purchase);

            if(newCost >= 0) _interactable.SetCost(newCost);
            if(newCost < 0) _interactable.SetSellPrice(-newCost);
        }

        // private void OnSellPriceChangeHook(int oldSellPrice, int newSellPrice)
        // {
        //     _interactable.SetSellPrice(newSellPrice);
        // }

        [Command(requiresAuthority = false)]
        public void CmdSetCost(int newCost)
        {
            cost = newCost;
        }

        // [Command(requiresAuthority = false)]
        // public void CmdSetSellPrice(int newSell)
        // {
        //     sellPrice = newSell;
        // }

        public static Color CommonColor = Color.black;//new Color(43f/255f, 43f/255f, 69f/255f);
        public static Color UncommonColor = Color.green;//new Color(99f/255f, 171f/255f, 63f/255f);
        public static Color RareColor = Color.yellow;//new Color(255f/255f, 238f/255f, 131f/255f);
        public static Color UniqueColor = Color.red;//new Color(230f/255f, 69f/255f, 57f/255f);

        public static Color RarityToColor(ItemRarity rarity)
        {
            switch (
                rarity) //Set the material's outline color based on the rarity. These are hardcoded right now.
            {
                case ItemRarity.Common:
                    return CommonColor;
                    break;
                case ItemRarity.Uncommon:
                    return UncommonColor;
                    break;
                case ItemRarity.Rare:
                    return RareColor;
                    break;
                case ItemRarity.Unique:
                    return UniqueColor;
                    break;
            }

            return Color.white;
        }
        
        private void UpdateOutlineColor()
        {
            spriteRenderer.sharedMaterial.SetColor("_OutlineColor", RarityToColor(Item.itemRarity));
        }

        private bool _pickupPending = false;

        [Command(requiresAuthority = false)]
        private void CmdRequestPickup(int senderMoney, NetworkConnectionToClient sender = null)
        {
            if (senderMoney < cost || !_pickupAble || _pickupPending)
                return; //If the item is too expensive, don't allow pickup.

            //Debug.Log("Requesting pickup");
            _pickupPending = true;

            RpcNotifyPickup();
            TargetConfirmPickup(sender, cost > 0 ? cost : 0);

            StartCoroutine(
                DisablePickupForSeconds(0.1f)); //Disable pickup for 0.5 seconds to prevent insta-replacing the item

            onPickupServer.Invoke(); //Invoke the onPickup method
        }

        [Command(requiresAuthority = false)]
        private void CmdRequestSell(NetworkConnectionToClient sender = null)
        {
            if (cost > 0 || !_pickupAble) return; //If the item costs money, don't allow selling it.

            RpcNotifyPickup();
            TargetConfirmSell(sender, -cost);

            onPickupServer.Invoke(); //Invoke the onPickup method
        }

        [ClientRpc]
        private void RpcNotifyPickup()
        {
            Debug.Log("Rpc pickup called!");
            
            onPickupGlobal.Invoke();
        }

        [TargetRpc]
        private void TargetConfirmSell(NetworkConnection target, int confirmedCoins)
        {
            GameManager.Money += confirmedCoins;

            ObjectSpawner.SpawnTextPopup(transform.position, "+" + confirmedCoins + " coins", Color.yellow, 0.8f);

            CmdNotifyReplacementItem(null);
        }

        [TargetRpc]
        private void TargetConfirmPickup(NetworkConnection target, int confirmedCost)
        {
            Debug.Log("Target pickup called!");
            
            GameManager.Money -= confirmedCost;

            var pickupAnimUI = GameObject.Instantiate(ObjectFinder.GetPrefab("pickupAnimUI"), transform.position, Quaternion.identity)
                .GetComponent<PickupAnimationUI>();
            
            pickupAnimUI.Set(transform.position, Item);
            
            var returnItem = PlayerInventory.Instance.InsertItem(Item);

            // Just insert the other items with no return. Count should only be >1 for towers which have no replacement.
            for (var i = 1; i < Item.stackCount; i++)
            {
                PlayerInventory.Instance.InsertItem(Item);
            }

            ObjectSpawner.SpawnTextPopup(transform.position, "Item inserted", Color.green, 0.8f);

            CmdNotifyReplacementItem(returnItem);
        }

        [Command(requiresAuthority = false)]
        private void CmdNotifyReplacementItem(Item newItem)
        {
            if (newItem == null)
            {
                ItemDestroyTask().Forget();
                //NetworkServer.Destroy(gameObject); //If no item was replaced, just destroy this GroundItem
            }
            else
            {
                CmdSetItem(newItem); //Otherwise, replaced the item
                CmdSetCost(-ItemGenerator.GenerateSellPrice(newItem, GameManager.DungeonFloor)); //Don't make the player pay for their replaced item
            }
        }

        private IEnumerator DisablePickupForSeconds(float seconds) //Disables pickup for a specified number of seconds
        {
            _pickupAble = false;
            yield return new WaitForSeconds(seconds);
            _pickupAble = true;
        }

        [field: SyncVar] public NetworkIdentity CurrentParentIdentity { get; set; }
    }
}