using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace TheWell.MAUI.Controls;

public class DropletCanvasView : SKCanvasView
{
    public static readonly BindableProperty StreakCountProperty =
        BindableProperty.Create(nameof(StreakCount), typeof(int), typeof(DropletCanvasView), 0,
            propertyChanged: (b, _, _) => ((DropletCanvasView)b).InvalidateSurface());

    public int StreakCount
    {
        get => (int)GetValue(StreakCountProperty);
        set => SetValue(StreakCountProperty, value);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColors.Transparent);

        float w = info.Width;
        float h = info.Height;
        float cx = w / 2f;

        using var dropletPath = new SKPath();
        float tipY = 4;
        float bodyTop = h * 0.35f;
        float r = Math.Min(w / 2f - 4, h * 0.35f);
        float bodyCenter = h - r - 4;

        dropletPath.MoveTo(cx, tipY);
        dropletPath.CubicTo(cx + r * 0.8f, bodyTop, cx + r, bodyCenter, cx, bodyCenter + r);
        dropletPath.CubicTo(cx - r, bodyCenter, cx - r * 0.8f, bodyTop, cx, tipY);
        dropletPath.Close();

        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(cx, tipY),
                new SKPoint(cx, bodyCenter + r),
                [new SKColor(100, 200, 240), new SKColor(26, 107, 138)],
                SKShaderTileMode.Clamp)
        };
        canvas.DrawPath(dropletPath, fillPaint);

        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = r * 0.7f,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        };
        float textY = bodyCenter + r * 0.35f;
        canvas.DrawText(StreakCount.ToString(), cx, textY, textPaint);
    }
}
