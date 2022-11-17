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
        Nothing
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
            RpcRoll(Random.Range(3, 7));
        }

        [ClientRpc]
        private void RpcRoll(int rollCount)
        {
            _maxRolls = rollCount;
            foreach (var s in slots)
            {
                s.PlayAnimation();
            }

            _rolling = true;
        }

        [ServerCallback]
        private void GiveReward()
        {
            if (slots.Count >= maxSlots)
            {
                outcomes = outcomes.Where(o => o.outcome != SlotOutcome.ExtraSlot).ToArray();
            }
            
            _rolls = 0;
            _maxRolls = Random.Range(3, 7);

            var determinedOutcome = Tools.SelectRandom(outcomes);

            CmdSetReward(determinedOutcome.outcome);

            switch (determinedOutcome.outcome)
            {
                case SlotOutcome.Chest:
                    foreach (var s in slots)
                    {
                        ObjectSpawner.SpawnChest(0, (Vector2) s.transform.position - new Vector2(0, 1f), netIdentity);
                    }
                    break;
                case SlotOutcome.Coins:
                    foreach (var s in slots)
                    {
                        GameManager.Instance.CmdSpawnMoney((int) (ItemGenerator.DungeonInteractMultiplier * 15),
                            (Vector2) s.transform.position - new Vector2(0, 1f), netIdentity);
                    }
                    break;
                case SlotOutcome.Time:
                    FindObjectOfType<WaveManager>().StallTime(20 * slots.Count);
                    break;
                case SlotOutcome.Enemies:
                    foreach (var s in slots)
                    {
                        ObjectSpawner.SpawnEnemy(Tools.SelectRandom(ObjectFinder.EnemyPrefabs), (Vector2) s.transform.position - new Vector2(0, 1f), netIdentity);
                    }
                    break;
                case SlotOutcome.ExtraSlot:
                    CmdAddSlot();
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
            
            SoundManager.PlaySound("die", slots[0].transform.position);
            
            foreach (var s in slots)
            {
                if (outcome != SlotOutcome.Nothing)
                {
                    s.SetEntry(outcomes.FirstOrDefault(o => o.outcome == outcome));
                }
                else
                {
                    var chosen = Tools.SelectRandom(repeatless);

                    repeatless.Remove(chosen);
                    
                    s.SetEntry(chosen);
                }
            }
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

            var slotXOffset = (slots.Count % 2 == 1 ? -1 : 1) * 1.5f * slots.Count / 2;

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

            SoundManager.PlaySound("hit", slots[0].transform.position);

            var unusedEntries = outcomes.ToList();
            
            foreach (var s in slots)
            {
                var outcome = Tools.SelectRandom(unusedEntries);
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