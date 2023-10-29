using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public class GamePagingHandler
    {
        private List<string> GameFilePaths { get; set; }
        private int CurrentPage { get; set; } = 1;
        private int PageSize { get; set; } = 10; // Or any default size you prefer

        public GamePagingHandler(List<string> gameFilePaths, int pageSize = 10)
        {
            GameFilePaths = gameFilePaths ?? throw new ArgumentNullException(nameof(gameFilePaths));
            PageSize = pageSize;
        }

        public List<string> GetCurrentPage()
        {
            return GameFilePaths
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public bool NextPage()
        {
            if ((CurrentPage * PageSize) < GameFilePaths.Count)
            {
                CurrentPage++;
                return true;
            }
            return false;
        }

        public bool PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                return true;
            }
            return false;
        }
    }

}
