using System;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Level.Rooms.RoomControlScripts;
using UnityEditor;

namespace TonyDev.Editor
{
    [CustomEditor(typeof(LevelItemSpawner)), CanEditMultipleObjects]
    public class ItemSpawnerEditor : UnityEditor.Editor
    {
        private SerializedProperty _autoSpawn;
        private SerializedProperty _spawnType;
        private SerializedProperty _generateSetting;
        private SerializedProperty _randomItemType;
        private SerializedProperty _generateItemType;
        private SerializedProperty _randomRarity;
        private SerializedProperty _generateRarity;
        private SerializedProperty _rarityBoost;
        private SerializedProperty _itemData;
        private SerializedProperty _costMultiplier;
        private SerializedProperty _onItemInteractServer;
        //private SerializedProperty _onItemInteractGlobal;
        private SerializedProperty _spawned;
        
        private void OnEnable()
        {
            _autoSpawn = serializedObject.FindProperty(nameof(LevelItemSpawner.autoSpawn));
            _spawnType = serializedObject.FindProperty(nameof(LevelItemSpawner.spawnType));
            _generateSetting = serializedObject.FindProperty(nameof(LevelItemSpawner.generateSetting));
            _randomItemType = serializedObject.FindProperty(nameof(LevelItemSpawner.randomItemType));
            _generateItemType = serializedObject.FindProperty(nameof(LevelItemSpawner.generateItemTypes));
            _randomRarity = serializedObject.FindProperty(nameof(LevelItemSpawner.randomRarity));
            _generateRarity = serializedObject.FindProperty(nameof(LevelItemSpawner.generateRarity));
            _rarityBoost = serializedObject.FindProperty(nameof(LevelItemSpawner.rarityBoost));
            _itemData = serializedObject.FindProperty(nameof(LevelItemSpawner.itemDataPool));
            _costMultiplier = serializedObject.FindProperty(nameof(LevelItemSpawner.costMultiplier));
            _onItemInteractServer = serializedObject.FindProperty(nameof(LevelItemSpawner.onItemInteractServer));
            //_onItemInteractGlobal = serializedObject.FindProperty(nameof(LevelItemSpawner.onItemInteractGlobal));
            _spawned = serializedObject.FindProperty(nameof(LevelItemSpawner.spawned));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_spawned.boolValue)
            {
                EditorGUILayout.LabelField("Item has been spawned! Can no longer edit values. Component still needed to call events.");
            }
            else
            {
                EditorGUILayout.PropertyField(_autoSpawn);
                EditorGUILayout.PropertyField(_spawnType);

                var spawnSetting = (LevelItemSpawner.ItemSpawnType) _spawnType.enumValueIndex;

                if (spawnSetting == LevelItemSpawner.ItemSpawnType.Item) EditorGUILayout.PropertyField(_costMultiplier);

                EditorGUILayout.PropertyField(_generateSetting);

                var genSetting = (LevelItemSpawner.ItemGenerateSetting) _generateSetting.enumValueIndex;

                switch (genSetting)
                {
                    case LevelItemSpawner.ItemGenerateSetting.FromGenerated:
                        ShowItemGenerateOptions();
                        break;
                    case LevelItemSpawner.ItemGenerateSetting.FromItemData:
                        ShowPresetItemOptions();
                        break;
                }

                EditorGUILayout.PropertyField(_onItemInteractServer);
                //EditorGUILayout.PropertyField(_onItemInteractGlobal);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowItemGenerateOptions()
        {
            EditorGUILayout.PropertyField(_randomItemType);

            if (!_randomItemType.boolValue)
            {
                EditorGUILayout.PropertyField(_generateItemType);
            }
            
            EditorGUILayout.PropertyField(_randomRarity);

            EditorGUILayout.PropertyField(_randomRarity.boolValue ? _rarityBoost : _generateRarity);
        }

        private void ShowPresetItemOptions()
        {
            EditorGUILayout.PropertyField(_itemData);
        }
    }
}
