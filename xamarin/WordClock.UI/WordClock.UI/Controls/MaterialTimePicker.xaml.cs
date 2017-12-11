using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace WordClock.UI.Controls
{
    public partial class MaterialTimePicker : ContentView
    {
        public static BindableProperty TimeProperty = BindableProperty.Create(nameof(Time), typeof(TimeSpan?), typeof(MaterialTimePicker), defaultBindingMode: BindingMode.TwoWay);
        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MaterialTimePicker), defaultBindingMode: BindingMode.TwoWay);
        public static BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(MaterialTimePicker), defaultBindingMode: BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newval) =>
        {
            var matEntry = (MaterialTimePicker)bindable;
            matEntry.EntryField.Placeholder = (string)newval;
            matEntry.HiddenLabel.Text = (string)newval;
        });

        public static BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(MaterialTimePicker), defaultValue: false, propertyChanged: (bindable, oldVal, newVal) =>
        {
            var matEntry = (MaterialTimePicker)bindable;
            matEntry.EntryField.IsPassword = (bool)newVal;
        });

        public static BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(MaterialTimePicker), defaultValue: Keyboard.Default, propertyChanged: (bindable, oldVal, newVal) =>
        {
            var matEntry = (MaterialTimePicker)bindable;
            matEntry.EntryField.Keyboard = (Keyboard)newVal;
        });

        public static BindableProperty AccentColorProperty = BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(MaterialTimePicker), defaultValue: Color.Accent);
        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(MaterialTimePicker), defaultValue: Color.White);
        public static BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(MaterialTimePicker), defaultValue: Color.Gray);

        public TimeSpan? Time {
            get => (TimeSpan?)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public Color AccentColor {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        public Color TextColor {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public Color PlaceholderColor {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }

        public Keyboard Keyboard {
            get => (Keyboard)GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        public bool IsPassword {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Placeholder {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public MaterialTimePicker()
        {
            InitializeComponent();
            EntryField.BindingContext = this;
            EntryField.TextColor = TextColor;
            EntryField.PlaceholderColor = PlaceholderColor;
            HiddenLabel.TextColor = Color.Gray;
            HiddenBottomBorder.BackgroundColor = Color.Gray;
            EntryField.Focused += (s, a) =>
            {
                Device.BeginInvokeOnMainThread(() => {
                    EntryField.Unfocus();
                    Picker.Focus();
                });
            };
            Picker.Focused += async (s, a) =>
            {
                HiddenLabel.IsVisible = true;
                if (string.IsNullOrEmpty(EntryField.Text))
                {
                    // animate both at the same time
                    await Task.WhenAll(
                        HiddenBottomBorder.LayoutTo(new Rectangle(BottomBorder.X, BottomBorder.Y, BottomBorder.Width, BottomBorder.Height), 200),
                        HiddenLabel.FadeTo(1, 60),
                        HiddenLabel.TranslateTo(HiddenLabel.TranslationX, EntryField.Y - EntryField.Height + 4, 200, Easing.BounceIn)
                    );
                    EntryField.Placeholder = null;
                }
                else
                {
                    await HiddenBottomBorder.LayoutTo(new Rectangle(BottomBorder.X, BottomBorder.Y, BottomBorder.Width, BottomBorder.Height), 200);
                }
            };
            Picker.Unfocused += async (s, a) =>
            {
                if (Time == null)
                {
                    // animate both at the same time
                    await Task.WhenAll(
                        HiddenBottomBorder.LayoutTo(new Rectangle(BottomBorder.X, BottomBorder.Y, 0, BottomBorder.Height), 200),
                        HiddenLabel.FadeTo(0, 180),
                        HiddenLabel.TranslateTo(HiddenLabel.TranslationX, EntryField.Y, 200, Easing.BounceIn)
                    );
                    EntryField.Placeholder = Placeholder;
                }
                else
                {
                    await HiddenBottomBorder.LayoutTo(new Rectangle(BottomBorder.X, BottomBorder.Y, 0, BottomBorder.Height), 200);
                }
            };

            Picker.PropertyChanged += Picker_PropertyChanged; ;
        }

        private void Picker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Picker.Time))
            {
                EntryField.Text = DateTime.Today.Add(Picker.Time).ToString("hh:mm tt");
                Time = Picker.Time;
            }
        }

        public TimePicker GetUnderlyingPicker()
        {
            return Picker;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            switch(propertyName) {
                case null:
                    return;
                case nameof(PlaceholderColor):
                    EntryField.PlaceholderColor = PlaceholderColor;
                    break;
                case nameof(Time):
                    HiddenLabel.IsVisible = true;
                    break;
            }
        }
    }
}