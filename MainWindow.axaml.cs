using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace Hades2
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, int> characterStatus = new Dictionary<string, int>();
        private List<string> characters = new List<string>
        {
            "Melinoë", "Hecate", "Moros", "Apollo", "Nemesis", "Dora",
            "Chronos", "Odysseus", "Hestia", "Hephaestus", "Selene",
            "Arachne", "Eris", "Narcissus", "Scylla", "Hera", "Circe",
            "Heracles", "Medea", "Polyphemus", "Icarus", "Schelemeus",
            "Echo", "Hades", "Zeus", "Demeter", "Poseidon", "Artemis",
            "Aphrodite", "Hypnos", "Charon", "Hermes", "Chaos"
        };
        private StackPanel characterList;
        private string currentSortCriteria = "Name";
        private bool sortAscending = true;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            characterList = this.FindControl<StackPanel>("CharacterList");
            LoadProgress();
            UpdateCharacterList();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UpdateCharacterList(string filter = "")
        {
            characterList.Children.Clear();
            var filteredCharacters = characters.Where(c => c.ToLower().Contains(filter.ToLower()));
            IEnumerable<string> sortedCharacters;

            if (currentSortCriteria == "Name")
            {
                sortedCharacters = sortAscending ? filteredCharacters.OrderBy(c => c) : filteredCharacters.OrderByDescending(c => c);
            }
            else // "Nectar Count"
            {
                sortedCharacters = sortAscending ? filteredCharacters.OrderBy(c => characterStatus[c]) : filteredCharacters.OrderByDescending(c => characterStatus[c]);
            }

            foreach (var character in sortedCharacters)
            {
                var stackPanel = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("3*,1*,1*,1*"),
                    Margin = new Thickness(5)
                };

                var nameLabel = new TextBlock { Text = character, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5) };
                Grid.SetColumn(nameLabel, 0);
                stackPanel.Children.Add(nameLabel);

                var countLabel = new TextBlock { Text = characterStatus[character].ToString(), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5) };
                countLabel.Name = $"Count_{character.Replace(" ", "_")}";
                Grid.SetColumn(countLabel, 1);
                stackPanel.Children.Add(countLabel);

                var decreaseButton = new Button
                {
                    Content = "-",
                    Width = 50,
                    Height = 40, // Increase height for better alignment
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };
                ToolTip.SetTip(decreaseButton, "Decrease Nectar Count");
                decreaseButton.Click += (s, e) => ChangeNectarCount(character, -1);
                Grid.SetColumn(decreaseButton, 2);
                stackPanel.Children.Add(decreaseButton);

                var increaseButton = new Button
                {
                    Content = "+",
                    Width = 50,
                    Height = 40, // Increase height for better alignment
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };
                ToolTip.SetTip(increaseButton, "Increase Nectar Count");
                increaseButton.Click += (s, e) => ChangeNectarCount(character, 1);
                Grid.SetColumn(increaseButton, 3);
                stackPanel.Children.Add(increaseButton);

                characterList.Children.Add(stackPanel);
            }
        }

        private void ChangeNectarCount(string character, int change)
        {
            if (characterStatus.ContainsKey(character))
            {
                characterStatus[character] += change;
                if (characterStatus[character] < 0)
                {
                    characterStatus[character] = 0;
                }

                // Update the count label
                var countLabel = characterList.Children
                    .OfType<Grid>()
                    .SelectMany(panel => panel.Children.OfType<TextBlock>())
                    .FirstOrDefault(label => label.Name == $"Count_{character.Replace(" ", "_")}");
                if (countLabel != null)
                {
                    countLabel.Text = characterStatus[character].ToString();
                }
            }
        }

        private void SearchBox_KeyUp(object sender, Avalonia.Input.KeyEventArgs e)
        {
            var searchBox = sender as TextBox;
            UpdateCharacterList(searchBox?.Text ?? string.Empty);
        }

        private void SortColumn_Click(object sender, PointerPressedEventArgs e)
        {
            var header = sender as TextBlock;
            var criteria = header.Tag.ToString();
            if (currentSortCriteria == criteria)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortCriteria = criteria;
                sortAscending = true;
            }
            UpdateCharacterList();
        }

        private async void SaveProgress_Click(object sender, RoutedEventArgs e)
        {
            SaveProgress();
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                new MessageBoxStandardParams
                {
                    ContentTitle = "Hades 2 Nectar Tracker",
                    ContentMessage = "Progress saved!",
                    ButtonDefinitions = ButtonEnum.Ok
                });

            await messageBox.ShowAsync();
        }

        public void SaveProgress()
        {
            using (StreamWriter file = new StreamWriter("nectar_progress.txt"))
            {
                foreach (var character in characterStatus)
                {
                    file.WriteLine($"{character.Key}: {character.Value}");
                }
            }
        }

        private void LoadProgress()
        {
            if (File.Exists("nectar_progress.txt"))
            {
                foreach (var line in File.ReadAllLines("nectar_progress.txt"))
                {
                    var parts = line.Split(": ");
                    characterStatus[parts[0]] = int.Parse(parts[1]);
                }
            }
            else
            {
                foreach (var character in characters)
                {
                    characterStatus[character] = 0;
                }
            }
        }
    }
}
