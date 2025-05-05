using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

//https://www.youtube.com/watch?v=FwOxLkJTXag

[CreateAssetMenu(menuName = "Seasonal/Seasonal Rule Tile")]
public class SeasonalRuleTile : RuleTile<SeasonalRuleTile.Neighbor>
{
    public TileType tileType;
    public Seasons season;
    public TileBase[] tilesToConnect;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        //This = 1, NotThis = 2
        public const int Any = 3;
        public const int NotAny = 4;
        public const int OnlyFirst = 5;
        public const int OnlySecond = 6;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.This: return Check_ThisOnly(tile);
            case Neighbor.NotThis: return Check_NotThis(tile);
            case Neighbor.Any: return Check_Any(tile);
            case Neighbor.NotAny: return Check_NotAny(tile);
            case Neighbor.OnlyFirst: return Check_OnlyFirst(tile);
            case Neighbor.OnlySecond: return Check_OnlySecond(tile);
        }

        return base.RuleMatch(neighbor, tile);
    }

    private bool Check_ThisOnly(TileBase tile)
    {
        return tile == this;
    }

    private bool Check_Any(TileBase tile)
    {
        return tilesToConnect.Contains(tile) || tile == this;
    }

    private bool Check_NotThis(TileBase tile)
    {
        return tile != this;
    }

    private bool Check_NotAny(TileBase tile)
    {
        if (tile == this)
        {
            return false;
        }

        return tile == null || !tilesToConnect.Contains(tile);
    }

    private bool Check_OnlyFirst(TileBase tile)
    {
        return tilesToConnect[0] == tile;
    }

    private bool Check_OnlySecond(TileBase tile)
    {
        return tilesToConnect[1] == tile;
    }

    /*public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
    {
        Matrix4x4 transform = Matrix4x4.identity;
        foreach (TilingRule rule in m_TilingRules)
        {
            if (rule.m_Output == TilingRuleOutput.OutputSprite.Animation)
            {
                if (RuleMatches(rule, position, tilemap, ref transform))
                {
                    Sprite[] sprites = new Sprite[2];
                    Array.Copy(rule.m_Sprites, (int)season * 2, sprites, 0, 2);
                    tileAnimationData.animatedSprites = sprites;
                    tileAnimationData.animationSpeed = UnityEngine.Random.Range(rule.m_MinAnimationSpeed, rule.m_MaxAnimationSpeed);
                    return true;
                }
            }
        }
        return false;
    }*/

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