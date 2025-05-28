namespace Lab6;

public class Bubble
{
    public enum BubbleState { Current, OnBoard, Firing, Popping, Falling, Popped, Fired, Fallen }

    private int row, col, type;
    private BubbleState state;
    private DateTime stateStart;
    private float x, y; // Для анімації
    private float rotation;

    public Bubble(int row, int col, int type)
    {
        this.row = row;
        this.col = col;
        this.type = type;
        state = BubbleState.OnBoard;
        stateStart = DateTime.Now;
        rotation = (float)(new Random().NextDouble() * 360);
        UpdatePosition();
    }

    public int GetRow() => row;
    public void SetRow(int row) { this.row = row; UpdatePosition(); }
    public int GetCol() => col;
    public void SetCol(int col) { this.col = col; UpdatePosition(); }
    public int GetType() => type;
    public BubbleState GetState() => state;
    public void SetState(BubbleState state) { this.state = state; stateStart = DateTime.Now; }
    public TimeSpan GetTimeInState() => DateTime.Now - stateStart;
    public (float x, float y) GetPosition() => (x, y);
    public void SetPosition(float x, float y) { this.x = x; this.y = y; }
    public float GetRotation() => rotation;

    private void UpdatePosition()
    {
        x = col * 50f / 2 + 25f; // BUBBLE_DIMS = 50
        y = row * 43.3f + 25f;   // ROW_HEIGHT = 43.3 (приблизно 50 * sqrt(3)/2)
        if (row % 2 == 1) x += 25f;
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