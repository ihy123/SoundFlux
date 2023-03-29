using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SoundFlux.Services
{
    public abstract class SettingsManager
    {
        public abstract string SettingsDirectory { get; }

        public virtual string SettingsFileName => "soundflux.settings.xml";

        public Dictionary<string, Dictionary<string, string>> Sections { get; private set; } = new();

        public virtual bool Load()
        {
            string path = SettingsDirectory + SettingsFileName;
            if (!File.Exists(path)) return false;

            XElement? loadedSections;

            try
            {
                loadedSections = XDocument.Load(path).Element("sections");
            }
            catch (XmlException)
            {
                return false;
            }

            if (loadedSections == null) return false;
            Sections.Clear();

            // iterate sections
            foreach (var s in loadedSections.Elements())
            {
                Dictionary<string, string> sect = new();

                // iterate section children
                foreach (var c in s.Elements())
                    sect.Add(c.Name.LocalName, c.Value);

                Sections.Add(s.Name.LocalName, sect);
            }
            return true;
        }

        public virtual void Save()
        {
            XElement docSections = new("sections");
            XDocument doc = new(new XDeclaration("1.0", "UTF-8", "yes"), docSections);

            // iterate sections
            foreach (var s in Sections)
            {
                List<XElement> children = new();

                // iterate section children
                foreach (var c in s.Value)
                    children.Add(new(c.Key, new XText(c.Value)));

                docSections.Add(new XElement(s.Key, children));
            }

            // save file
            Directory.CreateDirectory(SettingsDirectory);
            doc.Save(SettingsDirectory + SettingsFileName);
        }

        public void Set<T>(string section, string name, T value)
            => Set(section, name, value?.ToString() ?? string.Empty);

        public void Set(string section, string name, string value)
            => OpenSection(section)[name] = value;

        public bool Get(string section, string name, out string? value)
            => OpenSection(section).TryGetValue(name, out value);

        public string? Get(string section, string name, string? defaultValue)
            => Get(section, name, out string? value) ? value : defaultValue;

        public int Get(string section, string name, int defaultValue)
            => Get(section, name, out string? value) && int.TryParse(value, out int r) ? r : defaultValue;

        public double Get(string section, string name, double defaultValue)
            => Get(section, name, out string? value) && double.TryParse(value, out double r) ? r : defaultValue;

        public bool Get(string section, string name, bool defaultValue)
            => Get(section, name, out string? value) && bool.TryParse(value, out bool r) ? r : defaultValue;

        private Dictionary<string, string> OpenSection(string section)
        {
            // try to get section
            if (!Sections.TryGetValue(section, out var sect))
            {
                // create if it is not there
                sect = new();
                Sections.Add(section, sect);
            }
            return sect;
        }
    }
}
