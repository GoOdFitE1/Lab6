using System.Collections.Generic;

namespace Lab6;

public class Board
{
    private const int NUM_ROWS = 11;
    private const int NUM_COLS = 8;
    private const float VISUAL_ROW_HEIGHT = 43.3f;
    private const float VISUAL_BUBBLE_DIAMETER = 50f;
    private Bubble[][] rows;
    private Random random = new Random();

    public Board()
    {
        rows = new Bubble[NUM_ROWS][];
        for (int i = 0; i < NUM_ROWS; i++)
        {
            rows[i] = new Bubble[NUM_COLS];
            int startCol = i % 2 == 0 ? 1 : 0;
            for (int j = startCol; j < NUM_COLS; j += 2)
            {
                rows[i][j] = new Bubble(i, j, random.Next(4));
            }
        }
    }

    public Bubble[][] GetRows() => rows;

    public void AddBubble(Bubble bubble, (float x, float y) coords)
    {
        int rowNum = (int)(coords.y / VISUAL_ROW_HEIGHT); // Use constant
        int colNum = (int)(coords.x / VISUAL_BUBBLE_DIAMETER * 2); // Use constant
        if (rowNum % 2 == 1) colNum -= 1;
        colNum = (int)Math.Round(colNum / 2.0) * 2;
        if (rowNum % 2 == 0) colNum -= 1;
        colNum = Math.Max(0, Math.Min(colNum, NUM_COLS - 1));
        rowNum = Math.Max(0, Math.Min(rowNum, NUM_ROWS - 1));
        rows[rowNum][colNum] = bubble;
        bubble.SetRow(rowNum);
        bubble.SetCol(colNum);
    }

    public Bubble GetBubbleAt(int row, int col) => row >= 0 && row < rows.Length && col >= 0 && col < rows[row].Length ? rows[row][col] : null;

    public List<Bubble> GetBubblesAround(int curRow, int curCol)
    {
        var bubbles = new List<Bubble>();
        for (int rowNum = curRow - 1; rowNum <= curRow + 1; rowNum++)
        {
            for (int colNum = curCol - 2; colNum <= curCol + 2; colNum++)
            {
                var bubbleAt = GetBubbleAt(rowNum, colNum);
                if (bubbleAt != null && !(colNum == curCol && rowNum == curRow))
                    bubbles.Add(bubbleAt);
            }
        }
        return bubbles;
    }

    public void GetGroup(Bubble bubble, Dictionary<int, Dictionary<int, Bubble>> found, ref List<Bubble> list, bool differentColor)
    {
        int curRow = bubble.GetRow();
        if (!found.ContainsKey(curRow))
            found[curRow] = new Dictionary<int, Bubble>();

        if (found[curRow].ContainsKey(bubble.GetCol()))
            return;

        found[curRow][bubble.GetCol()] = bubble;
        list.Add(bubble);

        int curCol = bubble.GetCol();
        var surrounding = GetBubblesAround(curRow, curCol);
        foreach (var bubbleAt in surrounding)
        {
            if (bubbleAt != null && (bubbleAt.ColorType == bubble.ColorType || differentColor))
            {
                GetGroup(bubbleAt, found, ref list, differentColor); // Recursive call with ref list
            }
        }

    }

    public void PopBubbleAt(int row, int col)
    {
        if (row >= 0 && row < rows.Length && col >= 0 && col < rows[row].Length)
            rows[row][col] = null;
    }

    public List<Bubble> FindOrphans()
    {
        var connected = new bool[NUM_ROWS][];
        for (int i = 0; i < NUM_ROWS; i++)
            connected[i] = new bool[NUM_COLS];

        for (int i = 0; i < rows[0].Length; i++)
        {
            var bubble = GetBubbleAt(0, i);
            if (bubble != null && !connected[0][i])
            {
                var foundBubbles = new Dictionary<int, Dictionary<int, Bubble>>();
                var groupList = new List<Bubble>();
                connected[b.GetRow()][b.GetCol()] = true;
            }
        }

        var orphaned = new List<Bubble>();
        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j < rows[i].Length; j++)
            {
                var bubble = GetBubbleAt(i, j);
                if (bubble != null && !connected[i][j])
                    orphaned.Add(bubble);
            }
        }
        return orphaned;
    }

    public List<Bubble> GetBubbles()
    {
        var bubbles = new List<Bubble>();
        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j < rows[i].Length; j++)
            {
                var bubble = rows[i][j];
                if (bubble != null)
                    bubbles.Add(bubble);
            }
        }
        return bubbles;
    }

    public bool IsEmpty() => GetBubbles().Count == 0;
}
