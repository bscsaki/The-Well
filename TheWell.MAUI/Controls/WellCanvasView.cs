using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace TheWell.MAUI.Controls;

public class WellCanvasView : SKCanvasView
{
    public static readonly BindableProperty FillPercentProperty =
        BindableProperty.Create(nameof(FillPercent), typeof(double), typeof(WellCanvasView), 0.0,
            propertyChanged: (b, _, _) => ((WellCanvasView)b).InvalidateSurface());

    public double FillPercent
    {
        get => (double)GetValue(FillPercentProperty);
        set => SetValue(FillPercentProperty, Math.Clamp(value, 0.0, 1.0));
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColors.Transparent);

        float cx = info.Width / 2f;
        float cy = info.Height / 2f;
        float r = Math.Min(cx, cy) - 4;

        using var wellPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(26, 107, 138),
            StrokeWidth = 3,
            IsAntialias = true
        };
        canvas.DrawCircle(cx, cy, r, wellPaint);

        if (FillPercent > 0)
        {
            float fillHeight = (float)(2 * r * FillPercent);
            float top = cy + r - fillHeight;

            using var clipPath = new SKPath();
            clipPath.AddCircle(cx, cy, r - 2);
            canvas.Save();
            canvas.ClipPath(clipPath);

            using var fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(cx, cy + r),
                    new SKPoint(cx, cy - r),
                    [new SKColor(26, 107, 138, 100), new SKColor(26, 107, 138, 220)],
                    SKShaderTileMode.Clamp)
            };
            canvas.DrawRect(cx - r, top, 2 * r, fillHeight, fillPaint);
            canvas.Restore();
        }

        var pct = (int)(FillPercent * 100);
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = r * 0.45f,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        };
        canvas.DrawText($"{pct}%", cx, cy + textPaint.TextSize / 3, textPaint);
    }
}
