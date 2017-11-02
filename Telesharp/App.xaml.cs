using System.Globalization;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TLSharp.Core;

namespace Telesharp
{
    public partial class App : Application
    {
        public static void SelectCulture(string culture)
        {
            // List all our resources      
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
                dictionaryList.Add(dictionary);

            // We want our specific culture      
            string requestedCulture = string.Format("StringResources.{0}.xaml", culture);
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault
                (d => d.Source.OriginalString == requestedCulture);
            if (resourceDictionary == null)
            {
                requestedCulture = "StringResources.xaml";
                resourceDictionary = dictionaryList.FirstOrDefault
                    (d => d.Source.OriginalString == requestedCulture);
            }

            // If we have the requested resource, remove it from the list and place at the end
            // Then this language will be our string table to use.     
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }

            // Inform the threads of the new culture      
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }

        public App()
        {
            if (Telegram.IsUserAuthorized())
            {
                StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            }
            else
            {
                StartupUri = new Uri("LoginWindow.xaml", UriKind.Relative);
            }
        }
    }
}

