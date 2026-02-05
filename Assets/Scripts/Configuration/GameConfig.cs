using UnityEngine;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Card Game/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Grid Layouts")]
    public GridLayout[] SupportedLayouts;
    public int DefaultLayoutIndex = 0;
    
    [Header("Card Settings"), Tooltip("These values are an example for cardds scale")]
    public Vector2 CardSize = new Vector2(1.8f, 2.4f);
    
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
    
    [Header("Moves")]
    [Tooltip("Moves = Pairs * Multiplier + Base")]
    public float MoveCountMultiplier = 2.0f;
    public int BaseMoves = 4;

    public void OnValidate()
    {
        if (SupportedLayouts != null)
        {
            foreach (var layout in SupportedLayouts)
            {
                if (layout != null)
                {
                    layout.Name = $"{layout.Rows}x{layout.Columns}";
                }
            }
        }
    }

    public GridLayout GetDefaultLayout()
    {
        if (SupportedLayouts == null || SupportedLayouts.Length == 0)
            return new GridLayout { Name = "2x2", Rows = 2, Columns = 2 };
        
        return SupportedLayouts[DefaultLayoutIndex];
    }

    public GridLayout GetRandomLayout()
    {
        if (SupportedLayouts == null || SupportedLayouts.Length == 0)
            return new GridLayout { Name = "2x2", Rows = 2, Columns = 2 };

        int randomIndex = Random.Range(0, SupportedLayouts.Length);
        return SupportedLayouts[randomIndex];
    }

    public Sprite GetSpriteFromAtlas(string spriteName)
    {
        if (CardAtlas == null)
            return null;
        
        return CardAtlas.GetSprite(spriteName);
    }
}
