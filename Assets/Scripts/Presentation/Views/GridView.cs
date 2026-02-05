using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
public class GridView : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private RectTransform rectTransform;

    private void Awake()
    {
        if (gridLayout == null)
            gridLayout = GetComponent<GridLayoutGroup>();
        
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }

    public void SetupGrid(int rows, int columns, Vector2 cardSize)
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        
        CalculateAndApplyLayout(rows, columns, cardSize);
    }

    private void CalculateAndApplyLayout(int rows, int columns, Vector2 desiredCardSize)
    {
        if (rectTransform == null)
            return;

        Rect rect = rectTransform.rect;
        float width = rect.width;
        float height = rect.height;
        float aspect = desiredCardSize.x / desiredCardSize.y;

        float cellW = (width * 0.8f) / columns;
        float spacing = columns > 1 ? (width * 0.1f) / (columns - 1) : 0;
        float cellH = cellW / aspect;

        float totalH = cellH * rows + spacing * (rows - 1);

        if (totalH > height * 0.9f)
        {
            cellH = (height * 0.8f) / rows;
            spacing = rows > 1 ? (height * 0.1f) / (rows - 1) : 0;
            cellW = cellH * aspect;
        }

        gridLayout.cellSize = new Vector2(cellW, cellH);
        gridLayout.spacing = new Vector2(spacing, spacing);
    }
}
