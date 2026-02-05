using UnityEngine;
using TMPro;
using DG.Tweening;

public class ScoreView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI matchesText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private CanvasGroup comboGroup;

    private void Awake()
    {
        if (comboGroup != null)
            comboGroup.alpha = 0f;
    }

    private Tween comboColorTween;

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.SetText($"Score: {score}");
            scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        }
    }

    public void UpdateCombo(int combo)
    {
        if (comboText == null || comboGroup == null)
            return;

        if (combo > 1)
        {
            comboText.SetText($"Combo x{combo}!");
            comboGroup.alpha = 1f;
            
            comboText.transform.DOKill();
            comboText.transform.localScale = Vector3.one;
            comboText.transform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 10, 1);

            if (comboColorTween == null || !comboColorTween.IsActive())
            {
                comboColorTween = DOVirtual.Float(0f, 1f, 1f, (value) => {
                    if (comboText != null)
                        comboText.color = Color.HSVToRGB(value, 0.8f, 1f);
                }).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
            }
        }
        else
        {
            comboGroup.DOFade(0f, 0.2f);
            
            comboColorTween?.Kill();
            if (comboText != null)
                comboText.color = Color.white;
        }
    }

    public void UpdateMatches(int matches)
    {
        if (matchesText != null)
        {
            matchesText.SetText($"Matches: {matches}");
            matchesText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        }
    }

    public void UpdateMoves(int moves)
    {
        if (movesText != null)
        {
            movesText.SetText($"Moves: {moves}");
            
            if (moves <= 3)
                movesText.color = Color.red;
            else
                movesText.color = Color.white;
                
            movesText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        }
    }
}
