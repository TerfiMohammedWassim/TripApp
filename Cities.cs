using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;

namespace TripApp
{
    public class Cities
    {
        private String name;
        private double coordination_x;
        private double coordination_y;
        private List<Cities> goingTo;
        private bool is_start_city = false;

        public Cities(string name, double coordination_x, double coordination_y, List<Cities> goingTo)
        {
            this.name = name;
            this.coordination_x = coordination_x;
            this.coordination_y = coordination_y;
            this.goingTo = goingTo;
        }

        public Cities(String name, double coordination_x, double coordination_y, List<Cities> goingTo, bool is_starting_city)
        {
            this.name = name;
            this.coordination_y = coordination_y;
            this.coordination_x = coordination_x;
            this.goingTo = goingTo;
            this.is_start_city = is_starting_city;
        }

        public bool is_Starting_City()
        {
            return this.is_start_city;
        }

        public String getName()
        {
            return this.name;
        }

        public double getCoordinationX()
        {
            return this.coordination_x;
        }

        public double getCoordinationY()
        {
            return this.coordination_y;
        }

        public List<Cities> getGoingTo()
        {
            return this.goingTo;
        }
    }

    // Using primary constructor for CitiesDrawable class
    public class CitiesDrawable(List<Cities> cities) : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (cities == null || cities.Count == 0)
                return;

            // Draw connections first (so they appear behind the cities)
            foreach (var city in cities)
            {
                foreach (var targetCity in city.getGoingTo())
                {
                    // Draw a line from this city to the target city
                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 2;
                    canvas.DrawLine(
                        (float)city.getCoordinationX(),
                        (float)city.getCoordinationY(),
                        (float)targetCity.getCoordinationX(),
                        (float)targetCity.getCoordinationY()
                    );

                    // Draw an arrow at the end of the line
                    DrawArrow(canvas,
                        (float)city.getCoordinationX(),
                        (float)city.getCoordinationY(),
                        (float)targetCity.getCoordinationX(),
                        (float)targetCity.getCoordinationY()
                    );
                }
            }

            foreach (var city in cities)
            {
                float radius = 15;

                canvas.FillColor = city.is_Starting_City() ? Colors.Green : Colors.DodgerBlue;
                canvas.FillCircle(
                    (float)city.getCoordinationX(),
                    (float)city.getCoordinationY(),
                    radius
                );

                canvas.FontColor = Colors.Black;
                canvas.FontSize = 12;
                canvas.DrawString(
                    city.getName(),
                    (float)city.getCoordinationX() - radius,
                    (float)city.getCoordinationY() + radius + 5,
                    HorizontalAlignment.Left
                );
            }
        }

        private void DrawArrow(ICanvas canvas, float x1, float y1, float x2, float y2)
        {
            float arrowSize = 10;
            float angle = (float)Math.Atan2(y2 - y1, x2 - x1);

            float endX = x2 - (float)(Math.Cos(angle) * arrowSize);
            float endY = y2 - (float)(Math.Sin(angle) * arrowSize);

            float arrow1X = endX - arrowSize * (float)Math.Cos(angle - Math.PI / 6);
            float arrow1Y = endY - arrowSize * (float)Math.Sin(angle - Math.PI / 6);
            float arrow2X = endX - arrowSize * (float)Math.Cos(angle + Math.PI / 6);
            float arrow2Y = endY - arrowSize * (float)Math.Sin(angle + Math.PI / 6);

            canvas.StrokeColor = Colors.Gray;
            canvas.DrawLine(endX, endY, arrow1X, arrow1Y);
            canvas.DrawLine(endX, endY, arrow2X, arrow2Y);
        }
    }
}