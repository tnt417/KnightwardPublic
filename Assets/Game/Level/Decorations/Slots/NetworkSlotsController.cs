using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Global;
using TonyDev.Game.Level.Decorations.Button;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Level.Decorations.Slots
{
    public enum SlotOutcome
    {
        Coins,
        Time,
        Enemies,
        Chest,
        ExtraSlot,
        Nothing,
        Weapon,
        Relic,
        Tower,
        Common,
        Uncommon,
        Rare,
        Unique
    }

    [Serializable]
    public class SlotEntry
    {
        public Sprite slotSprite;
        public SlotOutcome outcome;
    }

    public class NetworkSlotsController : NetworkBehaviour
    {
        private int _rolls;
        private int _maxRolls;

        public List<Slot> slots;

        public SlotEntry[] outcomes;

        private bool _rolling;

        [SerializeField] private GameObject slotObject;
        [SerializeField] private NetworkedInteractable buttonInteractable;

        [SerializeField] private int maxSlots;

        private void Start()
        {
            slots[0].SwapImageAction += () => ProgressRollAnimation().Forget();
        }

        public void Roll()
        {
            CmdRoll();
        }

        [Command(requiresAuthority = false)]
        private void CmdRoll()
        {
            RpcRoll(Random.Range(2, 3));
        }

        [ClientRpc]
        private void RpcRoll(int rollCount)
        {
            buttonInteractable.interactable.enabled = false;

            _rolls = 0;
            _maxRolls = rollCount;
            foreach (var s in slots)
            {
                s.PlayAnimation();
            }

            _rolling = true;
        }

        [SerializeField] private List<GameObject> enemyPossibilities;

        [ServerCallback]
        private void GiveReward()
        {
            var validOutcomes = outcomes;

            if (slots.Count >= maxSlots)
            {
                validOutcomes = outcomes.Where(o => o != null && o.outcome != SlotOutcome.ExtraSlot).ToArray();
            }

            var determinedOutcome = GameTools.SelectRandom(validOutcomes);

            CmdSetReward(determinedOutcome.outcome);

            switch (determinedOutcome.outcome)
            {
                case SlotOutcome.Weapon:
                    ObjectSpawner.SpawnGroundItem(
                        ItemGenerator.GenerateItemOfType(ItemType.Weapon, Item.RandomRarity(slots.Count * 10)), 0,
                        (Vector2) transform.position - new Vector2(0, 1f), netIdentity);
                    break;
                case SlotOutcome.Relic:
                    ObjectSpawner.SpawnGroundItem(
                        ItemGenerator.GenerateItemOfType(ItemType.Relic, Item.RandomRarity(slots.Count * 10)), 0,
                        (Vector2) transform.position - new Vector2(0, 1f), netIdentity);
                    break;
                case SlotOutcome.Tower:
                    ObjectSpawner.SpawnGroundItem(
                        ItemGenerator.GenerateItemOfType(ItemType.Tower, Item.RandomRarity(slots.Count * 10)), 0,
                        (Vector2) transform.position - new Vector2(0, 1f), netIdentity);
                    break;
                case SlotOutcome.Coins:
                    foreach (var s in slots)
                    {
                        GameManager.Instance.CmdSpawnMoney((int) (ItemGenerator.DungeonInteractMultiplier * 15),
                            (Vector2) s.transform.position - new Vector2(0, 1f), netIdentity);
                    }

                    break;
                case SlotOutcome.Enemies:
                    foreach (var s in slots)
                    {
                        ObjectSpawner.SpawnEnemy(GameTools.SelectRandom(enemyPossibilities),
                            (Vector2) s.transform.position - new Vector2(0, 1f), netIdentity);
                        GameManager.Instance.CmdSpawnMoney((int) (ItemGenerator.DungeonInteractMultiplier * 15),
                            (Vector2) s.transform.position - new Vector2(0, 1f), netIdentity);
                    }

                    break;
                case SlotOutcome.ExtraSlot:
                    if (slots.Count < maxSlots) CmdAddSlot();
                    break;
                case SlotOutcome.Nothing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (determinedOutcome.outcome != SlotOutcome.ExtraSlot && determinedOutcome.outcome != SlotOutcome.Nothing)
            {
                buttonInteractable.DestroyInteractableObjectAll();
            }

            _rolling = false;
        }

        [Command(requiresAuthority = false)]
        private void CmdSetReward(SlotOutcome outcome)
        {
            RpcSetReward(outcome);
        }

        [ClientRpc]
        private void RpcSetReward(SlotOutcome outcome)
        {
            var repeatless = outcomes.ToList();

            SoundManager.PlaySound("die", 0.5f, slots[0].transform.position);

            foreach (var s in slots)
            {
                if (outcome != SlotOutcome.Nothing)
                {
                    s.SetEntry(outcomes.FirstOrDefault(o => o.outcome == outcome));
                }
                else
                {
                    var chosen = GameTools.SelectRandom(repeatless);

                    repeatless.Remove(chosen);

                    s.SetEntry(chosen);
                }
            }

            DelayEnable().Forget();
        }

        private async UniTask DelayEnable()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.1f));
            if (buttonInteractable != null && buttonInteractable.interactable != null) buttonInteractable.interactable.enabled = true;
        }

        [Command(requiresAuthority = false)]
        private void CmdAddSlot()
        {
            RpcAddSlot();
        }

        [ClientRpc]
        private void RpcAddSlot()
        {
            var go = Instantiate(ObjectFinder.GetPrefab("slot"), slotObject.transform);

            slots.Add(go.GetComponent<Slot>());

            var slotXOffset = (slots.Count % 2 == 1 ? -1 : 1) * 1.5f * Mathf.FloorToInt(slots.Count / 2f);

            go.transform.position = (Vector2) slotObject.transform.position + new Vector2(slotXOffset, 0);
        }

        private async UniTask ProgressRollAnimation()
        {
            if (!_rolling) return;

            if (_rolls > _maxRolls)
            {
                GiveReward();
                return;
            }

            SoundManager.PlaySound("hit", 0.5f, slots[0].transform.position);

            var unusedEntries = outcomes.ToList();

            foreach (var s in slots)
            {
                var outcome = GameTools.SelectRandom(unusedEntries);
                s.SetEntry(outcome);
                unusedEntries.Remove(outcome);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            foreach (var s in slots)
            {
                s.PlayAnimation();
            }

            _rolls++;
        }
    }
}