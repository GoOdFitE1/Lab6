namespace Lab6;

public class Bubble
{
    public enum BubbleState { Current, OnBoard, Firing, Popping, Falling, Popped, Fired, Fallen }

    private int row, col, colorIndex; // Renamed from type
    private BubbleState state;
    private DateTime stateStart;
    private float x, y; // Для анімації
    private float rotation;

    // New public properties for movement direction
    public double DirectionX { get; set; }
    public double DirectionY { get; set; }

    // Додаємо параметр canvasWidth у конструктор
    public Bubble(int row, int col, int colorIndex, float canvasWidth)
    {
        this.row = row;
        this.col = col;
        this.colorIndex = colorIndex; // New assignment
        state = BubbleState.OnBoard;
        stateStart = DateTime.Now;
        rotation = (float)(new Random().NextDouble() * 360); // Можливо, обертання вже не потрібне, якщо не використовується
        UpdatePosition(canvasWidth); // Передаємо canvasWidth у метод
    }

    public int GetRow() => row;
    public void SetRow(int row) { this.row = row; UpdatePosition(); } // Потрібно передати canvasWidth, якщо викликається окремо
    public int GetCol() => col;
    public void SetCol(int col) { this.col = col; UpdatePosition(); } // Потрібно передати canvasWidth, якщо викликається окремо
    public int GetColorIndex() => colorIndex; // Renamed from GetType
    public int ColorIndex => colorIndex; // Renamed from ColorType
    public BubbleState GetState() => state;
    public void SetState(BubbleState state) { this.state = state; stateStart = DateTime.Now; }
    public TimeSpan GetTimeInState() => DateTime.Now - stateStart;
    public (float x, float y) GetPosition() => (x, y);
    public void SetPosition(float x, float y) { this.x = x; this.y = y; }
    public float GetRotation() => rotation;

    private void UpdatePosition(float canvasWidth = 480) // Додаємо параметр з типовим значенням
    {
        // Використовуємо публічні константи з класу Board
        float currentX = this.col * Board.VISUAL_BUBBLE_DIAMETER + Board.BUBBLE_RADIUS;
        if (this.row % 2 == 1) // Зміщення для непарних рядів ("odd-r" сітка)
        {
            currentX += Board.BUBBLE_RADIUS;
        }
        float currentY = this.row * Board.VISUAL_ROW_HEIGHT + Board.BUBBLE_RADIUS;

        // Центрування сітки: зміщуємо на половину ширини екрана мінус половину загальної ширини сітки
        float gridOffsetX = (canvasWidth - (Board.DEFAULT_NUM_COLS * Board.VISUAL_BUBBLE_DIAMETER)) / 2;
        currentX += gridOffsetX;

        this.x = currentX;
        this.y = currentY;
        // System.Diagnostics.Debug.WriteLine($"Bubble R:{this.row},C:{this.col} updated to X:{this.x}, Y:{this.y}");
    }

    public void AnimatePop()
    {
        SetState(BubbleState.Popping);
    }

    public void AnimateFall()
    {
        SetState(BubbleState.Falling);
    }
}
