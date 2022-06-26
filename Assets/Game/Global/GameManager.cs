using System.Collections.Generic;
using TonyDev.Game.Core;
using TonyDev.Game.Core.Entities.Enemies;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Level;
using TonyDev.Game.Level.Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TonyDev.Game.Global
{
    public class GameManager : MonoBehaviour
    {
        public static float CrystalHealth = 1000f;
        public static readonly List<Enemy> Enemies = new ();
        public static readonly List<EnemySpawner> EnemySpawners = new ();
        public static int EnemyDifficultyScale => Mathf.CeilToInt(Timer.GameTimer / 60f); //Enemy difficulty scale. Goes up by 1 every minute.
        private void Awake()
        {
            DontDestroyOnLoad(gameObject); //Persist between scenes
        }

        private void FixedUpdate()
        {
            /*Description:
         !TEMPORARY! This code switches between the rooms and castles, once all enemies and spawners are cleared.
         */
            return;
            if ((WaveManager.Instance != null && WaveManager.Instance.wavesSpawned >= 5 || Enemies.Count == 0 && EnemySpawners.Count == 0) && !TransitionController.Instance.InSceneTransition)
            {
                switch (RoomManager.InRoomsPhase)
                {
                    case false:
                        TransitionController.Instance.LoadScene("RoomScene");
                        break;
                    case true:
                        TransitionController.Instance.LoadScene("CastleScene");
                        Player.Instance.transform.position = Vector3.zero;
                        break;
                }
            }

            if (CrystalHealth <= 0)
            {
                Timer.Stop();
                SceneManager.LoadScene("Scenes/GameOver");
                Destroy(gameObject); //Destroy the DontDestroyOnLoad things
                Destroy(FindObjectOfType<Player>().gameObject); //Destroy the Player
            }
        }
    }
}
