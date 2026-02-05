using UnityEngine;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Card Game/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Grid Layouts")]
    public GridLayout[] SupportedLayouts;
    public int DefaultLayoutIndex = 0;
    
    [Header("Card Settings")]
    public Vector2 CardSize = new Vector2(1.8f, 2.4f);
    public Vector2 CardSpacing = new Vector2(0.2f, 0.2f);
    
    [Header("Card Sprites")]
    public SpriteAtlas CardAtlas;
    public Sprite CardBaseSprite;
    public Sprite CardBackSprite;
    
    [Header("Animation Timings")]
    public float CardFlipDuration = 0.3f;
    public float MatchDelay = 0.5f;
    public float MismatchDelay = 1.0f;
    public float StaggerDelay = 0.05f;
    
    [Header("Scoring")]
    public int BaseMatchScore = 100;
    public float ComboMultiplier = 1.5f;
    public int MismatchPenalty = 10;

    public GridLayout GetDefaultLayout()
    {
        if (SupportedLayouts.Length == 0)
            return new GridLayout { Name = "2x2", Rows = 2, Columns = 2 };
        
        return SupportedLayouts[DefaultLayoutIndex];
    }

    public Sprite GetSpriteFromAtlas(string spriteName)
    {
        if (CardAtlas == null)
            return null;
        
        return CardAtlas.GetSprite(spriteName);
    }
}
