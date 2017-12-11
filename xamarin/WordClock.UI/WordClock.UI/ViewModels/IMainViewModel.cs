using System;
using System.Drawing;
using WordClock.UI.Models;

namespace WordClock.UI.ViewModels
{
    public interface IMainViewModel
    {
        int Red { get; set; }
        int Green { get; set; }
        int Blue { get; set; }
        int Alpha { get; set; }
        bool IsNightModeEnabled { get; set; }
        int NightModeBrightness { get; set; }
        TimeSpan NightModeFromTime { get; set; }
        TimeSpan NightModeToTime { get; set; }
        ConnectionState ConnectionState { get; }
        Color Color { get; }
    }
}
