using System.Collections.Generic;

namespace Lab6;

public class Board
{
    // Public constants for bubble and grid dimensions
    public const float VISUAL_BUBBLE_DIAMETER = 60f; // Змінено з 50f на 60f
    public const float BUBBLE_RADIUS = VISUAL_BUBBLE_DIAMETER / 2f;
    public const float VISUAL_ROW_HEIGHT = (float)(VISUAL_BUBBLE_DIAMETER * 0.86602540378); // Оновлено автоматично


    // Private constants for fixed board size
    private const int DEFAULT_NUM_ROWS = 11;
    public const int DEFAULT_NUM_COLS = 8;

    // Instance fields for dimensions, initialized from constants
    private readonly int numRows;
    private readonly int numCols;
    private Bubble[][] rows;
    private Random random = new Random();

    public Board(float canvasWidth)
    {
        this.numRows = DEFAULT_NUM_ROWS;
        this.numCols = DEFAULT_NUM_COLS;
        rows = new Bubble[this.numRows][];
        for (int i = 0; i < this.numRows; i++)
        {
            rows[i] = new Bubble[this.numCols];
            for (int j = 0; j < this.numCols; j++)
            {
                if (i < 10) // Заповнюємо лише перші 10 рядків
                {
                    int colorIndex = random.Next(4);
                    rows[i][j] = new Bubble(i, j, colorIndex, canvasWidth); // Передаємо canvasWidth
                }
            }
        }
    }

    public Bubble[][] GetRows() => rows;

    public void AddBubble(Bubble bubbleToAdd, (float x, float y) collisionCoords)
    {
        const float BUBBLE_DIMS = VISUAL_BUBBLE_DIAMETER;
        const float BUBBLE_RADIUS = VISUAL_BUBBLE_DIAMETER / 2f;

        float canvasY = 70f;
        float adjustedY = collisionCoords.y - canvasY;

        System.Diagnostics.Debug.WriteLine($"collisionCoords: x={collisionCoords.x}, y={collisionCoords.y}, adjustedY={adjustedY}");

        // Обчислюємо зміщення для центрування сітки
        float gridOffsetX = (collisionCoords.x - (Board.DEFAULT_NUM_COLS * VISUAL_BUBBLE_DIAMETER) / 2) / BUBBLE_DIMS;
        float adjustedX = collisionCoords.x - (gridOffsetX * BUBBLE_DIMS); // Коректно враховуємо зміщення

        int calculatedRowNum = (int)(adjustedY / VISUAL_ROW_HEIGHT);
        float xOffset = (calculatedRowNum % 2 == 1) ? BUBBLE_RADIUS : 0;
        int calculatedColNum = (int)((adjustedX - xOffset) / BUBBLE_DIMS);

        calculatedRowNum = Math.Max(0, Math.Min(calculatedRowNum, numRows - 1));
        calculatedColNum = Math.Max(0, Math.Min(calculatedColNum, numCols - 1));

        int finalRowNum = calculatedRowNum;
        int finalColNum = calculatedColNum;

        // Перевіряємо, чи є місце для прилипання
        bool hasNeighbor = false;
        foreach (var neighbor in GetBubblesAround(finalRowNum, finalColNum))
        {
            if (neighbor != null)
            {
                hasNeighbor = true;
                break;
            }
        }

        if (!hasNeighbor && finalRowNum > 0)
        {
            // Якщо немає сусідів і це не перший рядок, кулька не прилипає
            bubbleToAdd.SetState(Bubble.BubbleState.Fallen);
            return;
        }

        if (GetBubbleAt(finalRowNum, finalColNum) != null)
        {
            (int dr, int dc)[] neighbors;
            if (finalRowNum % 2 == 1)
            {
                neighbors = new (int, int)[] { (0, -1), (0, 1), (-1, 0), (-1, 1), (1, 0), (1, 1) };
            }
            else
            {
                neighbors = new (int, int)[] { (0, -1), (0, 1), (-1, -1), (-1, 0), (1, -1), (1, 0) };
            }

            bool foundFreeSpot = false;
            float minDistanceSqToOriginalCoords = float.MaxValue;
            int bestR = -1, bestC = -1;
            foreach (var offsetPair in neighbors)
            {
                int nr = finalRowNum + offsetPair.dr;
                int nc = finalColNum + offsetPair.dc;

                if (nr >= 0 && nr < numRows && nc >= 0 && nc < numCols && GetBubbleAt(nr, nc) == null)
                {
                    float neighborScreenXOffset = (nr % 2 == 1) ? BUBBLE_RADIUS : 0;
                    float neighborCenterX = nc * BUBBLE_DIMS + neighborScreenXOffset + BUBBLE_RADIUS;
                    float neighborCenterY = nr * VISUAL_ROW_HEIGHT + BUBBLE_RADIUS;

                    float distSq = (adjustedX - neighborCenterX) * (adjustedX - neighborCenterX) +
                                   (adjustedY - neighborCenterY) * (adjustedY - neighborCenterY);
                    if (!foundFreeSpot || distSq < minDistanceSqToOriginalCoords)
                    {
                        minDistanceSqToOriginalCoords = distSq;
                        bestR = nr;
                        bestC = nc;
                        foundFreeSpot = true;
                    }
                }
            }

            if (foundFreeSpot)
            {
                System.Diagnostics.Debug.WriteLine($"Placing bubble at row={bestR}, col={bestC}, dist={minDistanceSqToOriginalCoords}");
                finalRowNum = bestR;
                finalColNum = bestC;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No free spot found near row={finalRowNum}, col={finalColNum}");
                bubbleToAdd.SetState(Bubble.BubbleState.Fallen);
                return;
            }
        }

        if (finalRowNum >= 0 && finalRowNum < numRows && finalColNum >= 0 && finalColNum < numCols)
        {
            if (rows[finalRowNum][finalColNum] == null)
            {
                rows[finalRowNum][finalColNum] = bubbleToAdd;
                bubbleToAdd.SetRow(finalRowNum);
                bubbleToAdd.SetCol(finalColNum);
                bubbleToAdd.SetState(Bubble.BubbleState.OnBoard); // Явно встановлюємо стан
                                                                  // Оновлюємо позицію кульки після прилипання
                float newX = finalColNum * BUBBLE_DIMS + BUBBLE_RADIUS;
                if (finalRowNum % 2 == 1) newX += BUBBLE_RADIUS;
                float newY = finalRowNum * VISUAL_ROW_HEIGHT + BUBBLE_RADIUS;
                float canvasWidth = 1042; // Значення з логів (canvasWidth=1042)
                float gridOffset = (canvasWidth - (DEFAULT_NUM_COLS * VISUAL_BUBBLE_DIAMETER)) / 2;
                newX += gridOffset;
                bubbleToAdd.SetPosition(newX, newY + canvasY);
                System.Diagnostics.Debug.WriteLine($"Bubble placed at x={newX}, y={newY + canvasY}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[Board.AddBubble] Failed to find a free spot for bubble at R:{finalRowNum}, C:{finalColNum}.");
                bubbleToAdd.SetState(Bubble.BubbleState.Fallen);
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[Board.AddBubble] Final row/col out of bounds: R:{finalRowNum}, C:{finalColNum}");
            bubbleToAdd.SetState(Bubble.BubbleState.Fallen);
        }
    }

    public Bubble GetBubbleAt(int row, int col) => row >= 0 && row < rows.Length && col >= 0 && col < rows[row].Length ? rows[row][col] : null;

    public List<Bubble> GetBubblesAround(int curRow, int curCol)
    {
        var bubbles = new List<Bubble>();
        (int dr, int dc)[] neighbors;
        if (curRow % 2 == 1) // Непарний рядок
        {
            neighbors = new (int, int)[] { (0, -1), (0, 1), (-1, 0), (-1, 1), (1, 0), (1, 1) };
        }
        else // Парний рядок
        {
            neighbors = new (int, int)[] { (0, -1), (0, 1), (-1, -1), (-1, 0), (1, -1), (1, 0) };
        }

        foreach (var (dr, dc) in neighbors)
        {
            int nr = curRow + dr;
            int nc = curCol + dc;
            var bubbleAt = GetBubbleAt(nr, nc);
            if (bubbleAt != null)
                bubbles.Add(bubbleAt);
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
            if (bubbleAt != null && (bubbleAt.ColorIndex == bubble.ColorIndex || differentColor))
            {
                int neighborRow = bubbleAt.GetRow();
                int neighborCol = bubbleAt.GetCol();
                if (!found.ContainsKey(neighborRow) || !found[neighborRow].ContainsKey(neighborCol))
                {
                    GetGroup(bubbleAt, found, ref list, differentColor);
                }
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
        var connected = new bool[numRows][];
        for (int i = 0; i < numRows; i++)
            connected[i] = new bool[numCols];

        for (int i = 0; i < numCols; i++)
        {
            var bubble = GetBubbleAt(0, i);
            if (bubble != null && !connected[0][i])
            {
                var foundBubbles = new Dictionary<int, Dictionary<int, Bubble>>();
                var groupList = new List<Bubble>();
                GetGroup(bubble, foundBubbles, ref groupList, true);
                foreach (var bInGroup in groupList)
                {
                    if (bInGroup.GetRow() < numRows && bInGroup.GetCol() < numCols)
                    {
                        connected[bInGroup.GetRow()][bInGroup.GetCol()] = true;
                    }
                }
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
