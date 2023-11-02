using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SimpleLauncher
{
    public class Software
    {
        public string Zipfile { get; set; }
        public string GameName { get; set; }
    }

    public class SoftwareCollection
    {
        public List<Software> Softwares { get; set; } = new List<Software>();

        public static SoftwareCollection LoadFromXml(string xmlPath)
        {
            var doc = XDocument.Load(xmlPath);
            var collection = new SoftwareCollection();

            foreach (var softwareElement in doc.Root.Elements("software"))
            {
                var software = new Software
                {
                    Zipfile = softwareElement.Element("zipfile").Value,
                    GameName = softwareElement.Element("gamename").Value
                };

                collection.Softwares.Add(software);
            }

            return collection;
        }
    }
}
