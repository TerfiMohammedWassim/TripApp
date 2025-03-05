using System.Net.Http.Json;

namespace TripApp
{
    public partial class MainPage : ContentPage
    {
        private Dictionary<Cities, int> allCities = new Dictionary<Cities, int>();
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
                starting_city.SetIsStarting(false);
                starting_city = null;
            }

            if (target_city != null)
            {
                target_city.SetIsTarget(false);
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
                Dictionary<Cities, int> connections = new Dictionary<Cities, int>();
                Cities city = new Cities(name, x, y, connections, false);
                allCities.Add(city, 0);
            }

            foreach (var city in allCities)
            {
                int numConnections = random.Next(1, 4);
                for (int i = 0; i < numConnections; i++)
                {
                    var potentialCities = allCities.Keys.Where(c => c != city.Key && !city.Key.GetGoingTo().ContainsKey(c)).ToList();
                    if (potentialCities.Count > 0)
                    {
                        Cities targetCity = potentialCities[random.Next(potentialCities.Count)];
                        int cost = random.Next(1, 30);
                        city.Key.GetGoingTo().Add(targetCity, cost);
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
                tapLocation = (Point)tappedEvent.GetPosition(TspCanvas);
            }

            Cities closestCity = FindClosestCity(tapLocation.X, tapLocation.Y);

            if (closestCity != null)
            {
                if (is_starting_first)
                {
                    if (target_city != null)
                    {
                        target_city.SetIsTarget(false);
                        target_city = null;
                    }

                    if (starting_city != null)
                    {
                        starting_city.SetIsStarting(false);
                    }

                    starting_city = closestCity;
                    starting_city.SetIsStarting(true);

                    is_starting_first = false;

                    DisplayAlert("Starting City", $"Selected {starting_city.GetName()} as starting city", "OK");
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
                            target_city.SetIsTarget(false);
                        }

                        target_city = closestCity;
                        target_city.SetIsTarget(true);
                        is_starting_first = true;

                        DisplayAlert("Target City", $"Selected {target_city.GetName()} as target city", "OK");
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
                double distance = Math.Sqrt(Math.Pow(x - city.Key.GetX(), 2) + Math.Pow(y - city.Key.GetY(), 2));
                if (distance < minDistance && distance < maxDistance)
                {
                    minDistance = distance;
                    closest = city.Key;
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
                    MutationRate = MutationRateEntry.Text,
                    Generations = GenerationsEntry.Text,
                    Population = PopulationSizeEntry.Text,
                    StartingCity = new
                    {
                        Name = starting_city.GetName(),
                        X = starting_city.GetX(),
                        Y = starting_city.GetY(),
                        GoingTo = starting_city.GetGoingTo().Select(c => new {
                            Name = c.Key.GetName(),
                            X = c.Key.GetX(),
                            Y = c.Key.GetY(),
                            Cost = c.Value
                        }).ToList()
                    },
                    TargetCity = new
                    {
                        Name = target_city.GetName(),
                        X = target_city.GetX(),
                        Y = target_city.GetY()
                    },
                    AllCities = allCities.Select(c => new
                    {
                        Name = c.Key.GetName(),
                        X = c.Key.GetX(),
                        Y = c.Key.GetY(),
                        GoingTo = c.Key.GetGoingTo().Select(gc => new {
                            Name = gc.Key.GetName(),
                            X = gc.Key.GetX(),
                            Y = gc.Key.GetY(),
                            Cost = gc.Value
                        }).ToList()
                    }).ToList()
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