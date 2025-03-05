namespace TripApp
{
    public class Cities
    {
        private string name;
        private double coordination_x;
        private double coordination_y;
        private Dictionary<Cities, int> goingTo;
        private bool is_start_city = false;
        private bool is_target_city = false;
        private double x;
        private double y;

        public Cities(string name, double coordination_x, double coordination_y, Dictionary<Cities, int> goingTo)
        {
            this.name = name;
            this.coordination_x = coordination_x;
            this.coordination_y = coordination_y;
            this.goingTo = goingTo;
        }

        public Cities(string name, double coordination_x, double coordination_y, Dictionary<Cities, int> goingTo, bool is_starting_city)
        {
            this.name = name;
            this.coordination_x = coordination_x;
            this.coordination_y = coordination_y;
            this.goingTo = goingTo;
            this.is_start_city = is_starting_city;
        }

        public Cities(string name, double x, double y)
        {
            this.name = name;
            this.x = x;
            this.y = y;
        }

        public bool IsStartingCity() => is_start_city;
        public bool IsTargetCity() => is_target_city;
        public void SetIsStarting(bool isStarting) => is_start_city = isStarting;
        public void SetIsTarget(bool isTarget) => is_target_city = isTarget;
        public string GetName() => name;
        public double GetX() => coordination_x;
        public double GetY() => coordination_y;
        public Dictionary<Cities, int> GetGoingTo() => goingTo;
    }

    public class CitiesDrawable : IDrawable
    {
        private Dictionary<Cities, int> cities;

        public CitiesDrawable(Dictionary<Cities, int> cities)
        {
            this.cities = cities;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (cities == null || cities.Count == 0) return;

            // Draw connections with costs
            foreach (var city in cities)
            {
                foreach (var connection in city.Key.GetGoingTo())
                {
                    Cities targetCity = connection.Key;
                    int cost = connection.Value;

                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 2;
                    float startX = (float)city.Key.GetX();
                    float startY = (float)city.Key.GetY();
                    float endX = (float)targetCity.GetX();
                    float endY = (float)targetCity.GetY();

                    DrawArrow(canvas, startX, startY, endX, endY);

                    // Draw the cost label
                    float midX = (startX + endX) / 2;
                    float midY = (startY + endY) / 2;

                    float dx = endX - startX;
                    float dy = endY - startY;
                    float length = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (length > 0)
                    {
                        float offsetX = -dy / length * 10;
                        float offsetY = dx / length * 10;

                        midX += offsetX;
                        midY += offsetY;

                        DrawCostLabel(canvas, midX, midY, cost.ToString());
                    }
                }
            }

            // Draw cities
            foreach (var city in cities)
            {
                float radius = 15;
                canvas.FillColor = city.Key.IsStartingCity() ? Colors.Green :
                                   city.Key.IsTargetCity() ? Colors.Red : Colors.DodgerBlue;

                float x = (float)city.Key.GetX();
                float y = (float)city.Key.GetY();
                canvas.FillCircle(x, y, radius);

                // Draw city name
                canvas.FontColor = Colors.Black;
                canvas.FontSize = 12;
                canvas.DrawString(city.Key.GetName(), x - radius, y + radius + 5, HorizontalAlignment.Left);
            }
        }

        private void DrawArrow(ICanvas canvas, float x1, float y1, float x2, float y2, float margin = 15)
        {
            float arrowSize = 10;
            float angle = (float)Math.Atan2(y2 - y1, x2 - x1);

            float adjustedX2 = x2 - (float)(Math.Cos(angle) * margin);
            float adjustedY2 = y2 - (float)(Math.Sin(angle) * margin);

            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 2;
            canvas.DrawLine(x1, y1, adjustedX2, adjustedY2);

            float arrow1X = adjustedX2 - arrowSize * (float)Math.Cos(angle - Math.PI / 6);
            float arrow1Y = adjustedY2 - arrowSize * (float)Math.Sin(angle - Math.PI / 6);
            float arrow2X = adjustedX2 - arrowSize * (float)Math.Cos(angle + Math.PI / 6);
            float arrow2Y = adjustedY2 - arrowSize * (float)Math.Sin(angle + Math.PI / 6);

            canvas.DrawLine(adjustedX2, adjustedY2, arrow1X, arrow1Y);
            canvas.DrawLine(adjustedX2, adjustedY2, arrow2X, arrow2Y);
        }

        private void DrawCostLabel(ICanvas canvas, float x, float y, string text)
        {
            canvas.FillColor = Colors.White;
            canvas.FillRoundedRectangle(x - 15, y - 15, 30, 30, 5);

            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(x - 15, y - 15, 30, 30, 5);

            canvas.FontColor = Colors.Black;
            canvas.FontSize = 16;
            canvas.DrawString(text, x, y, HorizontalAlignment.Center);
        }
    }
}
