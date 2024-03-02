using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SimpleLauncher
{
    public class LetterNumberMenu
    {
        public StackPanel LetterPanel { get; private set; } = new StackPanel { Orientation = Orientation.Horizontal };
        private readonly Dictionary<string, Button> letterButtons = [];
        private Button selectedButton;

        public event Action<string> OnLetterSelected;

        public LetterNumberMenu()
        {
            InitializeLetterButtons();
            InitializeNumberButton();
        }

        private void InitializeLetterButtons()
        {
            foreach (char c in Enumerable.Range('A', 26).Select(x => (char)x))
            {
                Button button = new() { Content = c.ToString(), Width = 30, Height = 30 };

                button.Click += (_, _) =>
                {
                    UpdateSelectedButton(button);
                    OnLetterSelected?.Invoke(c.ToString());
                };

                letterButtons.Add(c.ToString(), button);
                LetterPanel.Children.Add(button);
            }
        }

        private void InitializeNumberButton()
        {
            Button numButton = new() { Content = "#", Width = 30, Height = 30 };
            numButton.Click += (_, _) =>
            {
                UpdateSelectedButton(numButton);
                OnLetterSelected?.Invoke("#");
            };

            LetterPanel.Children.Add(numButton);
        }

        private void UpdateSelectedButton(Button button)
        {
            if (selectedButton != null && selectedButton != button)
            {
                selectedButton.ClearValue(Control.BackgroundProperty);
            }

            button.Background = Brushes.Green;
            selectedButton = button;
        }

        public void SimulateClick(string letter)
        {
            if (letterButtons.TryGetValue(letter, out Button value))
            {
                value.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        public void DeselectLetter()
        {
            if (selectedButton != null)
            {
                selectedButton.ClearValue(Control.BackgroundProperty);
                selectedButton = null;
            }
        }
    }
}
