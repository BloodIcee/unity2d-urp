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

    public void SetupGrid(int rows, int columns, Vector2 cardSize, Vector2 spacing)
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
        gridLayout.cellSize = cardSize;
        gridLayout.spacing = spacing;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        
        AdjustCellSizeToFit(rows, columns);
    }

    private void AdjustCellSizeToFit(int rows, int columns)
    {
        if (rectTransform == null)
            return;

        Rect rect = rectTransform.rect;
        float availableWidth = rect.width;
        float availableHeight = rect.height;

        float totalSpacingX = gridLayout.spacing.x * (columns - 1);
        float totalSpacingY = gridLayout.spacing.y * (rows - 1);

        float maxCellWidth = (availableWidth - totalSpacingX) / columns;
        float maxCellHeight = (availableHeight - totalSpacingY) / rows;

        float cellSize = Mathf.Min(maxCellWidth, maxCellHeight);
        cellSize *= 0.95f;

        gridLayout.cellSize = new Vector2(cellSize, cellSize * 1.3f);
    }

    public void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
