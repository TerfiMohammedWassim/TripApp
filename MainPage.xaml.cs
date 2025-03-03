using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TripApp
{
    public partial class MainPage : ContentPage
    {
        private List<Cities> allCities = new List<Cities>();
        private Random random = new Random();
        private Cities starting_city;
        private Cities target_city;
        private bool is_starting_first = false;

        private readonly string[] cityNames = new string[]
        {
            "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia",
            "San Antonio", "San Diego", "Dallas", "San Jose", "Austin", "Jacksonville",
            "Fort Worth", "Columbus", "Charlotte", "San Francisco", "Indianapolis",
            "Seattle", "Denver", "Boston", "Portland", "Las Vegas", "Nashville",
            "Oklahoma City", "Detroit", "Memphis", "Louisville", "Baltimore", "Milwaukee",
            "Albuquerque", "Tucson", "Sacramento", "Kansas City", "Atlanta", "Miami"
        };

        public MainPage()
        {
            InitializeComponent();
            TspCanvas.Drawable = new CitiesDrawable(allCities);
        }

        private void OnGenerateCitiesClicked(object sender, EventArgs e)
        {
            allCities.Clear();

            if (int.Parse(CitiesCountEntry.Text) < 3)
            {
                DisplayAlert("Error occured","you need to heve at least 4 cities","cancel");
            }
            int numCities = int.Parse(CitiesCountEntry.Text);
            for (int i = 0; i < numCities; i++)
            {
                string name = cityNames[random.Next(cityNames.Length)];

                double x = random.NextDouble() * (TspCanvas.Width * 0.8) + (TspCanvas.Width * 0.1);
                double y = random.NextDouble() * (TspCanvas.Height * 0.8) + (TspCanvas.Height * 0.1);

                Cities city = new Cities(name, x, y, new List<Cities>(), i == 0);

                allCities.Add(city);
            }

            foreach (var city in allCities)
            {
                int numConnections = random.Next(1, 4);

                for (int i = 0; i < numConnections; i++)
                {
                    var potentialCities = allCities.Where(c => c != city && !city.getGoingTo().Contains(c)).ToList();

                    if (potentialCities.Count > 0)
                    {
                        Cities targetCity = potentialCities[random.Next(potentialCities.Count)];
                        city.getGoingTo().Add(targetCity);
                    }
                }
            }

            TspCanvas.Invalidate();
        }
        private void OnSolveClicked(object sender, EventArgs e)
        {

            if (starting_city == null || target_city == null) 
            {
                DisplayAlert("No Starting and Finishing", "you need to select a starting and finishing cities","ok");
            }

            if (allCities == null || allCities.Count == 0)
            {
                DisplayAlert("No Cities", "Please generate cities first", "OK");
                return;
            }


            TspCanvas.Invalidate();

            DisplayAlert("Solution", "Path calculated successfully!", "OK");
        }
    }
}