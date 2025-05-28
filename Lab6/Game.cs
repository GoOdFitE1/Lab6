using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Lab6;

public class Game
{
    private const int MAX_BUBBLES = 70;
    private const int POINTS_PER_BUBBLE = 10;
    private const float BUBBLE_DIMS = 50f;
    private const float ROW_HEIGHT = 43.3f;

    private MainPage page;
    private Bubble curBubble;
    private Board board;
    private List<Bubble> bubbles;
    private int bubblesRemaining;
    private SKBitmap bubbleSpriteSheet;
    private float canvasWidth, canvasHeight;

    public int Score { get; private set; }
    public int Level { get; private set; }
    public int HighScore { get; private set; }
    public int BubblesRemaining => bubblesRemaining;

    public Game(MainPage page)
    {
        this.page = page;
        bubbles = new List<Bubble>();
        board = new Board();
        Level = 1;
        Score = 0;
        HighScore = Preferences.Get("HighScore", 0);
        bubbleSpriteSheet = SKBitmap.Decode(FileSystem.OpenAppPackageFileAsync("bubble_sprite_sheet.png").Result);
    }

    public void StartGame()
    {
        bubbles.Clear();
        board = new Board();
        bubbles = board.GetBubbles();
        bubblesRemaining = MAX_BUBBLES - (Level - 1) * 5;
        Score = 0;
        curBubble = GetNextBubble();
        page.UpdateUI();
    }

    private Bubble GetNextBubble()
    {
        var bubble = new Bubble(12, 3, new Random().Next(4)); // Розташування внизу центру
        bubble.SetState(Bubble.BubbleState.Current);
        bubbles.Add(bubble);
        bubblesRemaining--;
        return bubble;
    }

    public void HandleTouch(float touchX, float touchY)
    {
        if (curBubble?.GetState() != Bubble.BubbleState.Current) return;

        var bubblePos = curBubble.GetPosition();
        var angle = Math.Atan2(touchX - bubblePos.x, bubblePos.y - touchY);
        var collision = CollisionDetector.FindIntersection(curBubble, board, angle, bubblePos.x, bubblePos.y);

        if (collision.HasValue)
        {
            var duration = 750 * collision.Value.dist / 1000;
            board.AddBubble(curBubble, collision.Value.coords);
            var groupResult = board.GetGroup(curBubble, new Dictionary<int, Dictionary<int, Bubble>>(), false);
            if (groupResult.list.Count >= 3)
            {
                PopBubbles(groupResult.list, duration);
                var topRowBubbles = board.GetRows()[0].Where(b => b != null).ToList();
                if (topRowBubbles.Count <= 5)
                {
                    PopBubbles(topRowBubbles, duration);
                    groupResult.list.AddRange(topRowBubbles);
                }
                var orphans = board.FindOrphans();
                var delay = duration + 200 + 30 * groupResult.list.Count;
                DropBubbles(orphans, delay);
                var popped = groupResult.list.Concat(orphans).ToList();
                Score += popped.Count * POINTS_PER_BUBBLE;
            }
        }
        else
        {
            var distX = (float)(Math.Sin(angle) * 1000);
            var distY = (float)(-Math.Cos(angle) * 1000);
            var coords = (x: bubblePos.x + distX, y: bubblePos.y + distY);
            curBubble.SetPosition(coords.x, coords.y);
        }

        CheckGameEnd();
        curBubble = bubblesRemaining > 0 ? GetNextBubble() : null;
        page.UpdateUI();
    }

    private void PopBubbles(List<Bubble> bubblesToPop, double delay)
    {
        foreach (var bubble in bubblesToPop)
        {
            bubble.AnimatePop();
            board.PopBubbleAt(bubble.GetRow(), bubble.GetCol());
        }
    }

    private void DropBubbles(List<Bubble> bubblesToDrop, double delay)
    {
        foreach (var bubble in bubblesToDrop)
        {
            bubble.AnimateFall();
            board.PopBubbleAt(bubble.GetRow(), bubble.GetCol());
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
            if (bubble.GetState() == Bubble.BubbleState.Popping && bubble.GetTimeInState().TotalMilliseconds > 200)
                bubble.SetState(Bubble.BubbleState.Popped);
            if (bubble.GetState() == Bubble.BubbleState.Falling && bubble.GetTimeInState().TotalMilliseconds > 500)
                bubble.SetState(Bubble.BubbleState.Fallen);
            if (bubble.GetState() == Bubble.BubbleState.Popped || bubble.GetState() == Bubble.BubbleState.Fallen)
                bubbles.Remove(bubble);
        }
    }

    public void Render(SKCanvas canvas, int width, int height)
    {
        canvasWidth = width;
        canvasHeight = height;
        canvas.Clear(SKColors.Transparent);

        foreach (var bubble in bubbles)
        {
            var (x, y) = bubble.GetPosition();
            var type = bubble.GetType();
            var state = bubble.GetState();
            if (state == Bubble.BubbleState.Popped || state == Bubble.BubbleState.Fallen) continue;

            using var paint = new SKPaint();
            var sourceRect = new SKRect(type * BUBBLE_DIMS, 0, (type + 1) * BUBBLE_DIMS, BUBBLE_DIMS);
            var destRect = new SKRect(x - BUBBLE_DIMS / 2, y - BUBBLE_DIMS / 2, x + BUBBLE_DIMS / 2, y + BUBBLE_DIMS / 2);

            canvas.Save();
            canvas.RotateDegrees(bubble.GetRotation(), x, y);
            canvas.DrawBitmap(bubbleSpriteSheet, sourceRect, destRect, paint);
            canvas.Restore();
        }
    }
}