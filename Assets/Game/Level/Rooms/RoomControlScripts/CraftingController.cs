using Mirror;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Button;
using TonyDev.Game.Level.Decorations.Slots;
using UnityEngine;

namespace TonyDev.Game.Level.Rooms.RoomControlScripts
{
    public class CraftingController : NetworkBehaviour
    {
        [SerializeField] private Slot typeSlot;
        [SerializeField] private Slot raritySlot;

        [SerializeField] private SlotEntry[] typeEntries;
        [SerializeField] private SlotEntry[] rarityEntries;

        [SerializeField] private NetworkedInteractable purchaseButton;

        [SerializeField] private ItemData defaultItem;

        [SyncVar(hook = nameof(TypeIndexHook))]
        private int _typeIndex;

        [SyncVar(hook = nameof(RarityIndexHook))]
        private int _rarityIndex;

        [SerializeField] private Transform itemSpawnPos;
        
        private ItemRarity GetRarity()
        {
            return rarityEntries[_rarityIndex].outcome switch
            {
                SlotOutcome.Common => ItemRarity.Common,
                SlotOutcome.Uncommon => ItemRarity.Uncommon,
                SlotOutcome.Rare => ItemRarity.Rare,
                SlotOutcome.Unique => ItemRarity.Unique,
                _ => default
            };
        }

        private ItemType GetItemType()
        {
            return typeEntries[_typeIndex].outcome switch
            {
                SlotOutcome.Weapon => ItemType.Weapon,
                SlotOutcome.Tower => ItemType.Tower,
                SlotOutcome.Relic => ItemType.Relic,
                _ => default
            };
        }
        
        private void TypeIndexHook(int oldIndex, int newIndex)
        {
            typeSlot.PlayAnimation();
            typeSlot.SetEntry(typeEntries[newIndex]);
        }
        
        private void RarityIndexHook(int oldIndex, int newIndex)
        {
            raritySlot.PlayAnimation();
            raritySlot.SetEntry(rarityEntries[newIndex]);
            purchaseButton.interactable.SetCost(ItemGenerator.GenerateCost(GetRarity(), GameManager.DungeonFloor));
        }

        public override void OnStartClient()
        {
            raritySlot.SetEntry(rarityEntries[0]);
            typeSlot.SetEntry(typeEntries[0]);
        }

        public void RollType()
        {
            CmdRollType();
        }

        [Command(requiresAuthority = false)]
        private void CmdRollType()
        {
            _typeIndex = (_typeIndex + 1) % typeEntries.Length;
        }

        public void RollRarity()
        {
            CmdRollRarity();
        }

        [Command(requiresAuthority = false)]
        private void CmdRollRarity()
        {
            _rarityIndex = (_rarityIndex + 1) % rarityEntries.Length;
        }

        private GroundItem _groundItem;
        
        public override void OnStartServer()
        {
            _groundItem = ObjectSpawner.SpawnGroundItem(ItemGenerator.GenerateItemFromData(defaultItem), 1000, itemSpawnPos.position, netIdentity);
        }

        public void Generate()
        {
            CmdGenerate();
        }

        private bool _generated = false;
        
        [Command(requiresAuthority = false)]
        private void CmdGenerate()
        {
            if (_generated) return;
            _generated = true;
            var item = ItemGenerator.GenerateItemOfType(GetItemType(), GetRarity());
            _groundItem.CmdSetItem(item);
            _groundItem.CmdSetCost(-ItemGenerator.GenerateSellPrice(item, GameManager.DungeonFloor));
            RpcGenerated();
        }

        private void RpcGenerated()
        {
            purchaseButton.DestroyInteractableObjectAll();
        }

        [ServerCallback]
        private void OnDestroy()
        {
            NetworkServer.Destroy(_groundItem.gameObject);
        }
    }
}
