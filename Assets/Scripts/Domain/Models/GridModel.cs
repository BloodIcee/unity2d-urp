using System.Collections.Generic;

public class GridModel
{
    public int Rows { get; private set; }
    public int Columns { get; private set; }
    public List<CardModel> Cards { get; private set; }

    public int TotalCards => Rows * Columns;

    public GridModel(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        Cards = new List<CardModel>();
    }
}
