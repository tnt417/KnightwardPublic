using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core;
using TonyDev.Game.Core.Entities;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Core.Items;
using TonyDev.Game.Level;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.Global
{
    public enum GamePhase{
        Arena, Dungeon
    }
    public class GameManager : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private List<ItemData> itemData;
        //

        public static List<Item> AllItems = new ();
        public static float CrystalHealth = 1000f;
        public static int Money = 0;
        public static List<GameEntity> Entities = new ();
        public static readonly List<EnemySpawner> EnemySpawners = new ();
        public static int EnemyDifficultyScale => Mathf.CeilToInt(Timer.GameTimer / 60f); //Enemy difficulty scale. Goes up by 1 every minute.
        public static GamePhase GamePhase;

        public static void Reset()
        {
            CrystalHealth = 1000f;
            Money = 0;
            Timer.GameTimer = 0;
            AllItems.Clear();
            Entities.Clear();
            EnemySpawners.Clear();
            PlayerStats.ClearStatBonuses();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject); //Persist between scenes
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (AllItems.Count >= 0) AllItems = itemData.Select(t => t.item).ToList();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) TogglePhase(); //Toggle the phase when R is pressed

            if (CrystalHealth <= 0) GameOver(); //Lose the game when the crystal dies
            
            if(PlayerDeath.Dead && GamePhase == GamePhase.Dungeon) EnterArenaPhase();
        }

        //Switches back and forth between Arena and Dungeon phases
        private void TogglePhase()
        {
            if(GamePhase == GamePhase.Arena) EnterDungeonPhase();
            else if(RoomManager.Instance.InStartingRoom) EnterArenaPhase();
        }

        //Teleports player to the arena and sets GamePhase to Arena.
        private void EnterArenaPhase()
        {
            RoomManager.Instance.DeactivateRoomPhase();
            GamePhase = GamePhase.Arena;
            Player.Instance.gameObject.transform.position =
                GameObject.FindGameObjectWithTag("Castle").transform.position;
            ReTargetEnemies();
        }

        //Teleports the player to the dungeon, sets the starting room as active, and sets the GamePhase to Dungeon.
        private void EnterDungeonPhase()
        {
            GamePhase = GamePhase.Dungeon;
            RoomManager.Instance.TeleportPlayerToStart();
            ReTargetEnemies();
        }

        private void ReTargetEnemies()
        {
            foreach (var e in Entities)
            {
                e.UpdateTarget();
            }
        }

        private void GameOver()
        {
            Destroy(Player.Instance.gameObject);
            Destroy(gameObject);
            SceneManager.LoadScene("GameOver");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "CastleScene") //When the castle scene loads, start the arena phase.
            {
                EnterArenaPhase();
            }
        }
    }
}
