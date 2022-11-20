using System.Collections;
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
using UnityEngine.Serialization;
using Random = System.Random;

namespace TonyDev.Game.Core.Items
{
    public class GroundItem : NetworkBehaviour, IHideable
    {
        //Editor variables
        [SerializeField] private int rarityBoost;

        [SerializeField] private SpriteRenderer spriteRenderer;
        //

        private InteractableItem _interactable;

        private bool _pickupAble = true;

        [FormerlySerializedAs("onPickup")]
        public UnityEvent onPickupServer = new();
        public UnityEvent onPickupGlobal = new();

        private void Awake()
        {
            spriteRenderer.sharedMaterial =
                new Material(spriteRenderer
                    .sharedMaterial); //Create a copy of the renderer's material to allow temporary editing.

            _interactable = GetComponent<InteractableItem>();
            
            _interactable.AddInteractKey(KeyCode.F, InteractType.Scrap);
            
            _interactable.onInteract.AddListener((type) =>
            {
                if (cost > GameManager.Money)
                {
                    ObjectSpawner.SpawnTextPopup(Player.LocalInstance.transform.position, "You can't afford this!", Color.red, 0.5f);
                    return;
                }
                
                if (type is InteractType.Purchase or InteractType.Pickup)
                {
                    CmdRequestPickup(GameManager.Money);
                    StartCoroutine(
                        DisablePickupForSeconds(
                            0.1f)); //Disable pickup for 0.5 seconds to prevent insta-replacing the item
                }
                else if(type == InteractType.Scrap)
                {
                    CmdRequestScrap(GameManager.Money);
                }
            });
        }

        public override void OnStartServer()
        {
            if (Item == null) CmdSetItem(ItemGenerator.GenerateItem(rarityBoost));
        }

        [field: SyncVar(hook = nameof(OnItemChangeHook))]
        public Item Item { get; private set; }

        private void OnItemChangeHook(Item oldItem, Item newItem)
        {
            spriteRenderer.sprite = newItem.uiSprite; //Update the sprite
            UpdateOutlineColor(); //Update the outline color
        }

        //Set the GroundItem's item.
        [Command(requiresAuthority = false)]
        public void CmdSetItem(Item newItem)
        {
            if (newItem == null)
            {
                NetworkServer.Destroy(gameObject);
                return;
            }

            Item = newItem; //Update the item
        }

        [SerializeField] [SyncVar(hook = nameof(OnCostChangeHook))]
        private int cost;

        [SerializeField] [SyncVar(hook = nameof(OnEssenceChangeHook))]
        private int essence;

        private void OnCostChangeHook(int oldCost, int newCost)
        {
            _interactable.SetCost(newCost);
        }

        private void OnEssenceChangeHook(int oldEssence, int newEssence)
        {
            _interactable.SetEssence(newEssence);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetCost(int newCost)
        {
            cost = newCost;
        }
        
        [Command(requiresAuthority = false)]
        public void CmdSetEssence(int newEssence)
        {
            essence = newEssence;
        }

        private void UpdateOutlineColor()
        {
            switch (
                Item.itemRarity) //Set the material's outline color based on the rarity. These are hardcoded right now.
            {
                case ItemRarity.Common:
                    spriteRenderer.sharedMaterial.SetColor("_OutlineColor", Color.black);
                    break;
                case ItemRarity.Uncommon:
                    spriteRenderer.sharedMaterial.SetColor("_OutlineColor", Color.green);
                    break;
                case ItemRarity.Rare:
                    spriteRenderer.sharedMaterial.SetColor("_OutlineColor", Color.yellow);
                    break;
                case ItemRarity.Unique:
                    spriteRenderer.sharedMaterial.SetColor("_OutlineColor", Color.red);
                    break;
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdRequestPickup(int senderMoney, NetworkConnectionToClient sender = null)
        {
            if (senderMoney < cost || !_pickupAble) return; //If the item is too expensive, don't allow pickup.

            RpcNotifyPickup();
            TargetConfirmPickup(sender, cost);

            StartCoroutine(
                DisablePickupForSeconds(0.1f)); //Disable pickup for 0.5 seconds to prevent insta-replacing the item

            onPickupServer.Invoke(); //Invoke the onPickup method
        }

        [Command(requiresAuthority = false)]
        private void CmdRequestScrap(int senderMoney, NetworkConnectionToClient sender = null)
        {
            if (senderMoney < cost || !_pickupAble) return; //If the item is too expensive, don't allow pickup.

            RpcNotifyPickup();
            TargetConfirmScrap(sender, essence, cost);

            onPickupServer.Invoke(); //Invoke the onPickup method
        }

        [ClientRpc]
        private void RpcNotifyPickup()
        {
            onPickupGlobal.Invoke();
        }

        [TargetRpc]
        private void TargetConfirmScrap(NetworkConnection target, int confirmedEssence, int confirmedCost)
        {
            GameManager.Essence += confirmedEssence;
            GameManager.Money -= confirmedCost;
            
            ObjectSpawner.SpawnTextPopup(transform.position, "+" + confirmedEssence + " essence", Color.cyan, 0.8f);
            
            CmdNotifyReplacementItem(null);
        }

        [TargetRpc]
        private void TargetConfirmPickup(NetworkConnection target, int confirmedCost)
        {
            GameManager.Money -= confirmedCost;

            var returnItem = PlayerInventory.Instance.InsertItem(Item);

            ObjectSpawner.SpawnTextPopup(transform.position, "Item inserted", Color.green, 0.8f);

            CmdNotifyReplacementItem(returnItem);
        }

        [Command(requiresAuthority = false)]
        private void CmdNotifyReplacementItem(Item newItem)
        {

            if (newItem == null)
                NetworkServer.Destroy(gameObject); //If no item was replaced, just destroy this GroundItem
            else
            {
                CmdSetItem(newItem); //Otherwise, replaced the item
                CmdSetCost(0); //Don't make the player pay for their replaced item
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