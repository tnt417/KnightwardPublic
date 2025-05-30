using TonyDev.Game.Core.Attacks;
using TonyDev.Game.Global;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TonyDev.Game.Core.Entities.Towers.Barrier
{
    public class BarrierTower : Tower
    {
        public override Team Team => Team.Player;
        public override bool IsInvulnerable => false;
        public override bool IsTangible => true;

        private static Tilemap _wallTilemap;
        private static TilemapCollider2D _compositeCollider;

        public TileBase tile;

        public new void Start()
        {
            base.Start();

            //TODO: maybe breaks with celestial wrench?
            if (_wallTilemap == null || _wallTilemap.gameObject == null)
            {
                _wallTilemap = GameManager.Instance.arenaWallTilemap;
                _compositeCollider = _wallTilemap.GetComponent<TilemapCollider2D>();
            }
            
            _wallTilemap.SetTile(_wallTilemap.WorldToCell(gameObject.transform.position), tile);
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            OnHurtOwner += OnHurt;
        }

        public override void OnStopAuthority()
        {
            base.OnStopAuthority();
            
            OnHurtOwner -= OnHurt;
        }

        private void OnHurt(float damage)
        {
            return;
            if (CurrentHealth <= 0)
            {
                Die();
            }
            else
            {
                SubtractDurability((int)damage);
            }
        }

        public void OnDestroy()
        {
            var pos = gameObject.transform.position;
            
            _wallTilemap.SetTile(_wallTilemap.WorldToCell(pos), null);
            GameManager.Instance?.OccupiedTowerSpots.Remove(new Vector2Int((int)pos.x, (int)pos.y));
            GameManager.RemoveEntity(this);
        }

    }
}
