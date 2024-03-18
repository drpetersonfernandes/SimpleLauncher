using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleLauncher
{
    public class LetterNumberMenu
    {
        public StackPanel LetterPanel { get; private set; } = new() { Orientation = Orientation.Horizontal };
        private Button _selectedButton;

        public event Action<string> OnLetterSelected;

        public LetterNumberMenu()
        {
            InitializeAllButton();
            InitializeNumberButton();
            InitializeLetterButtons();
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
                LetterPanel.Children.Add(button);
            }
        }
        
        private void InitializeAllButton()
        {
            Button allButton = new Button { Content = "All", Width = 50, Height = 30 };
            allButton.Click += (_, _) =>
            {
                UpdateSelectedButton(allButton);
                OnLetterSelected?.Invoke(null); // Or use "All" or another identifier
            };
            LetterPanel.Children.Add(allButton);
        }

        private void UpdateSelectedButton(Button button)
        {
            if (_selectedButton != null && _selectedButton != button)
            {
                _selectedButton.ClearValue(Control.BackgroundProperty);
            }

            button.Background = Brushes.Green;
            _selectedButton = button;
        }

        public void DeselectLetter()
        {
            if (_selectedButton != null)
            {
                _selectedButton.ClearValue(Control.BackgroundProperty);
                _selectedButton = null;
            }
        }
    }
}
