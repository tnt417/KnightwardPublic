using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;

//Provides functionality for randomly generating items
namespace TonyDev.Game.Core.Items
{
    public class ItemGenerator
    {
        private static Sprite[] _weaponSprites; //Possible weapon sprites to pick from
        private static Sprite[] _armorSprites; //Possible armor sprites to pick from
        private static Sprite[] _relicSprites; //Possible relic sprites to pick from

        public static void
            InitSprites() //Load sprites from SpriteDictionary based on the prefix to be used in randomly generated items.
        {
            _weaponSprites = SpriteDictionary.Sprites.Where(s => s.Key.StartsWith("wi_")).Select(s => s.Value)
                .ToArray();
            _armorSprites = SpriteDictionary.Sprites.Where(s => s.Key.StartsWith("ai_")).Select(s => s.Value).ToArray();
            _relicSprites = SpriteDictionary.Sprites.Where(s => s.Key.StartsWith("ri_")).Select(s => s.Value).ToArray();
        }

        //Returns a randomly generated item based on input parameters
        public static Item GenerateEquippableItem(ItemType type, ItemRarity rarity)
        {
            //Instantiate local variables that will end up at the parameters for the generated item.
            var itemName = string.Empty;
            Sprite sprite = null;
            var statBonuses = new List<StatBonus>();
            //

            //Set sprites and item names based on the item type
            switch (type)
            {
                case ItemType.Weapon:
                    sprite = _weaponSprites[Random.Range(0, _weaponSprites.Length)];
                    itemName = "Sword";
                    break;
                case ItemType.Armor:
                    sprite = _armorSprites[Random.Range(0, _armorSprites.Length)];
                    itemName = "Armor";
                    break;
                case ItemType.Relic:
                    sprite = _relicSprites[Random.Range(0, _relicSprites.Length)];
                    itemName = "Relic";
                    break;
            }

            //Set stats based on the item type
            for (var i = 0; i < (int) rarity; i++)
            {
                statBonuses.Add(new StatBonus(PlayerStats.GetValidStatForItem(type),
                    Mathf.Sqrt(GameManager.EnemyDifficultyScale) / 2 * Random.Range(0.01f, 1f), itemName));
            }

            //Return a new item using all the variables this function has generated and/or been provided with
            return new Item()
            {
                itemType = type,
                itemRarity = rarity,
                itemName = itemName,
                statBonuses = statBonuses.ToArray(),
                uiSprite = sprite
            };
        }

        public static Item GenerateItemFromData()
        {
            return null; //TODO
        }

        public static int GenerateCost(Item item)
        {
            return item.itemRarity switch
            {
                ItemRarity.Common => 10 * GameManager.EnemyDifficultyScale,
                ItemRarity.Uncommon => 15 * GameManager.EnemyDifficultyScale,
                ItemRarity.Rare => 20 * GameManager.EnemyDifficultyScale,
                ItemRarity.Unique => 25 * GameManager.EnemyDifficultyScale,
                _ => 0
            };
        }
    }
}