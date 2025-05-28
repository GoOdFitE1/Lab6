using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Timers;

namespace Lab6;

public partial class MainPage : ContentPage
{
    private Game game;
    private Timer gameTimer;

    public MainPage()
    {
        InitializeComponent();
        game = new Game(this);
        SetupTimer();
    }

    private void SetupTimer()
    {
        gameTimer = new Timer(1000 / 60); // 60 FPS
        gameTimer.Elapsed += (s, e) => Dispatcher.Dispatch(() =>
        {
            game.Update();
            GameCanvas.InvalidateSurface();
        });
        gameTimer.AutoReset = true;
    }

    private void OnNewGameClicked(object sender, EventArgs e)
    {
        game.StartGame();
        NewGameButton.IsVisible = false;
        GameOverLabel.IsVisible = false;
        gameTimer.Start();
        UpdateUI();
    }

    private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        game.Render(e.Surface.Canvas, e.Info.Width, e.Info.Height);
    }

    private void OnCanvasTouch(object sender, SKTouchEventArgs e)
    {
        if (e.ActionType == SKTouchAction.Pressed)
        {
            game.HandleTouch(e.Location.X, e.Location.Y);
            e.Handled = true;
        }
    }

    public void UpdateUI()
    {
        ScoreLabel.Text = $"Бали: {game.Score}";
        LevelLabel.Text = $"Гра: {game.Level}";
        BubblesRemainingLabel.Text = $"Ударів: {game.BubblesRemaining}";
        HighScoreLabel.Text = $"Макс: {game.HighScore}";
        GameCanvas.InvalidateSurface();
    }

    public void ShowGameOver(bool hasWon)
    {
        gameTimer.Stop();
        GameOverLabel.Text = hasWon ? "Гра виграна!" : "Гра програна!";
        GameOverLabel.IsVisible = true;
        NewGameButton.Text = hasWon ? "Наступна гра" : "Нова гра";
        NewGameButton.IsVisible = true;
    }
}