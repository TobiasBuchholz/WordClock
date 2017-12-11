using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Xamarin.Forms;

namespace WordClock.UI.Controls
{
    public static class ControlExtensions
    {
        public static IObservable<double> ValueChange(this Slider @this)
        {
            return Observable
                .FromEventPattern(@this, nameof(@this.ValueChanged))
                .Select(x => @this.Value);
        }
        
        public static IObservable<bool> ToggleChange(this Switch @this)
        {
            return Observable
                .FromEventPattern(@this, nameof(@this.Toggled))
                .Select(x => @this.IsToggled);
        }
        
        public static IObservable<int> SelectionChange(this MaterialPicker @this)
        {
            return Observable
                .FromEventPattern(@this, nameof(@this.SelectedIndexChanged))
                .Select(x => @this.SelectedIndex);
        }
        
        public static IObservable<TimeSpan> TimeChange(this MaterialTimePicker @this)
        {
            return Observable
                .FromEventPattern<PropertyChangedEventArgs>(@this, nameof(@this.PropertyChanged))
                .Where(x => x.EventArgs.PropertyName.Equals(nameof(TimePicker.Time)))
                .Where(_ => @this.Time.HasValue)
                .Select(x => @this.Time.Value);
        }
    }
}
