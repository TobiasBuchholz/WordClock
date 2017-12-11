using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.XamForms;
using WordClock.UI.Controls;
using WordClock.UI.Models;
using WordClock.UI.ViewModels;
using Xamarin.Forms;

namespace WordClock.UI.Views
{
    public partial class MainPage : ReactiveContentPage<IMainViewModel>
    {
        private double _initialNightModePickerRowHeight = -1; 
        
        public MainPage()
        {
            InitializeComponent();

            this.WhenActivated(disposables => 
            {
                NightModeBrightnessPicker.Items = Enumerable.Range(0, 255).ToArray();
                
                this.WhenAnyValue(x => x.ViewModel.Color)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(x => Color.FromRgba(x.R/255f, x.G/255f, x.B/255f, x.A/32f))
                    .BindTo(this, x => x.Grid.BackgroundColor)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Color)
                    .Select(x => x.GetBrightness())
                    .Merge(this
                        .WhenAnyValue(x => x.ViewModel.Color.A)
                        .Select(x => x/255f)
                        .Skip(1))
                    .Select(x => 1 - x)
                    .Select(x => Color.FromRgb(x, x, x))
                    .SubscribeSafe(SetTextColors)
                    .DisposeWith(disposables);
                
                this.WhenAnyValue(x => x.ViewModel.ConnectionState.Message)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .BindTo(this, x => x.ConnectionStateLabel.Text)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.ConnectionState.Type)
                    .Select(x => x == StateType.Connecting)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .BindTo(this, x => x.ProgressIndicator.IsVisible)
                    .DisposeWith(disposables);
                    
                this.WhenAnyValue(x => x.ViewModel.ConnectionState.Type)
                    .Select(x => x == StateType.Connected)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Do(SetSliderIsEnabled)
                    .Select(x => x ? 1f : 0.5f)
                    .Do(SetSliderOpacity)
                    .SubscribeSafe()
                    .DisposeWith(disposables);
                
                this.WhenAnyValue(x => x.ViewModel.Red)
                    .BindTo(this, x => x.RedLabel.Text)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Green)
                    .BindTo(this, x => x.GreenLabel.Text)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Blue)
                    .BindTo(this, x => x.BlueLabel.Text)
                    .DisposeWith(disposables);
                
                this.WhenAnyValue(x => x.ViewModel.Alpha)
                    .BindTo(this, x => x.BrightnessLabel.Text)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Red)
                    .Take(1)
                    .BindTo(this, x => x.RedSlider.Value)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Green)
                    .Take(1)
                    .BindTo(this, x => x.GreenSlider.Value)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Blue)
                    .Take(1)
                    .BindTo(this, x => x.BlueSlider.Value)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Alpha)
                    .Take(1)
                    .BindTo(this, x => x.BrightnessSlider.Value)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.IsNightModeEnabled)
                    .Do(_ => HandleInitInitialNightModePickerRowHeight())
                    .Where(_ => _initialNightModePickerRowHeight > 0)
                    .Do(AnimateNightModePickerLayout)
                    .BindTo(this, x => x.NightModeSwitch.IsToggled)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.NightModeBrightness)
                    .BindTo(this, x => x.NightModeBrightnessPicker.SelectedIndex)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.NightModeFromTime)
                    .SubscribeSafe(x => NightModeFromTimePicker.GetUnderlyingPicker().Time = x)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.NightModeToTime)
                    .SubscribeSafe(x => NightModeToTimePicker.GetUnderlyingPicker().Time = x)
                    .DisposeWith(disposables);

                RedSlider
                    .ValueChange()
                    .Select(x => (int) x)
                    .BindTo(ViewModel, x => x.Red)
                    .DisposeWith(disposables);

                GreenSlider
                    .ValueChange()
                    .Select(x => (int) x)
                    .BindTo(ViewModel, x => x.Green)
                    .DisposeWith(disposables);

                BlueSlider
                    .ValueChange()
                    .Select(x => (int) x)
                    .BindTo(ViewModel, x => x.Blue)
                    .DisposeWith(disposables);

                BrightnessSlider
                    .ValueChange()
                    .Select(x => (int) x)
                    .BindTo(ViewModel, x => x.Alpha)
                    .DisposeWith(disposables);

                NightModeSwitch
                    .ToggleChange()
                    .BindTo(ViewModel, x => x.IsNightModeEnabled)
                    .DisposeWith(disposables);

                NightModeBrightnessPicker
                    .SelectionChange()
                    .BindTo(ViewModel, x => x.NightModeBrightness)
                    .DisposeWith(disposables);

                NightModeFromTimePicker
                    .TimeChange()
                    .BindTo(ViewModel, x => x.NightModeFromTime)
                    .DisposeWith(disposables);
                
                NightModeToTimePicker
                    .TimeChange()
                    .BindTo(ViewModel, x => x.NightModeToTime)
                    .DisposeWith(disposables);
            });
        }

        private void SetTextColors(Color color)
        {
            ConnectionStateLabel.TextColor = color;
            RedLabel.TextColor = color;
            GreenLabel.TextColor = color;
            BlueLabel.TextColor = color;
            BrightnessLabel.TextColor = color;
            RedPrefixLabel.TextColor = color;
            GreenPrefixLabel.TextColor = color;
            BluePrefixLabel.TextColor = color;
            BrightnessPrefixLabel.TextColor = color;
            ProgressIndicator.Color = color;
            NightModeSwitchLabel.TextColor = color;
        }

        private void SetSliderIsEnabled(bool isEnabled)
        {
            RedSlider.IsEnabled = isEnabled;
            GreenSlider.IsEnabled = isEnabled;
            BlueSlider.IsEnabled = isEnabled;
            BrightnessSlider.IsEnabled = isEnabled;
            NightModeSwitch.IsEnabled = isEnabled;
            NightModeBrightnessPicker.IsEnabled = isEnabled;
            NightModeFromTimePicker.IsEnabled = isEnabled;
            NightModeToTimePicker.IsEnabled = isEnabled;
        }

        private void SetSliderOpacity(float opacity)
        {
            RedSlider.Opacity = opacity;
            GreenSlider.Opacity = opacity;
            BlueSlider.Opacity = opacity;
            BrightnessSlider.Opacity = opacity;
            NightModeSwitch.Opacity = opacity;
            NightModeBrightnessPicker.Opacity = opacity;
            NightModeFromTimePicker.Opacity = opacity;
            NightModeToTimePicker.Opacity = opacity;
        }

        private void HandleInitInitialNightModePickerRowHeight()
        {
            if(_initialNightModePickerRowHeight < 0 && NightModePickerLayout.Height > 1) {
                _initialNightModePickerRowHeight = NightModePickerLayout.Height;
            }
        }
        
        private void AnimateNightModePickerLayout(bool animateIn)
        {
            if(animateIn) {
                AnimateNightModePickerRowHeight(NightModePickerRow.Height.Value, _initialNightModePickerRowHeight);
                NightModePickerLayout.FadeTo(1, 500);
            } else {
                AnimateNightModePickerRowHeight(_initialNightModePickerRowHeight, 0);
                NightModePickerLayout.FadeTo(0, 500);
            }
        }

        private void AnimateNightModePickerRowHeight(double fromHeight, double toHeight)
        {
            var animation = new Animation(d => NightModePickerRow.Height = new GridLength(Clamp(d, 0, double.MaxValue)),
                fromHeight, toHeight, Easing.CubicInOut);
            animation.Commit(this, "nightModePickerAnimation", 16, 600);
        }
       
        private static double Clamp(double value, double minValue, double maxValue)
        {
            if(value < minValue) {
                return minValue;
            }
            return value > maxValue ? maxValue : value;
        }
    }
}
