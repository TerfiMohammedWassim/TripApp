using Microsoft.Maui.Graphics;

public class CitiesDrawable : IDrawable
{
    private List<Cities> cities;
    private List<Cities> bestPaths;

    public CitiesDrawable(List<Cities> cities, List<Cities> bestPaths)
    {
        this.cities = cities;
        this.bestPaths = bestPaths;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (cities == null || cities.Count == 0)
        {
            return;
        }

        foreach (Cities city in cities)
        {
            foreach (var connection in city.GetLinkedTo())
            {
                DrawArrow(canvas, city.GetX(), city.GetY(), connection.GetX(), connection.GetY(), "", Colors.Grey);
            }
        }

        if (bestPaths != null && bestPaths.Count > 0)
        {
            for (int i = 0; i < bestPaths.Count - 1; i++)
            {
                DrawArrow(canvas, bestPaths[i].GetX(), bestPaths[i].GetY(),
                         bestPaths[i + 1].GetX(), bestPaths[i + 1].GetY(), "", Colors.Red, 2);
            }
        }

        
        foreach (Cities city in cities)
        {
            canvas.FillColor = Colors.Blue;
            canvas.FillCircle(float.Parse(city.GetX().ToString()), float.Parse(city.GetY().ToString()), 10);
        }
    }

    private void DrawArrow(ICanvas canvas, double x1, double y1, double x2, double y2,
        string label, Color color, float lineThickness = 1)
    {
        canvas.StrokeColor = color;
        canvas.StrokeSize = lineThickness;
        double dx = x2 - x1;
        double dy = y2 - y1;
        double length = Math.Sqrt(dx * dx + dy * dy);
        dx /= length;
        dy /= length;
        double startX = x1 + dx * 10;
        double startY = y1 + dy * 10;
        double endX = x2 - dx * 10;
        double endY = y2 - dy * 10;
        canvas.DrawLine((float)startX, (float)startY, (float)endX, (float)endY);
    }
}