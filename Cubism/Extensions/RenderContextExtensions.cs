using Cubism.Abstractions;
using Cubism.Models;

namespace Cubism.Extensions;

public static class RenderContextExtensions
{
    public static void DrawRectangle(this IRenderContext2D ctx, System.Drawing.Color color, Point2 topLeft, double width, double height)
    {
        ctx.DrawPolygon(color, [
            topLeft,
            new Point2(topLeft.X + width, topLeft.Y),
            new Point2(topLeft.X + width, topLeft.Y + height),
            new Point2(topLeft.X, topLeft.Y + height),
        ]);
    }

    public static void FillRectangle(this IRenderContext2D ctx, System.Drawing.Color color, Point2 topLeft, double width, double height)
    {
        ctx.FillPolygon(color, [
            topLeft,
            new Point2(topLeft.X + width, topLeft.Y),
            new Point2(topLeft.X + width, topLeft.Y + height),
            new Point2(topLeft.X, topLeft.Y + height),
        ]);
    }
}