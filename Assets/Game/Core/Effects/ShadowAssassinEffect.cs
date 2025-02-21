using System.Collections.Generic;
using System.Linq;
using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Core.Effects;
using TonyDev.Game.Core.Entities.Player;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TonyDev
{
    public class ShadowAssassinEffect : GameEffect
    {
        public float CloneCooldown = 6f;
        private float NextCloneTime = 0f;
        public float ShurikenCooldown = 3f;
        private float NextShurikenTime = 0f;
        public float SlashCooldown = 2.5f;
        private float NextSlashTime = 0f;

        private ProjectileData _shurikenData = new ProjectileData()
        {
            attackData = new AttackData()
            {
                damageMultiplier = 1f,
                destroyOnApply = false,
                destroyOnCollideWall = false,
                hitboxRadius = 0.25f,
                ignoreInvincibility = false,
                inflictEffects = new(),
                knockbackMultiplier = 0f,
                lifetime = -1f,
                spawnOnDestroyKey = "",
                team = Team.Player
            },
            childOfOwner = false,
            disableMovement = false,
            doNotRotate = true,
            effects = new List<GameEffect>(),
            offsetDegrees = 0f,
            prefabKey = "ShadowShuriken",
            travelSpeed = 10f
        };

        private List<GameObject> _clones = new();

        public override void OnAddClient()
        {
        }

        public override void OnRemoveClient()
        {
        }

        public override void OnUpdateClient()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame && Time.time > NextShurikenTime)
            {
                _clones = _clones.Where(go => go != null && go.activeInHierarchy).ToList();
                
                NextShurikenTime = Time.time + ShurikenCooldown;

                foreach (var go in _clones.ToList().Append(Entity.gameObject))
                {
                    var originPos = (Vector2)go.transform.position;
                    var direction = (GameManager.MousePosWorld - originPos).normalized;
                    ObjectSpawner.SpawnProjectile(Entity, originPos, direction,
                        _shurikenData);
                }
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                _clones = _clones.Where(go => go != null && go.activeInHierarchy).ToList();

                if (Time.time > NextCloneTime)
                {
                    NextCloneTime = Time.time + CloneCooldown;
                    SpawnClone();
                }
                else if (_clones.Any())
                {
                    var clonePos = _clones.First().transform.position;
                    _clones.First().GetComponent<ShadowCloneController>().Swap(Entity.transform.position);
                    Player.LocalInstance.transform.position = clonePos;
                }
            }

            if (Keyboard.current.digit3Key.wasPressedThisFrame && Time.time > NextSlashTime)
            {
                _clones = _clones.Where(go => go != null && go.activeInHierarchy).ToList();
                
                NextSlashTime = Time.time + SlashCooldown;

                foreach (var go in _clones.ToList().Append(Entity.gameObject))
                {
                    var data = new ProjectileData()
                    {
                        attackData = new AttackData()
                        {
                            damageMultiplier = 1f,
                            destroyOnApply = false,
                            destroyOnCollideWall = false,
                            hitboxRadius = 1f,
                            ignoreInvincibility = false,
                            inflictEffects = new(),
                            knockbackMultiplier = 0f,
                            lifetime = -1f,
                            spawnOnDestroyKey = "",
                            team = Team.Player
                        },
                        childOfOwner = go.GetComponent<Player>() != null,
                        disableMovement = true,
                        doNotRotate = true,
                        effects = new List<GameEffect>(),
                        offsetDegrees = 0f,
                        prefabKey = "ShadowSlash",
                        travelSpeed = 0f
                    };
                    
                    var originPos = (Vector2)go.transform.position;
                    ObjectSpawner.SpawnProjectile(Entity, originPos, Vector2.zero,
                        data);
                }
            }
        }

        private void SpawnClone()
        {
            var clone = Object.Instantiate(ObjectFinder.GetPrefab("ShadowClone"), Entity.transform.position,
                Quaternion.identity);

            var cont = clone.GetComponent<ShadowCloneController>();

            cont.Set(GameManager.MouseDirection,
                Mathf.Clamp(Vector2.Distance(Player.LocalInstance.transform.position,
                    GameManager.MousePosWorld), 0, 4f));

            _clones.Add(clone);
        }
    }
}