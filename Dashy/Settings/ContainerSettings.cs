using System.Collections.Generic;

namespace Dashy.Settings
{
    public class ContainerSettings
    {
        public string Title { get; set; } = "";
        public uint Width { get; set; } = 500;
        public uint Height { get; set; } = 400;
        public uint Padding { get; set; } = 1;
        public uint CornerRadius { get; set; } = 0;
        public bool TopMost { get; set; } = false;
        public bool CanResize { get; set; } = true;
        public bool HideClose { get; set; } = false;
        public string Background { get; set; } = "#111";
        public string Foreground { get; set; } = "#eee";
        public string[] Columns { get; set; }
        public string[] Rows { get; set; }
        public List<BrowserInstanceSettings> Views { get; set; }
    }
}