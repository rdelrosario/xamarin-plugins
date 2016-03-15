using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
//using Share.Forms.Plugin.WindowsPhone;

namespace ScolioMetro.WinPhone
{
    public partial class MainPage : global::Xamarin.Forms.Platform.WinPhone.FormsApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            SupportedOrientations = SupportedPageOrientation.Landscape;
            //ShareImplementation.Init();
            OxyPlot.Xamarin.Forms.Platform.WP8.Forms.Init();
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new ScolioMetro.App());
        }
    }
}
