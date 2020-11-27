using System.Collections.Generic;

namespace Dashy.Settings
{
    public class ContainerSettings
    {
        public string Title { get; set; } = "Dashy";
        public int? Width { get; set; } = 500;
        public int? Height { get; set; } = 400;
        public bool? TopMost { get; set; } = true;
        public bool? CanResize { get; set; } = true;
        public bool? HideClose { get; set; } = false;
        public string Background { get; set; } = "#111";
        public string Foreground { get; set; } = "#eee";
        public int? Columns { get; set; } = 1;
        public int? Rows { get; set; } = 1;
        public List<BrowserInstanceSettings> Views { get; set; }
    }
}