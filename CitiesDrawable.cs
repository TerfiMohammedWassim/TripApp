public class CitiesDrawable : IDrawable
{
    private Dictionary<Cities, int> cities;
    private Dictionary<string, List<string>> bestPaths;

    public CitiesDrawable(Dictionary<Cities, int> cities, Dictionary<string, List<string>> bestPaths)
    {
        this.cities = cities;
        this.bestPaths = bestPaths;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (cities == null || cities.Count == 0)
            return;

        foreach (var city in cities.Keys)
        {
            foreach (var connection in city.GetGoingTo())
            {
                Cities targetCity = connection.Key;
                int cost = connection.Value;

                DrawArrow(canvas, city.GetX(), city.GetY(),
                    targetCity.GetX(), targetCity.GetY(),
                    cost.ToString(), Colors.Gray);
            }
        }

        if (bestPaths != null && bestPaths.Count > 0)
        {
            foreach (var path in bestPaths.Values)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    string fromCityName = path[i];
                    string toCityName = path[i + 1];

                    Cities fromCity = FindCityByName(fromCityName);
                    Cities toCity = FindCityByName(toCityName);

                    if (fromCity != null && toCity != null)
                    {
                        int cost = 0;
                        if (fromCity.GetGoingTo().TryGetValue(toCity, out int pathCost))
                        {
                            cost = pathCost;
                        }

                        DrawArrow(canvas, fromCity.GetX(), fromCity.GetY(),
                            toCity.GetX(), toCity.GetY(),
                            cost.ToString(), Colors.Green, 3);
                    }
                }
            }
        }

        foreach (var city in cities.Keys)
        {
            Color color = Colors.Blue;
            if (city.IsStarting())
            {
                color = Colors.Green;
            }
            else if (city.IsTarget())
            {
                color = Colors.Red;
            }

            canvas.FillColor = color;
            canvas.FillCircle((float)city.GetX(), (float)city.GetY(), 10);

            canvas.FontColor = Colors.Black;
            canvas.FontSize = 12;
            canvas.DrawString(city.GetName(), (float)city.GetX() - 20, (float)city.GetY() - 20,
                HorizontalAlignment.Left);
        }
    }

    private Cities FindCityByName(string name)
    {
        foreach (var city in cities.Keys)
        {
            if (city.GetName() == name)
            {
                return city;
            }
        }
        return null;
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

        double arrowSize = 8;
        double angle = Math.Atan2(dy, dx);
        double arrowAngle1 = angle - Math.PI / 6;
        double arrowAngle2 = angle + Math.PI / 6;

        double arrowX1 = endX - arrowSize * Math.Cos(arrowAngle1);
        double arrowY1 = endY - arrowSize * Math.Sin(arrowAngle1);
        double arrowX2 = endX - arrowSize * Math.Cos(arrowAngle2);
        double arrowY2 = endY - arrowSize * Math.Sin(arrowAngle2);

        canvas.DrawLine((float)endX, (float)endY, (float)arrowX1, (float)arrowY1);
        canvas.DrawLine((float)endX, (float)endY, (float)arrowX2, (float)arrowY2);

        double midX = (startX + endX) / 2;
        double midY = (startY + endY) / 2;

        canvas.FontColor = color;
        canvas.FontSize = 10;
        canvas.DrawString(label, (float)midX, (float)midY, HorizontalAlignment.Center);
    }
}