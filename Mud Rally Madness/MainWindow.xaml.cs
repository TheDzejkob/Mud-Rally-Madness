using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using System.Text.Json;

namespace MudRallyMadness
{
    public partial class MainWindow : Window
    {
        private int points = 0;
        private DispatcherTimer passiveIncomeTimer;

        private class Building
        {
            public string Name { get; set; }
            public int BaseCost { get; set; }
            public int Count { get; set; }
            public int PointsPerSec { get; set; }
            public double CurrentMultiplier { get; set; }
            public List<Upgrade> Upgrades { get; set; }

            public class Upgrade
            {
                public string Name { get; set; }
                public int Cost { get; set; }
                public double Multiplier { get; set; }
                public bool Purchased { get; set; }
            }
        }

        private Dictionary<string, Building> buildings;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            SetupPassiveIncome();
        }

        private void InitializeGame()
        {
            buildings = new Dictionary<string, Building>
            {
                {
                    "offRoadCamp", new Building
                    {
                        Name = "Off-Road Camp 🏕️",
                        BaseCost = 10,
                        Count = 0,
                        PointsPerSec = 1,
                        CurrentMultiplier = 1.0,
                        Upgrades = new List<Building.Upgrade>
                        {
                            new Building.Upgrade
                            {
                                Name = "Tougher Courses",
                                Cost = 50,
                                Multiplier = 1.5,
                                Purchased = false
                            },
                            new Building.Upgrade
                            {
                                Name = "Pro-Level Training",
                                Cost = 250,
                                Multiplier = 2.0,
                                Purchased = false
                            }
                        }
                    }
                },
                {
                    "mudFactory", new Building
                    {
                        Name = "Mud Factory 🚜",
                        BaseCost = 50,
                        Count = 0,
                        PointsPerSec = 5,
                        CurrentMultiplier = 1.0,
                        Upgrades = new List<Building.Upgrade>
                        {
                            new Building.Upgrade
                            {
                                Name = "High-Quality Mud",
                                Cost = 200,
                                Multiplier = 1.7,
                                Purchased = false
                            },
                            new Building.Upgrade
                            {
                                Name = "Alien Swamp Tech",
                                Cost = 750,
                                Multiplier = 2.5,
                                Purchased = false
                            }
                        }
                    }
                },
                {
                    "extremeRallyPark", new Building
                    {
                        Name = "Extreme Rally Park 🏟️",
                        BaseCost = 250,
                        Count = 0,
                        PointsPerSec = 25,
                        CurrentMultiplier = 1.0,
                        Upgrades = new List<Building.Upgrade>
                        {
                            new Building.Upgrade
                            {
                                Name = "Bigger Jumps",
                                Cost = 1000,
                                Multiplier = 2.0,
                                Purchased = false
                            },
                            new Building.Upgrade
                            {
                                Name = "Time-Travel Races",
                                Cost = 5000,
                                Multiplier = 3.0,
                                Purchased = false
                            }
                        }
                    }
                }
            };

            CreateBuildingControls();
            UpdatePointsDisplay();
        }

        private void CreateBuildingControls()
        {
            BuildingsPanel.Children.Clear();

            foreach (var buildingEntry in buildings)
            {
                var building = buildingEntry.Value;

                StackPanel buildingPanel = new StackPanel
                {
                    Margin = new Thickness(0, 10, 0, 10)
                };

                Border border = new Border
                {
                    Background = Brushes.White,
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(10),
                    CornerRadius = new CornerRadius(5),
                    Child = buildingPanel
                };

                Button buyButton = new Button
                {
                    Content = $"Buy {building.Name} (Cost: {building.BaseCost})",
                    Tag = buildingEntry.Key,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                buyButton.Click += BuyBuilding_Click;
                buildingPanel.Children.Add(buyButton);

                TextBlock countDisplay = new TextBlock
                {
                    Text = $"Count: {building.Count}",
                    Tag = $"{buildingEntry.Key}_count"
                };
                buildingPanel.Children.Add(countDisplay);

   

                BuildingsPanel.Children.Add(border);
            }
        }

        private void SetupPassiveIncome()
        {
            passiveIncomeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            passiveIncomeTimer.Tick += PassiveIncomeTimer_Tick;
            passiveIncomeTimer.Start();
        }

        private void PassiveIncomeTimer_Tick(object sender, EventArgs e)
        {
            int passivePoints = 0;
            foreach (var building in buildings.Values)
            {
                passivePoints += (int)(building.Count * building.PointsPerSec * building.CurrentMultiplier);
            }
            points += passivePoints;
            UpdatePointsDisplay();
        }

        private void ManualClickButton_Click(object sender, RoutedEventArgs e)
        {
            points += 1;
            UpdatePointsDisplay();
        }

        private void BuyBuilding_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string buildingKey = button.Tag.ToString();

            if (!buildings.ContainsKey(buildingKey))
            {
                MessageBox.Show($"Building not found: {buildingKey}", "Error");
                return;
            }

            Building building = buildings[buildingKey];

            if (points >= building.BaseCost)
            {
                points -= building.BaseCost;
                building.Count++;
                building.BaseCost = (int)(building.BaseCost * 1.15);


                UpdateBuildingUI(buildingKey, building);

                UpdatePointsDisplay();
            }
            else
            {
                MessageBox.Show("Not enough Mud Points to buy this building!", "Insufficient Funds");
            }
        }

        private void UpdateBuildingUI(string buildingKey, Building building)
        {

            foreach (UIElement elem in BuildingsPanel.Children)
            {
                if (elem is Border border && border.Child is StackPanel panel)
                {
                    foreach (UIElement child in panel.Children)
                    {
                        if (child is Button buyButton &&
                            buyButton.Tag != null &&
                            buyButton.Tag.ToString() == buildingKey)
                        {
                            buyButton.Content = $"Buy {building.Name} (Cost: {building.BaseCost})";

                            // Update count display
                            var countDisplay = panel.Children
                                .OfType<TextBlock>()
                                .FirstOrDefault(tb => tb.Tag != null && tb.Tag.ToString() == $"{buildingKey}_count");

                            if (countDisplay != null)
                            {
                                countDisplay.Text = $"Count: {building.Count}";
                            }

                            return;
                        }
                    }
                }
            }
        }

        private void SaveGame_Click(object sender, RoutedEventArgs e)
        {
            var gameState = new GameState
            {
                Points = points,
                Buildings = buildings
            };

            string jsonString = JsonSerializer.Serialize(gameState, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("mud_rally_save.json", jsonString);
            MessageBox.Show("Game saved successfully!", "Save Successful");
        }

        private void LoadGame_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("mud_rally_save.json"))
            {
                try
                {
                    string jsonString = File.ReadAllText("mud_rally_save.json");

     
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var loadedState = JsonSerializer.Deserialize<GameState>(jsonString, options);

                    points = loadedState.Points;
                    buildings = loadedState.Buildings;

   
                    BuildingsPanel.Children.Clear();
                    CreateBuildingControls();
                    UpdatePointsDisplay();

                    MessageBox.Show("Game loaded successfully!", "Load Successful");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading game: {ex.Message}", "Load Error");
                }
            }
            else
            {
                MessageBox.Show("No saved game found!", "Load Game");
            }
        }

        class GameState
        {
            public int Points { get; set; }
            public Dictionary<string, Building> Buildings { get; set; }
        }

        private void UpdatePointsDisplay()
        {
            PointsDisplay.Text = $"Mud Points: {points}";
        }
    }
}