using TonyDev.Game.Core.Combat;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev.Game.Core.Entities.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        //Editor variables
        [SerializeField] private PlayerSlashController playerSlashController;
    
        public static PlayerCombat Instance;

        private void Awake()
        {
            //Singleton code
            if (Instance == null && Instance != this) Instance = this;
            else Destroy(this);
            //
        }

        private void Update()
        {
            playerSlashController.enabled = Player.Instance.IsAlive; //Disabled slashing when the player is dead
        }
    }
}
