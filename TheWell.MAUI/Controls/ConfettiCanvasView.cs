using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace TheWell.MAUI.Controls;

public class ConfettiCanvasView : SKCanvasView
{
    private readonly List<Particle> _particles = [];
    private IDispatcherTimer? _timer;
    private static readonly SKColor[] Colors =
    [
        new SKColor(255, 99, 99), new SKColor(99, 200, 99),
        new SKColor(99, 99, 255), new SKColor(255, 215, 0),
        new SKColor(255, 150, 50), new SKColor(180, 99, 255)
    ];

    public void TriggerConfetti()
    {
        _particles.Clear();
        var rng = new Random();
        for (int i = 0; i < 80; i++)
        {
            _particles.Add(new Particle
            {
                X = (float)(rng.NextDouble() * Width),
                Y = (float)(-rng.NextDouble() * 60),
                VX = (float)((rng.NextDouble() - 0.5) * 4),
                VY = (float)(rng.NextDouble() * 4 + 2),
                Rotation = (float)(rng.NextDouble() * 360),
                RotationSpeed = (float)((rng.NextDouble() - 0.5) * 8),
                Size = (float)(rng.NextDouble() * 10 + 5),
                Color = Colors[rng.Next(Colors.Length)],
                Alpha = 255
            });
        }

        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.X += p.VX;
            p.Y += p.VY;
            p.VY += 0.15f;
            p.Rotation += p.RotationSpeed;
            p.Alpha = (byte)Math.Max(0, p.Alpha - 3);

            if (p.Y > Height + 20 || p.Alpha == 0)
                _particles.RemoveAt(i);
        }

        InvalidateSurface();

        if (_particles.Count == 0)
        {
            _timer?.Stop();
            IsVisible = false;
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        foreach (var p in _particles)
        {
            canvas.Save();
            canvas.Translate(p.X, p.Y);
            canvas.RotateDegrees(p.Rotation);

            using var paint = new SKPaint
            {
                Color = p.Color.WithAlpha(p.Alpha),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.DrawRect(-p.Size / 2, -p.Size / 4, p.Size, p.Size / 2, paint);
            canvas.Restore();
        }
    }

    private class Particle
    {
        public float X, Y, VX, VY, Rotation, RotationSpeed, Size;
        public SKColor Color;
        public byte Alpha;
    }
}
