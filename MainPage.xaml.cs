using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TripApp
{
    public partial class MainPage : ContentPage
    {
        private Dictionary<Cities, int> allCities = new Dictionary<Cities, int>();
        private Random random = new Random();
        private Cities starting_city;

        private static HttpClient client = new HttpClient();
        private bool is_starting_first = true;
        private Cities target_city;

        private bool isWaitingForTap = false;
        private string pendingCityName = null;

        private Dictionary<string, List<string>> bestPaths = new Dictionary<string, List<string>>();
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
            TspCanvas.Drawable = new CitiesDrawable(allCities, bestPaths);
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnCanvasTapped;
            TspCanvas.GestureRecognizers.Add(tapGestureRecognizer);
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private void OnStartInteraction(object sender, TouchEventArgs e)
        {
            var touchPoint = e.Touches.FirstOrDefault();
            if (touchPoint != null)
            {
                // This condition was incorrect - fixed logical comparison
                if (allCities.Keys.Any(city => Math.Abs(city.GetX() - touchPoint.X) < 10 &&
                                               Math.Abs(city.GetY() - touchPoint.Y) < 10))
                {
                    starting_city = allCities.Keys.First(city =>
                        Math.Abs(city.GetX() - touchPoint.X) < 10 &&
                        Math.Abs(city.GetY() - touchPoint.Y) < 10);
                }
                else if (allCities.Keys.Any(city => Math.Abs(city.GetX() - touchPoint.X) < 10 &&
                                                    Math.Abs(city.GetY() - touchPoint.Y) < 10))
                {
                    target_city = allCities.Keys.First(city =>
                        Math.Abs(city.GetX() - touchPoint.X) < 10 &&
                        Math.Abs(city.GetY() - touchPoint.Y) < 10);
                }
            }
            TspCanvas.Invalidate();
        }

        private void OnDragInteraction(object sender, EventArgs e)
        {
            TspCanvas.Invalidate();
        }

        private void OnEndInteraction(object sender, EventArgs e)
        {
            TspCanvas.Invalidate();
        }

        private Cities checkDuplication(Dictionary<Cities, int> cities, Cities city)
        {
            foreach (var c in cities)
            {
                if (c.Key.GetName() == city.GetName())
                {
                    city.setName(GenerateRandomString(5));
                    return checkDuplication(cities, city); // Added recursion to handle multiple duplicates
                }
            }
            return city;
        }

        private void OnAddCityClicked(object sender, EventArgs e)
        {
            _ = StartAddingCity();
        }

        private async Task StartAddingCity()
        {
            // Check if cities have been generated
            if (allCities.Count == 0)
            {
                await DisplayAlert("Error", "You need to generate cities first.", "OK");
                return;
            }

            // Prompt user for city name
            string cityName = await DisplayPromptAsync(
                "Add City",
                "Enter the name of the city:",
                "OK",
                "Cancel"
            );

            // Handle cancellation or empty input
            if (string.IsNullOrWhiteSpace(cityName))
            {
                await DisplayAlert("Error", "City name cannot be empty.", "OK");
                return;
            }

            // Check if the city name already exists
            if (allCities.Any(city => city.Key.GetName().Equals(cityName, StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Error", "A city with this name already exists.", "OK");
                return;
            }

            // Store the pending city name
            pendingCityName = cityName;
            isWaitingForTap = true;

            await DisplayAlert("Set Location", "Now tap on the canvas to set the city location.", "OK");
        }


        private void OnDeleteCityClicked(object sender, EventArgs e)
        {
            _ = DeleteCityAsync();
        }

        private async Task DeleteCityAsync()
        {
            if (allCities.Count == 0)
            {
                await DisplayAlert("Error Occurred", "You need to generate cities first", "OK");
                return;
            }

            string cityToDelete = await DisplayPromptAsync("Delete City", "Enter the name of the city to delete", "OK", "Cancel");
            if (string.IsNullOrEmpty(cityToDelete))
                return;

            Cities cityToRemove = null;
            foreach (var city in allCities.Keys)
            {
                if (city.GetName().Equals(cityToDelete, StringComparison.OrdinalIgnoreCase))
                {
                    cityToRemove = city;
                    break;
                }
            }

            if (cityToRemove == null)
            {
                await DisplayAlert("Error Occurred", "City not found", "OK");
                return;
            }

            if (cityToRemove == starting_city)
            {
                bool answer = await DisplayAlert("Delete City", "Are you sure you want to delete the starting city?", "Yes", "No");
                if (answer)
                {
                    starting_city.SetIsStarting(false);
                    starting_city = null;
                }
                else
                {
                    return;
                }
            }

            if (cityToRemove == target_city)
            {
                target_city = null;
            }

            foreach (var city in allCities.Keys.ToList())
            {
                if (city.GetGoingTo().ContainsKey(cityToRemove))
                {
                    city.GetGoingTo().Remove(cityToRemove);
                }
            }

            allCities.Remove(cityToRemove);
            TspCanvas.Invalidate();
        }

        private void OnGenerateCitiesClicked(object sender, EventArgs e)
        {
            allCities.Clear();
            bestPaths.Clear();

            if (starting_city != null)
            {
                starting_city.SetIsStarting(false);
                starting_city = null;
            }

            target_city = null;
            is_starting_first = true;

            if (!int.TryParse(CitiesCountEntry.Text, out int numCities) || numCities < 4)
            {
                DisplayAlert("Error Occurred", "You need to have at least 4 cities", "OK");
                return;
            }
            if (numCities > cityNames.Length)
            {
                DisplayAlert("Error Occurred", $"You need to have at most {cityNames.Length} cities", "OK");
                return;
            }

            // Create a shuffled list of city names to avoid duplicates
            var shuffledCityNames = cityNames.OrderBy(x => random.Next()).Take(numCities).ToList();

            for (int i = 0; i < numCities; i++)
            {
                string name = shuffledCityNames[i];
                double x = random.NextDouble() * (TspCanvas.Width * 0.8) + (TspCanvas.Width * 0.1);
                double y = random.NextDouble() * (TspCanvas.Height * 0.8) + (TspCanvas.Height * 0.1);
                Dictionary<Cities, int> connections = new Dictionary<Cities, int>();
                Cities city = new Cities(name, x, y, connections, false);
                allCities.Add(city, 0);
            }

            foreach (var city in allCities)
            {
                int numConnections = random.Next(1, 3);
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

        // Fixed method signature - parameter types were incorrect
        private void OnCanvasDraging(object sender, EventArgs e)
        {
            TspCanvas.Invalidate();
        }

        private void OnCanvasTapped(object sender, EventArgs e)
        {
            Point tapLocation = new Point();
            if (e is TappedEventArgs tappedEvent)
            {
                tapLocation = (Point)tappedEvent.GetPosition((View)sender);
            }

            if (isWaitingForTap && !string.IsNullOrEmpty(pendingCityName))
            {
                // We're waiting for a tap to set a new city location
                double x = tapLocation.X;
                double y = tapLocation.Y;

                // Check if the location is already taken
                if (allCities.Any(city =>
                    Math.Abs(city.Key.GetX() - x) < 10 &&
                    Math.Abs(city.Key.GetY() - y) < 10))
                {
                    DisplayAlert("Error", "This location is too close to another city.", "OK");
                    return;
                }

                // Add the new city at the tapped location
                allCities.Add(new Cities(pendingCityName, x, y, new Dictionary<Cities, int>(), false), 0);

                // Reset the waiting state
                isWaitingForTap = false;
                pendingCityName = null;

                // Refresh the canvas
                TspCanvas.Invalidate();
                return;
            }

            // If we're not waiting for a tap to add a city, proceed with the original functionality
            if (allCities.Count == 0)
                return;

            Cities closestCity = FindClosestCity(tapLocation.X, tapLocation.Y);

            if (closestCity != null)
            {
                if (is_starting_first)
                {
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
                        target_city = closestCity;
                        DisplayAlert("Target City", $"Selected {target_city.GetName()} as target city", "OK");
                        is_starting_first = true;
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
                if (starting_city == null)
                {
                    await DisplayAlert("No Starting City", "You need to select a starting city", "OK");
                    return;
                }

                if (target_city == null)
                {
                    await DisplayAlert("No Target City", "You need to select a target city", "OK");
                    return;
                }

                if (allCities == null || allCities.Count == 0)
                {
                    await DisplayAlert("No Cities", "Please generate cities first", "OK");
                    return;
                }

                // Validate input fields before sending to server
                if (!double.TryParse(MutationRateEntry.Text, out double mutationRate) ||
                    mutationRate < 0 || mutationRate > 1)
                {
                    await DisplayAlert("Invalid Input", "Mutation rate must be a number between 0 and 1", "OK");
                    return;
                }

                if (!int.TryParse(GenerationsEntry.Text, out int generations) ||
                    generations <= 0)
                {
                    await DisplayAlert("Invalid Input", "Generations must be a positive number", "OK");
                    return;
                }

                if (!int.TryParse(PopulationSizeEntry.Text, out int population) ||
                    population <= 0)
                {
                    await DisplayAlert("Invalid Input", "Population size must be a positive number", "OK");
                    return;
                }

                client.BaseAddress = new Uri("http://localhost:5000");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Maui TSP Solver");

                var requestData = new
                {
                    MutationRate = mutationRate, // Fixed: Using parsed value instead of text
                    Generations = generations,    // Fixed: Using parsed value instead of text
                    Population = population,      // Fixed: Using parsed value instead of text
                    StartingCity = new
                    {
                        Name = starting_city.GetName(),
                        X = starting_city.GetX(),  // Added X coordinate
                        Y = starting_city.GetY(),  // Added Y coordinate
                        GoingTo = starting_city.GetGoingTo().Select(c => new {
                            Name = c.Key.GetName(),
                            X = c.Key.GetX(),
                            Y = c.Key.GetY(),
                            Cost = c.Value
                        }).ToList()
                    },
                    TargetCity = target_city.GetName(),
                    AllCities = allCities.Select(c => new
                    {
                        Name = c.Key.GetName(),
                        X = c.Key.GetX(),  // Added X coordinate
                        Y = c.Key.GetY(),  // Added Y coordinate
                        GoingTo = c.Key.GetGoingTo().Select(gc => new {
                            Name = gc.Key.GetName(),
                            Cost = gc.Value
                        }).ToList()
                    }).ToList()
                };

                var response = await client.PostAsJsonAsync("/solve", requestData);

                if (response.IsSuccessStatusCode)
                {
                    bestPaths.Clear();

                    var resultString = await response.Content.ReadAsStringAsync();
                    var resultJson = JsonDocument.Parse(resultString);

                    if (resultJson.RootElement.TryGetProperty("bestPaths", out var pathsElement))
                    {
                        foreach (var targetProperty in pathsElement.EnumerateObject())
                        {
                            string targetCity = targetProperty.Name;
                            var pathData = targetProperty.Value;

                            if (pathData.TryGetProperty("path", out var pathArray))
                            {
                                List<string> path = new List<string>();
                                foreach (var cityElement in pathArray.EnumerateArray())
                                {
                                    path.Add(cityElement.GetString());
                                }
                                bestPaths[targetCity] = path;
                            }
                        }
                    }

                    int totalCitiesReached = 0;
                    int totalCities = 0;

                    if (resultJson.RootElement.TryGetProperty("totalCitiesReached", out var citiesReachedElement))
                    {
                        totalCitiesReached = citiesReachedElement.GetInt32();
                    }

                    if (resultJson.RootElement.TryGetProperty("totalCities", out var totalCitiesElement))
                    {
                        totalCities = totalCitiesElement.GetInt32();
                    }

                    TspCanvas.Invalidate();

                    await DisplayAlert("Solution",
                        $"Paths calculated successfully!\n" +
                        $"Cities reached: {totalCitiesReached} out of {totalCities}", "OK");
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