using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public class SystemConfig
    {
        public string SystemName { get; set; }
        public string SystemFolder { get; set; }
        public List<string> FileFormatsToSearch { get; set; }
        public bool ExtractFileBeforeLaunch { get; set; }
        public List<string> FileFormatsToLaunch { get; set; }
    }

}
