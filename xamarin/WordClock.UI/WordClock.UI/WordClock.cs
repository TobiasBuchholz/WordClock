using System;
using Akavache;
using WordClock.UI.Views;
using Xamarin.Forms;

namespace WordClock.UI
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new MainPage()) 
            {
                BarTextColor = Color.White 
            };
            BlobCache.ApplicationName = "WordClock.UI";
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
