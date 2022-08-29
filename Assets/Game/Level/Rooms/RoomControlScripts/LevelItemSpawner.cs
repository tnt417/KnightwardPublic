using System;
using JetBrains.Annotations;
using Mirror;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace TonyDev.Game.Level.Rooms.RoomControlScripts
{
    public class LevelItemSpawner : MonoBehaviour
    {
        public enum ItemSpawnType
        {
            Chest,
            Item
        }

        public enum ItemGenerateSetting
        {
            FromItemData,
            FromGenerated
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawSphere(transform.position, 0.5f);
        }

        //Inspector
        [Tooltip("Should the item be spawned on start?")]
        public bool autoSpawn = true;

        [Tooltip("The form to spawn the item in. Chest or Item.")]
        public ItemSpawnType spawnType = ItemSpawnType.Chest;

        [Tooltip("How should the item be generated?")]
        public ItemGenerateSetting generateSetting = ItemGenerateSetting.FromGenerated;

        [Tooltip("Should the item type be chosen or random?")]
        public bool randomItemType = true;

        [Tooltip("The item type to generate.")]
        public ItemType generateItemType;

        [Tooltip("Should the rarity be random or pre-determined?")]
        public bool randomRarity = true;

        [Tooltip("The rarity of item to generate.")]
        public ItemRarity generateRarity;

        [Tooltip("The amount to boost the rarity of the generated item by.")]
        public int rarityBoost;

        [Tooltip("Item data to be spawned. Can be null.")] [CanBeNull]
        public ItemData itemData;

        [Tooltip("Multiplies the cost of the item.")]
        public float costMultiplier = 1;

        [Tooltip("Called on the server when a ground item is picked up or a chest is opened.")]
        public UnityEvent onItemInteractServer;
        //

        public bool spawned;

        private void Start()
        {
            if (!NetworkServer.active)
            {
                Destroy(gameObject);
                return;
            }

            if (autoSpawn)
            {
                SpawnItem();
            }
        }

        [Server]
        public void SpawnItem()
        {
            if (spawned) return; //Don't spawn twice.

            var parent = transform.root.GetComponent<NetworkIdentity>(); //Parent is the root netIdentity

            Item item = null;

            switch (generateSetting)
            {
                case ItemGenerateSetting.FromGenerated:
                    
                    item = ItemGenerator.GenerateItemOfType(randomItemType ? Item.RandomItemType : generateItemType,
                        randomRarity ? Item.RandomRarity(rarityBoost) : generateRarity); //Generate a random item of proper specifications
                    
                    break;
                case ItemGenerateSetting.FromItemData:
                    if (itemData == null)
                    {
                        Debug.LogWarning("Can't spawn null item!");
                        return;
                    }

                    item = itemData.item;
                    break;
            }

            if (item == null)
            {
                Debug.LogWarning("Can't spawn null item!");
                return;
            }

            switch (spawnType)
            {
                case ItemSpawnType.Chest: //If spawn a chest...
                    var chest = ObjectSpawner.SpawnChest(item, transform.position, parent); //Spawn a chest
                    chest.onOpenServer.AddListener(() =>
                        onItemInteractServer.Invoke()); //Invoke interact event when the chest is opened
                    break;
                case ItemSpawnType.Item: //If spawn an item...

                    var groundItem =
                        ObjectSpawner.SpawnGroundItem(item, costMultiplier, transform.position, parent); //Spawn a ground item
                    groundItem.onPickupServer.AddListener(() =>
                        onItemInteractServer.Invoke()); //Invoke interact event when the item is picked up

                    break;
            }

            spawned = true;
        }
    }
}