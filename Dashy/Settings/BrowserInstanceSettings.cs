using System;
using System.Collections.Generic;

namespace Dashy.Settings
{
    public class BrowserInstanceSettings
    {
        public Uri Url { get; set; }
        public double Zoom { get; set; } = 1.0;
        public int Refresh { get; set; }
        public string Profile { get; set; } = "Default";
        public List<string> Js { get; set; }
        public List<string> Css { get; set; }
        public int ColIndex { get; set; } = 0;
        public int ColSpan { get; set; } = 1;
        public int RowIndex { get; set; } = 0;
        public int RowSpan { get; set; } = 1;
        public bool HandleInternalNavigation { get; set; } = true;
        public bool HandleExternalNavigation { get; set; } = false;
    }
}