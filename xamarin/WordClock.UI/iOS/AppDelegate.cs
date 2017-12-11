using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace WordClock.UI.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

			Websockets.Ios.WebsocketConnection.Link();
            LoadApplication(new App());
            UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(66, 66, 66);
            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes{ ForegroundColor = UIColor.White };

            return base.FinishedLaunching(app, options);
        }
    }
}
