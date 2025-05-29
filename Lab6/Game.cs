using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Lab6;

public class Game
{
    private const int MAX_BUBBLES = 70;
    private const int POINTS_PER_BUBBLE = 10;
    private const float BUBBLE_DIMS = 60f; // Змінено з 50f на 60f
    private const float ROW_HEIGHT = (float)(BUBBLE_DIMS * 0.86602540378);
    private MainPage page;
    private Bubble curBubble;
    private Board board;
    private List<Bubble> bubbles;
    private int bubblesRemaining;
    private float canvasWidth, canvasHeight;
    private bool isGameOver = false;
    private bool canShoot = true;

    public int Score { get; private set; }
    public int Level { get; private set; }
    public int HighScore { get; private set; }
    public int BubblesRemaining => bubblesRemaining;

    public Game(MainPage page)
    {
        this.page = page;
        this.canvasWidth = 480; // Типове значення
        this.canvasHeight = 800; // Типове значення
        bubbles = new List<Bubble>();
        board = new Board(canvasWidth);
        Level = 1;
        Score = 0;
        HighScore = Preferences.Get("HighScore", 0);
    }

    public void StartGame()
    {
        bubbles.Clear();
        board = new Board(canvasWidth);
        bubbles = board.GetBubbles();
        bubblesRemaining = MAX_BUBBLES - (Level - 1) * 5;
        Score = 0;
        curBubble = GetNextBubble();
        page.UpdateUI();
    }

    private Bubble GetNextBubble()
    {
        if (canvasWidth == 0 || canvasHeight == 0)
        {
            canvasWidth = 480;
            canvasHeight = 800;
        }

        Random rand = new Random();
        int colorIndex = rand.Next(4);
        var bubble = new Bubble(12, 3, colorIndex, canvasWidth);
        bubble.SetState(Bubble.BubbleState.Current);
        bubble.SetPosition(canvasWidth / 2, canvasHeight - BUBBLE_DIMS / 2 - 50);
        bubbles.Add(bubble);
        bubblesRemaining--;
        System.Diagnostics.Debug.WriteLine($"New bubble created at x={bubble.GetPosition().x}, y={bubble.GetPosition().y}");
        return bubble;
    }

    public void HandleTouch(float localTouchX, float localTouchY)
    {
        if (isGameOver || !canShoot || curBubble == null || curBubble.GetState() != Bubble.BubbleState.Current) return;

        var bubblePos = curBubble.GetPosition();

        float targetX = Math.Max(0, Math.Min(localTouchX, canvasWidth));
        float targetY = Math.Max(0, Math.Min(localTouchY, canvasHeight));

        if (Math.Sqrt(Math.Pow(targetX - bubblePos.x, 2) + Math.Pow(targetY - bubblePos.y, 2)) < Board.BUBBLE_RADIUS)
        {
            return;
        }

        var angle = Math.Atan2(targetX - bubblePos.x, bubblePos.y - targetY);
        curBubble.DirectionX = Math.Sin(angle);
        curBubble.DirectionY = -Math.Cos(angle);

        System.Diagnostics.Debug.WriteLine($"Shooting at angle={angle}, DirectionX={curBubble.DirectionX}, DirectionY={curBubble.DirectionY}");
        curBubble.SetState(Bubble.BubbleState.Firing);
        canShoot = false;
    }

    private void PopBubbles(List<Bubble> bubblesToPop, double delay)
    {
        foreach (var bubble in bubblesToPop.Distinct())
        {
            if (bubble != null && bubble.GetState() != Bubble.BubbleState.Popping && bubble.GetState() != Bubble.BubbleState.Popped)
            {
                bubble.AnimatePop();
                board.PopBubbleAt(bubble.GetRow(), bubble.GetCol());
            }
        }
    }

    private void DropBubbles(List<Bubble> bubblesToDrop, double delay)
    {
        foreach (var bubble in bubblesToDrop.Distinct())
        {
            if (bubble != null && bubble.GetState() != Bubble.BubbleState.Falling && bubble.GetState() != Bubble.BubbleState.Fallen)
            {
                bubble.AnimateFall();
                board.PopBubbleAt(bubble.GetRow(), bubble.GetCol());
            }
        }
    }

    private void CheckGameEnd()
    {
        if (board.GetRows().Length > 11)
        {
            EndGame(false);
        }
        else if (bubblesRemaining == 0)
        {
            EndGame(false);
        }
        else if (board.IsEmpty())
        {
            EndGame(true);
        }
    }

    private void EndGame(bool hasWon)
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            Preferences.Set("HighScore", HighScore);
        }
        if (hasWon)
            Level++;
        else
        {
            Score = 0;
            Level = 1;
        }
        page.ShowGameOver(hasWon);
    }

    public void Update()
    {
        foreach (var bubble in bubbles.ToList())
        {
            if (bubble == null) continue;

            if (bubble.GetState() == Bubble.BubbleState.Popping && bubble.GetTimeInState().TotalMilliseconds > 200)
                bubble.SetState(Bubble.BubbleState.Popped);
            if (bubble.GetState() == Bubble.BubbleState.Falling && bubble.GetTimeInState().TotalMilliseconds > 500)
                bubble.SetState(Bubble.BubbleState.Fallen);

            if (bubble.GetState() == Bubble.BubbleState.Firing)
            {
                var (bx, by) = bubble.GetPosition();
                bx += (float)(bubble.DirectionX * 10);
                by += (float)(bubble.DirectionY * 10);
                bubble.SetPosition(bx, by);

                bool outOfBounds = false;
                if (bx < -BUBBLE_DIMS / 2 || bx > canvasWidth + BUBBLE_DIMS / 2)
                {
                    outOfBounds = true;
                }
                else if (by > canvasHeight + BUBBLE_DIMS / 2)
                {
                    outOfBounds = true;
                }
                else if (by < -BUBBLE_DIMS / 2)
                {
                    outOfBounds = true;
                }

                if (outOfBounds)
                {
                    bubble.SetState(Bubble.BubbleState.Fallen);
                    canShoot = true;
                    curBubble = GetNextBubble();
                    continue;
                }

                // Перевіряємо зіткнення
                var collision = CollisionDetector.FindIntersection(bubble, board, Math.Atan2(bubble.DirectionX, bubble.DirectionY), bx, by);
                if (collision.HasValue && collision.Value.bubble != null) // Прилипаємо лише при зіткненні з іншою кулькою
                {
                    bubble.SetPosition(collision.Value.coords.x, collision.Value.coords.y);
                    board.AddBubble(bubble, collision.Value.coords);
                    if (bubble.GetState() == Bubble.BubbleState.OnBoard)
                    {
                        canShoot = true;
                        curBubble = GetNextBubble();

                        var group = new List<Bubble>();
                        var found = new Dictionary<int, Dictionary<int, Bubble>>();
                        board.GetGroup(bubble, found, ref group, false);
                        if (group.Count >= 3)
                        {
                            PopBubbles(group, 0);
                            Score += group.Count * POINTS_PER_BUBBLE;
                        }

                        var orphans = board.FindOrphans();
                        if (orphans.Count > 0)
                        {
                            DropBubbles(orphans, 0);
                            Score += orphans.Count * POINTS_PER_BUBBLE;
                        }

                        CheckGameEnd();
                        page.UpdateUI();
                        continue;
                    }
                    else if (bubble.GetState() == Bubble.BubbleState.Fallen)
                    {
                        canShoot = true;
                        curBubble = GetNextBubble();
                        continue;
                    }
                }
                else if (by <= 70f) // Якщо кулька досягла верхньої межі (canvasY = 70f) і немає зіткнення
                {
                    bubble.SetState(Bubble.BubbleState.Fallen);
                    canShoot = true;
                    curBubble = GetNextBubble();
                    continue;
                }
            }

            if (bubble.GetState() == Bubble.BubbleState.Popped || bubble.GetState() == Bubble.BubbleState.Fallen)
            {
                bubbles.Remove(bubble);
            }
        }
    }

    public void Render(SKCanvas canvas, int width, int height)
    {
        canvasWidth = width;
        canvasHeight = height;
        canvas.Clear(SKColors.LightSkyBlue);

        float canvasY = 70f;
        canvas.Translate(0, canvasY);

        SKColor[] baseColors = new SKColor[] {
            new SKColor(0, 255, 0),
            new SKColor(30, 144, 255),
            new SKColor(255, 215, 0),
            new SKColor(255, 99, 71)
        };

        float mainBubbleRadius = BUBBLE_DIMS / 2f;
        byte fillAlpha = 230;

        foreach (var bubble in bubbles)
        {
            if (bubble == null) continue;

            var (cx, cy) = bubble.GetPosition();
            var state = bubble.GetState();

            if (state == Bubble.BubbleState.Popped || state == Bubble.BubbleState.Fallen)
                continue;

            float adjustedY = (state == Bubble.BubbleState.Current || state == Bubble.BubbleState.Firing) ? cy : cy + canvasY;

            int colorIdx = bubble.GetColorIndex();
            if (colorIdx < 0 || colorIdx >= baseColors.Length)
                colorIdx = 0;

            SKColor bubbleBaseColor = baseColors[colorIdx];

            using (SKPaint fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = bubbleBaseColor.WithAlpha(fillAlpha),
                IsAntialias = true
            })
            using (SKPaint strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 2f,
                IsAntialias = true
            })
            {
                canvas.Save();
                canvas.DrawCircle(cx, adjustedY, mainBubbleRadius, fillPaint);
                canvas.DrawCircle(cx, adjustedY, mainBubbleRadius, strokePaint);
                canvas.Restore();
            }
        }

        if (curBubble != null && (curBubble.GetState() == Bubble.BubbleState.Current || curBubble.GetState() == Bubble.BubbleState.Firing))
        {
            var (cx, cy) = curBubble.GetPosition();
            int colorIdx = curBubble.GetColorIndex();
            if (colorIdx < 0 || colorIdx >= baseColors.Length)
                colorIdx = 0;
            SKColor bubbleBaseColor = baseColors[colorIdx];

            using (SKPaint fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = bubbleBaseColor.WithAlpha(fillAlpha),
                IsAntialias = true
            })
            using (SKPaint strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 2f,
                IsAntialias = true
            })
            {
                canvas.Save();
                canvas.DrawCircle(cx, cy, mainBubbleRadius, fillPaint);
                canvas.DrawCircle(cx, cy, mainBubbleRadius, strokePaint);
                canvas.Restore();
            }
            System.Diagnostics.Debug.WriteLine($"Rendering current bubble at x={cx}, y={cy}, state={curBubble.GetState()}");
        }
    }
}
