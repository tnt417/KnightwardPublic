using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static List<Enemy> Enemies = new List<Enemy>();
    public static List<EnemySpawner> EnemySpawners = new List<EnemySpawner>();
    private void Awake()
    {
        DontDestroyOnLoad(gameObject); //Persist between scenes
    }

    private void FixedUpdate()
    {
        /*Description:
         !TEMPORARY! This code switches between the rooms and castles, once all enemies and spawners are cleared.
         */
        if (Enemies.Count == 0 && EnemySpawners.Count == 0 && !TransitionController.Instance.InSceneTransition)
        {
            switch (RoomManager.InRoomsPhase)
            {
                case false:
                    TransitionController.Instance.LoadScene("RoomScene");
                    break;
                case true:
                    TransitionController.Instance.LoadScene("CastleScene");
                    break;
            }
        }
    }
}
