using System.Collections;
using Mirror;
using TMPro;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Global;
using TonyDev.Game.Global.Network;
using TonyDev.Game.Level.Decorations.Chests;
using TonyDev.Game.Level.Rooms;
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
        [SerializeField] private GameObject pickupIndicator;
        [SerializeField] private GameObject moneyIcon;

        [SerializeField] private TMP_Text moneyLabel;
        //

        private bool _pickupAble = true;

        [FormerlySerializedAs("onPickup")] public UnityEvent onPickupServer = new(); //TODO: this won't work either. Will need to be removed.

        private void Awake()
        {
            spriteRenderer.sharedMaterial =
                new Material(spriteRenderer
                    .sharedMaterial); //Create a copy of the renderer's material to allow temporary editing.
        }

        public override void OnStartServer()
        {
            if (Item == null) CmdSetItem(ItemGenerator.GenerateItem(rarityBoost));
        }

        public void GenerateCost(float costMultiplier)
        {
            if (Item == null)
            {
                Debug.LogWarning("Cannot generate cost for null item!");
                return;
            }
            
            CmdSetCost((int)(ItemGenerator.GenerateCost(Item) * costMultiplier));
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

        private void OnCostChangeHook(int oldCost, int newCost)
        {
            moneyIcon.SetActive(cost != 0);
            moneyLabel.enabled = cost != 0;
            moneyLabel.text = cost.ToString();
        }

        [Command(requiresAuthority = false)]
        private void CmdSetCost(int newCost)
        {
            cost = newCost;
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;

            pickupIndicator.SetActive(true); //Show pickup indicator when the player is on top of the item
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            if (!Input.GetKey(KeyCode.E) || !_pickupAble) return;
            
            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;

            CmdRequestPickup(GameManager.Money);
            StartCoroutine(DisablePickupForSeconds(0.1f)); //Disable pickup for 0.5 seconds to prevent insta-replacing the item
        }

        [Command(requiresAuthority = false)]
        private void CmdRequestPickup(int senderMoney, NetworkConnectionToClient sender = null)
        {
            Debug.Log($"Sender {sender?.connectionId} requested to pickup item {Item.itemName}");

            if (senderMoney < cost || !_pickupAble) return; //If the item is too expensive, don't allow pickup.

            Debug.Log("Confirming pickup.");

            TargetConfirmPickup(sender, cost);

            StartCoroutine(
                DisablePickupForSeconds(0.1f)); //Disable pickup for 0.5 seconds to prevent insta-replacing the item

            onPickupServer.Invoke(); //Invoke the onPickup method
        }

        [TargetRpc]
        private void TargetConfirmPickup(NetworkConnection target, int confirmedCost)
        {
            GameManager.Money -= confirmedCost;

            var returnItem = PlayerInventory.Instance.InsertItem(Item);

            Debug.Log($"Received item pickup confirmation. Inserted item. Return item: {returnItem?.itemName}");

            CmdNotifyReplacementItem(returnItem);
        }

        [Command(requiresAuthority = false)]
        private void CmdNotifyReplacementItem(Item newItem)
        {
            Debug.Log($"Received notification of replacement item: {newItem?.itemName}");

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

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var id = other.GetComponent<NetworkIdentity>();

            if (id == null || !id.isLocalPlayer) return;
                
            pickupIndicator.SetActive(false); //Deactivate pickup indicator when the player is no longer on top of the item
        }

        [field: SyncVar] public NetworkIdentity CurrentParentIdentity { get; set; }
    }
}