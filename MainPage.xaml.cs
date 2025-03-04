using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TripApp
{
    public partial class MainPage : ContentPage
    {
        private List<Cities> allCities = new List<Cities>();
        private Random random = new Random();
        private Cities starting_city;
        private Cities target_city;
        private static HttpClient client = new HttpClient();
        private bool is_starting_first = true; 
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

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnCanvasTapped;
            TspCanvas.GestureRecognizers.Add(tapGestureRecognizer);
        }

        private void OnGenerateCitiesClicked(object sender, EventArgs e)
        {
            allCities.Clear();

            
            if (starting_city != null)
            {
                starting_city.setIsStarting(false);
                starting_city = null;
            }

            if (target_city != null)
            {
                target_city.setIsTarget(false);
                target_city = null;
            }

            is_starting_first = true;

            if (!int.TryParse(CitiesCountEntry.Text, out int numCities) || numCities < 4)
            {
                DisplayAlert("Error occurred", "You need to have at least 4 cities", "OK");
                return;
            }

            for (int i = 0; i < numCities; i++)
            {
                string name = cityNames[random.Next(cityNames.Length)];
                double x = random.NextDouble() * (TspCanvas.Width * 0.8) + (TspCanvas.Width * 0.1);
                double y = random.NextDouble() * (TspCanvas.Height * 0.8) + (TspCanvas.Height * 0.1);
                Cities city = new Cities(name, x, y, new List<Cities>(), false);
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

        private void OnCanvasTapped(object sender, EventArgs e)
        {
            if (allCities.Count == 0)
                return;


            Point tapLocation = new Point();
            if (e is TappedEventArgs tappedEvent)
            {
                tapLocation =(Point)tappedEvent.GetPosition(TspCanvas);
            }

           
            Cities closestCity = FindClosestCity(tapLocation.X, tapLocation.Y);

            if (closestCity != null)
            {
                if (is_starting_first)
                {
                    
                    if (target_city != null)
                    {
                        target_city.setIsTarget(false);
                        target_city = null;
                    }

                    
                    if (starting_city != null)
                    {
                        starting_city.setIsStarting(false);
                    }

                    
                    starting_city = closestCity;
                    starting_city.setIsStarting(true);

                    
                    is_starting_first = false;

                    DisplayAlert("Starting City", $"Selected {starting_city.getName()} as starting city", "OK");
                }
                else
                {
                  
                    if (closestCity == starting_city)
                    {
                        DisplayAlert("Invalid Selection", "Starting and target cities must be different", "OK");
                    }
                    else
                    {
                        
                        if (target_city != null)
                        {
                            target_city.setIsTarget(false);
                        }

                        target_city = closestCity;
                        target_city.setIsTarget(true);  
                        is_starting_first = true; 

                        DisplayAlert("Target City", $"Selected {target_city.getName()} as target city", "OK");
                    }
                }

                TspCanvas.Invalidate();
            }
        }

        private Cities FindClosestCity(double x, double y)
        {
            const double maxDistance = 30;
            Cities closest = null;
            double minDistance = double.MaxValue;

            foreach (var city in allCities)
            {
                double distance = Math.Sqrt(Math.Pow(x - city.getCoordinationX(), 2) + Math.Pow(y - city.getCoordinationY(), 2));
                if (distance < minDistance && distance < maxDistance)
                {
                    minDistance = distance;
                    closest = city;
                }
            }

            return closest;
        }

        private async void OnSolveClicked(object sender, EventArgs e)
        {
            try
            {
                if (starting_city == null || target_city == null)
                {
                    await DisplayAlert("No Starting and Finishing", "You need to select starting and finishing cities", "OK");
                    return;
                }

                if (allCities == null || allCities.Count == 0)
                {
                    await DisplayAlert("No Cities", "Please generate cities first", "OK");
                    return;
                }

                client.BaseAddress = new Uri("http://localhost:5000");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Maui TSP Solver");

                
                var requestData = new
                {
                    StartingCity = starting_city,
                    TargetCity = target_city,
                    AllCities = allCities
                };

                
                var response = await client.PostAsJsonAsync("/solve", requestData);

                if (response.IsSuccessStatusCode)
                {
                  
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    Console.WriteLine(result);

                    TspCanvas.Invalidate();
                    await DisplayAlert("Solution", "Path calculated successfully!", "OK");
                }
                else
                {
                    await DisplayAlert("Server Error", $"Server returned: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}