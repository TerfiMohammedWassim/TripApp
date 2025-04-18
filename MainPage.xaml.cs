﻿using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Maui.Animations;


namespace TripApp
{


    public partial class MainPage : ContentPage
    {
        private List<Cities> allCities = new List<Cities>();
        private Random random = new Random();
        private List<Cities> bestPaths = new List<Cities>();
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
        }




        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }



        private double CalculateDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        private Cities checkDuplication(Dictionary<Cities, int> cities, Cities city)
        {
            foreach (var c in cities)
            {
                if (c.Key.GetName() == city.GetName())
                {
                    city.setName(GenerateRandomString(5));
                    return checkDuplication(cities, city);
                }
            }
            return city;
        }

        private void OnGenerateCitiesClicked(object sender, EventArgs e)
        {
            allCities.Clear();
            bestPaths.Clear();

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

            var shuffledCityNames = cityNames.OrderBy(x => random.Next()).Take(numCities).ToList();

            for (int i = 0; i < numCities; i++)
            {
                string name = shuffledCityNames[i];
                double x = random.NextDouble() * (TspCanvas.Width * 0.8) + (TspCanvas.Width * 0.1);
                double y = random.NextDouble() * (TspCanvas.Height * 0.8) + (TspCanvas.Height * 0.1);
                List<Cities> connections = new List<Cities>();
                Cities city = new Cities(name, x, y, connections);
                allCities.Add(city);
            }

            foreach (var city in allCities)
            {
                int numConnections = random.Next(1, 3);
                for (int i = 0; i < numConnections; i++)
                {
                    var potentialCities = allCities.Where(c => c != city && !city.GetLinkedTo().Contains(c)).ToList();
                    if (potentialCities.Count > 0)
                    {
                        Cities targetCity = potentialCities[random.Next(potentialCities.Count)];
                        int cost = random.Next(1, 30);
                        city.GetLinkedTo().Add(targetCity);
                    }
                }
            }
            TspCanvas.Invalidate();
        }
        private void OnCanvasDraging(object sender, EventArgs e)
        {
            TspCanvas.Invalidate();
        }

        private async void OnSolveClicked(object sender, EventArgs e)
        {
            try
            {
                if (allCities == null || allCities.Count == 0)
                {
                    await DisplayAlert("No Cities", "Please generate cities first", "OK");
                    return;
                }

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

               
                bestPaths.Clear();


                using(var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5000");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Add("User-Agent", "Maui TSP Solver");
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var requestData = new
                    {
                        mutationRate = mutationRate.ToString(),
                        generations = generations.ToString(),
                        population = population.ToString(),
                        allCities = allCities.Select(c => new
                        {
                            name = c.GetName(),
                            x = c.GetX(),
                            y = c.GetY(),
                            linkedTo = c.GetLinkedTo().Select(gc => new
                            {
                                name = gc.GetName(),
                                x = gc.GetX(), 
                                y = gc.GetY()  
                            }).ToList()
                        }).ToList()
                    };

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = false, 
                        MaxDepth = 64 
                    };

                    var response = await client.PostAsJsonAsync("/solve", requestData, options);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonDocument.Parse(responseContent);
                        

                        if (apiResponse.RootElement.TryGetProperty("bestPaths", out var pathsElement))
                        {
                            foreach (var path in pathsElement.EnumerateArray())
                            {
                                string cityname = path.ToString();
                                foreach (var city in allCities)
                                {
                                    if (city.GetName() == cityname)
                                    {
                                        bestPaths.Add(city);
                                    }
                                }
                            }

                            if (bestPaths.Count > 0)
                            {
                                bestPaths.Add(bestPaths[0]);
                            }

                            TspCanvas.Invalidate();
                        }

                        SolutionPathLabel.Text = $"Best Path: {string.Join(" -> ", bestPaths.Select(c => c.GetName()))}";

                        GenerationLabel.Text = $"Generations: {apiResponse.RootElement.GetProperty("generations").ToString()}";

                        if (apiResponse.RootElement.TryGetProperty("bestDistance",out var citiesReached))
                        {
                            DistanceLabel.Text = $"Total Distance: {citiesReached.ToString()}";
                        }

                        if(apiResponse.RootElement.TryGetProperty("bestFitness",out var bestFitness))
                        {
                            FitnessLabel.Text = $"Best Fitness: {bestFitness.ToString()}";
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        await DisplayAlert("Server Error",
                            $"Status: {response.StatusCode}\nDetails: {errorContent}", "OK");
                    }

                }
                
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }

}