using UnityEngine;
using UnityEngine.Tilemaps;

namespace TonyDev.Game.Level.Tiles.Walls
{
    [CreateAssetMenu]
    public class WallTile : RuleTile
    {
        public override bool RuleMatch(int neighbor, TileBase other)
        {
            if (other is RuleOverrideTile ot)
                other = ot.m_InstanceTile;

            switch (neighbor)
            {
                case TilingRuleOutput.Neighbor.This: return other != null;
                case TilingRuleOutput.Neighbor.NotThis: return other == null;
            }
            return true;
        }
    }
}
