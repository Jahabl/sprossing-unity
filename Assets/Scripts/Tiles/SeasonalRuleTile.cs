using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

//https://www.youtube.com/watch?v=FwOxLkJTXag

[CreateAssetMenu(menuName = "Seasonal/Seasonal Rule Tile")]
public class SeasonalRuleTile : RuleTile<SeasonalRuleTile.Neighbor>
{
    public TileType tileType;
    public Seasons season;
    public bool alwaysConnect;
    public TileBase[] tilesToConnect;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int Any = 3;
        public const int Specific = 4;
        public const int NotSpecific = 5;
        public const int Nothing = 6;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.This: return Check_This(tile);
            case Neighbor.NotThis: return Check_NotThis(tile);
            case Neighbor.Any: return Check_Any(tile);
            case Neighbor.Specific: return Check_Specific(tile);
            case Neighbor.NotSpecific: return Check_NotSpecific(tile);
            case Neighbor.Nothing: return Check_Nothing(tile);
        }

        return base.RuleMatch(neighbor, tile);
    }

    private bool Check_This(TileBase tile)
    {
        if (!alwaysConnect)
        {
            return tile == this;
        }
        else
        {
            return tilesToConnect.Contains(tile) || tile == this;
        }
    }

    private bool Check_NotThis(TileBase tile)
    {
        return tile != this;
    }

    private bool Check_Any(TileBase tile)
    {
        return tile != null || tile != this;
    }

    private bool Check_Specific(TileBase tile)
    {
        return tilesToConnect.Contains(tile);
    }

    private bool Check_NotSpecific(TileBase tile)
    {
        return !tilesToConnect.Contains(tile) && tile != this;
    }

    private bool Check_Nothing(TileBase tile)
    {
        return tile == null;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        var iden = Matrix4x4.identity;

        tileData.sprite = m_DefaultSprite;
        tileData.gameObject = m_DefaultGameObject;
        tileData.colliderType = m_DefaultColliderType;
        tileData.flags = TileFlags.LockTransform;
        tileData.transform = iden;

        Matrix4x4 transform = iden;

        foreach (TilingRule rule in m_TilingRules)
        {
            if (RuleMatches(rule, position, tilemap, ref transform))
            {
                switch (rule.m_Output)
                {
                    case TilingRuleOutput.OutputSprite.Single:
                    case TilingRuleOutput.OutputSprite.Animation:
                        tileData.sprite = rule.m_Sprites[0];
                        break;
                    case TilingRuleOutput.OutputSprite.Random:
                        tileData.sprite = rule.m_Sprites[(int)season];
                        if (rule.m_RandomTransform != TilingRuleOutput.Transform.Fixed)
                            transform = ApplyRandomTransform(rule.m_RandomTransform, transform, rule.m_PerlinScale, position);
                        break;
                }
                tileData.transform = transform;
                tileData.gameObject = rule.m_GameObject;
                tileData.colliderType = rule.m_ColliderType;
                break;
            }
        }
    }
}