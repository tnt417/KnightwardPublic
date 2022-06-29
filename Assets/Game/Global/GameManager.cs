using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core;
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

        public static List<Item> AllItems = new List<Item>();
        public static float CrystalHealth = 50f;
        public static int Money = 1000;
        public static readonly List<Enemy> Enemies = new ();
        public static readonly List<EnemySpawner> EnemySpawners = new ();
        public static int EnemyDifficultyScale => Mathf.CeilToInt(Timer.GameTimer / 60f); //Enemy difficulty scale. Goes up by 1 every minute.
        public static GamePhase GamePhase;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject); //Persist between scenes
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (AllItems.Count >= 0) AllItems = itemData.Select(t => t.item).ToList();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) TogglePhase(); //Toggle the phase when R is pressed

            if (CrystalHealth <= 0) SceneManager.LoadScene("GameOver"); //Lose the game when the crystal dies
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
        }

        //Teleports the player to the dungeon, sets the starting room as active, and sets the GamePhase to Dungeon.
        private void EnterDungeonPhase()
        {
            GamePhase = GamePhase.Dungeon;
            Player.Instance.gameObject.transform.position = Vector3.zero;
            RoomManager.Instance.SetActiveRoom((RoomManager.Instance.MapSize-1)/2, (RoomManager.Instance.MapSize-1)/2);
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
