using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace WordClock.UI.Controls
{
    public partial class MaterialPicker : ContentView
    {
        public event EventHandler SelectedIndexChanged;

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MaterialPicker), defaultBindingMode: BindingMode.TwoWay);
        public static BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(MaterialPicker), defaultBindingMode: BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newval) =>
        {
            var matPicker = (MaterialPicker)bindable;
            matPicker.HiddenLabel.Text = (string)newval;
        });
        public static BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IList), typeof(MaterialPicker), null);
        public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(MaterialPicker), 0, BindingMode.TwoWay);
        public static BindableProperty AccentColorProperty = BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(MaterialPicker), defaultValue: Color.Accent);
        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(MaterialPicker), defaultValue: Color.White);
        public static BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(MaterialPicker), defaultValue: Color.Gray);
        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(MaterialPicker), null, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            var matPicker = (MaterialPicker)bindable;
            matPicker.HiddenLabel.IsVisible = !string.IsNullOrEmpty(newValue?.ToString());
        });
        public static BindableProperty SelectedIndexChangedCommandProperty = BindableProperty.Create(nameof(SelectedIndexChangedCommand), typeof(ICommand), typeof(MaterialPicker), null);

        public ICommand SelectedIndexChangedCommand {
            get => (ICommand)GetValue(SelectedIndexChangedCommandProperty);
            set => SetValue(SelectedIndexChangedCommandProperty, value);
        }

        public object SelectedItem {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public int SelectedIndex {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public IList Items {
            get => (IList)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
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

        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Placeholder {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public MaterialPicker()
        {
            InitializeComponent();
            Picker.BindingContext = this;
            Picker.TextColor = TextColor;
            PlaceholderLabel.TextColor = PlaceholderColor;
            HiddenLabel.TextColor = Color.Gray;
            HiddenBottomBorder.BackgroundColor = Color.Gray;
            
            // TODO: Possible memory leak?
            Picker.SelectedIndexChanged += (sender, e) =>
            {
                SelectedIndexChangedCommand?.Execute(Picker.SelectedItem);
                SelectedIndexChanged?.Invoke(sender, e);
            };

            Picker.Focused += async (s, a) =>
            {
                HiddenLabel.IsVisible = true;
                if (Picker.SelectedItem == null)
                {
                    // animate both at the same time
                    await Task.WhenAll(
                        HiddenBottomBorder.LayoutTo(new Rectangle(BottomBorder.X, BottomBorder.Y, BottomBorder.Width, BottomBorder.Height), 200),
                        HiddenLabel.FadeTo(1, 60),
                        HiddenLabel.TranslateTo(HiddenLabel.TranslationX, Picker.Y - Picker.Height + 4, 200, Easing.BounceIn)
                    );
                    PlaceholderLabel.IsVisible = false;
                }
                else
                {
                    await HiddenBottomBorder.LayoutTo(new Rectangle(BottomBorder.X, BottomBorder.Y, BottomBorder.Width, BottomBorder.Height), 200);
                }
            };
            Picker.Unfocused += async (s, a) =>
            {
                if (Picker.SelectedItem == null)
                {
                    // animate both at the same time
                    await Task.WhenAll(
                        HiddenBottomBorder.LayoutTo(new Rectangle(BottomBorder.X, BottomBorder.Y, 0, BottomBorder.Height), 200),
                        HiddenLabel.FadeTo(0, 180),
                        HiddenLabel.TranslateTo(HiddenLabel.TranslationX, Picker.Y, 200, Easing.BounceIn)
                    );
                    PlaceholderLabel.IsVisible = true;
                }
                else
                {
                    await HiddenBottomBorder.LayoutTo(new Rectangle(BottomBorder.X, BottomBorder.Y, 0, BottomBorder.Height), 200);
                }
            };

        }

        public Picker GetUnderlyingPicker() => Picker;

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            switch(propertyName) {
                case null:
                    return;
                case nameof(Placeholder):
                    PlaceholderLabel.Text = Placeholder;
                    break;
                case nameof(PlaceholderColor):
                    PlaceholderLabel.TextColor = PlaceholderColor;
                    break;
                case nameof(SelectedIndex):
                    PlaceholderLabel.IsVisible = false;
                    break;
            }
        }
    }
}