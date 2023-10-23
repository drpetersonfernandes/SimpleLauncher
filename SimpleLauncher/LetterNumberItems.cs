using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleLauncher
{
    public class LetterNumberItems
    {
        public StackPanel LetterPanel { get; private set; } = new StackPanel { Orientation = Orientation.Horizontal };
        private Dictionary<string, Button> letterButtons = new Dictionary<string, Button>();
        private Button selectedButton = null;

        public event Action<string> OnLetterSelected;

        public LetterNumberItems()
        {
            InitializeLetterButtons();
            InitializeNumberButton();
        }

        private void InitializeLetterButtons()
        {
            foreach (char c in Enumerable.Range('A', 26).Select(x => (char)x))
            {
                Button button = new Button { Content = c.ToString(), Width = 30, Height = 30 };

                button.Click += (sender, e) =>
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
            Button numButton = new Button { Content = "#", Width = 30, Height = 30 };
            numButton.Click += (sender, e) =>
            {
                UpdateSelectedButton(numButton);
                OnLetterSelected?.Invoke("#");
            };

            LetterPanel.Children.Add(numButton);
        }

        private void UpdateSelectedButton(Button button)
        {
            if (selectedButton != null)
            {
                selectedButton.ClearValue(Button.BackgroundProperty);
            }

            button.Background = Brushes.Green;
            selectedButton = button;
        }

        public void SimulateClick(string letter)
        {
            if (letterButtons.ContainsKey(letter))
            {
                letterButtons[letter].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
    }
}
