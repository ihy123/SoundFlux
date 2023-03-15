using System.IO;
using System.Xml.Linq;

namespace SoundFlux
{
    public class SharedSettings
    {
        public class Section
        {
            private XElement section;

            public Section(XElement sect)
                => section = sect;

            public void Add(string name, string value)
                => section.Add(new XElement(name, new XText(value)));

            public void Add<T>(string name, T value)
                => Add(name, value.ToString());

            public string? Get(string name)
                => section.Element(name)?.Value;

            public string Get(string name, string _default)
            {
                string? val = Get(name);
                return val == null ? _default : val;
            }

            public int GetInt(string name, int _default = 0)
                => int.TryParse(Get(name), out int val) ? val : _default;

            public double GetDouble(string name, double _default = 0.0)
                => double.TryParse(Get(name), out double val) ? val : _default;

            public bool GetBool(string name, bool _default = false)
                => bool.TryParse(Get(name), out bool val) ? val : _default;
        }

        public const string SettingsFileName = "soundflux.settings.xml";

        public static readonly SharedSettings Instance = new();

        private XDocument? loaded;
        private XElement? loadedSections;

        private XDocument doc;
        private XElement docSections;

        private SharedSettings()
        {
            docSections = new XElement("sections");
            doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), docSections);
        }

        public void Load()
        {
            string path = PlatformUtilities.Instance.SettingsDirectory + SettingsFileName;
            if (File.Exists(path))
            {
                loaded = XDocument.Load(path);
                loadedSections = loaded.Element("sections");
            }
        }

        public Section? GetSection(string name)
        {
            var elem = loadedSections?.Element(name);
            return elem == null ? null : new(elem);
        }

        public void Save()
            => doc.Save(PlatformUtilities.Instance.SettingsDirectory + SettingsFileName);

        public Section AddSection(string name)
        {
            XElement sect = new(name);
            docSections.Add(sect);
            return new Section(sect);
        }
    }
}
