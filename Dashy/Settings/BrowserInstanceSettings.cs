using System;
using System.Collections.Generic;

namespace Dashy.Settings
{
    public class BrowserInstanceSettings
    {
        public Uri Url { get; set; }
        public double Zoom { get; set; } = 1.0;
        public uint Refresh { get; set; }
        public string Profile { get; set; } = "Default";
        public List<string> Js { get; set; } = new List<string>();
        public List<string> Css { get; set; } = new List<string>();
        public uint ColIndex { get; set; } = 0;
        public uint ColSpan { get; set; } = 1;
        public uint RowIndex { get; set; } = 0;
        public uint RowSpan { get; set; } = 1;
        public bool HandleInternalNavigation { get; set; } = true;
        public bool HandleExternalNavigation { get; set; } = true;
    }
}