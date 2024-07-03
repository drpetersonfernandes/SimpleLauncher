namespace SimpleLauncher
{
    public partial class Favorites
    {
        private FavoritesManager _favoritesManager;
        private string _filePath = "favorites.xml"; // Adjust the path as needed

        public Favorites()
        {
            InitializeComponent();
            _favoritesManager = new FavoritesManager(_filePath);
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            var favoritesConfig = _favoritesManager.LoadFavorites();
            FavoritesDataGrid.ItemsSource = favoritesConfig.FavoriteList;
        }
    }
}