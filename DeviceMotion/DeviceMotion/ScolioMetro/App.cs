using System;
using System.Collections.Generic;
using System.Diagnostics;
using DeviceMotion.Plugin;
using DeviceMotion.Plugin.Abstractions;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Xamarin.Forms;
using Plugin.Share;
//using Share.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace ScolioMetro
{
    /// <summary>
    ///     Represents the simple demo app.
    /// </summary>
    public class App : Application
    {
        private double _accelPitch;

        private double _accelRoll;

        private double _accelYaw;


        private double averageRoll;
        private double min, max, level;

        private readonly PlotModel model;

        private Label Pitch = new Label
        {
            XAlign = TextAlignment.Center,
            Text = "Pitch!"
        };

        private readonly PieSeries ps;
        private readonly PieSeries psInner;

        private Label Roll = new Label
        {
            XAlign = TextAlignment.Center,
            Text = "Roll!"
        };

        private readonly List<double> Rolls;
        private TimeSpan accelTimeSpan;
        private DateTime tempTime;
        private Label Yaw = new Label
        {
            XAlign = TextAlignment.Center,
            Text = "Yaw!"
        };

        private bool averaging = false;
        private bool calibrating = false;
        private double accelsPerSec = 25;

        /// <summary>
        ///     Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        public App()
        {
            Rolls = new List<double>();
            model = new PlotModel();
            ToggleAverageMode();

            ps = new PieSeries();
            ps.Slices.Add(new PieSlice("", 1) { IsExploded = true });
            ps.OutsideLabelFormat = "";
            //ps.TickRadialLength = 0;
            //ps.TickHorizontalLength = 0;
            ps.TextColor = OxyColor.Parse("#FF000000");
            ps.InnerDiameter = 0;
            ps.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
            ps.InsideLabelFormat = "0°";
            ps.ExplodedDistance = 0.0;
            ps.Selectable = true;
            ps.Stroke = OxyColors.White;
            ps.StrokeThickness = 2.0;
            ps.InsideLabelPosition = 0.5;
            ps.InsideLabelColor = OxyColor.Parse("#FF000000");
            ps.AngleSpan = 0;
            ps.StartAngle = -90;
            ps.Diameter = 0.85;
            model.Series.Add(ps);

            psInner = new PieSeries();
            psInner.Slices.Add(new PieSlice("", 1) { IsExploded = true });
            psInner.TextColor = OxyColor.Parse("#FF000000");
            psInner.OutsideLabelFormat = "";
            psInner.TickRadialLength = 0;
            psInner.TickHorizontalLength = 0;
            psInner.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
            psInner.InnerDiameter = 0.75;
            psInner.ExplodedDistance = 0.0;
            psInner.Selectable = false;
            psInner.Stroke = OxyColors.Black;
            psInner.StrokeThickness = 2.0;
            psInner.InsideLabelPosition = 0.0;
            psInner.AreInsideLabelsAngled = true;
            psInner.AngleSpan = 2;
            psInner.StartAngle = -89;

            model.Series.Add(psInner);

            ps.TouchCompleted += Ps_TouchCompleted;
            ps.MouseDown += Ps_MouseDown;
            psInner.TouchCompleted += Ps_TouchCompleted;
            psInner.MouseDown += Ps_MouseDown;

            MainPage = new NavigationPage(new ContentPage
            {
                Title = "ScolioMetro",
                //ToolbarItems =
                //{
                //    new ToolbarItem
                //    {
                //        Text = "Reset",
                //        ToolbarItemOrder = ToolbarItemOrder.Secondary,
                //        Command = new Command(() => { min=0; max=0; }),
                //    }
                //},
                Padding = new Thickness(0, 0, 0, 0),
                Content = new AbsoluteLayout
                {
                    VerticalOptions = LayoutOptions.Fill,
                    HorizontalOptions = LayoutOptions.Fill,
                    Children =
                    {
                        {
                            new PlotView
                            {
                                Model = model,
                                BackgroundColor = Color.White,
                                VerticalOptions = LayoutOptions.Fill,
                                HorizontalOptions = LayoutOptions.Fill
                            },
                            new Rectangle(0, 0, 1, 1),
                            AbsoluteLayoutFlags.All
                        },
                        {
                            new StackLayout
                            {
                                Children =
                                {
                                    new Label
                                    {
                                        Text =
                                            "ScolioMetro meassures patient's trunk asymmetry and its angle of rotation",
                                        TextColor = Color.Aqua,
                                        FontAttributes = FontAttributes.Bold,
                                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof (Label)),
                                        VerticalOptions = LayoutOptions.CenterAndExpand,
                                        HorizontalOptions = LayoutOptions.CenterAndExpand
                                    },
                                    new Label
                                    {
                                        Text =
                                            "hold phone to the patient's back, placing it on spine vertebrae and read the angles",
                                        TextColor = Color.Aqua,
                                        FontAttributes = FontAttributes.Italic,
                                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof (Label)),
                                        VerticalOptions = LayoutOptions.CenterAndExpand,
                                        HorizontalOptions = LayoutOptions.CenterAndExpand
                                    }
                                }
                            },
                            new Rectangle(0.5, 1, 1, 0.5),
                            AbsoluteLayoutFlags.All
                        }
                    }
                }

                //Content = new StackLayout
                //{
                //    VerticalOptions = LayoutOptions.Center,
                //    Children = {
                //            Pitch,
                //            Roll,
                //            Yaw,
                //            new PlotView
                //            {
                //                Model = model,
                //                VerticalOptions = LayoutOptions.Fill,
                //                HorizontalOptions = LayoutOptions.Fill,
                //                HeightRequest = 300,
                //                WidthRequest = 300
                //            }
                //        }
                //}
            })
            {
                BarBackgroundColor = Color.White,
                BarTextColor = Color.Aqua,
                Title = "ScolioMetro",
                BackgroundColor = Color.White
            };

            MainPage.ToolbarItems.Add(new ToolbarItem("Reset", Device.OnPlatform("", "", "Assets/Icons/reset.png"), () =>
            {
                max = 0;
                min = 0;
                level = 0;
            }, ToolbarItemOrder.Primary, 0));
            MainPage.ToolbarItems.Add(new ToolbarItem("Calibrate",
                string.Format("{0}{1}", Device.OnPlatform("Icons/", "", "Assets/Icons/"), "action.png"), () =>
                {
                    max = 0;
                    min = 0;
                    level = _accelRoll;
                }, ToolbarItemOrder.Primary, 0));
            MainPage.ToolbarItems.Add(new ToolbarItem("About app...", "",
                () =>
                {
                    if (MainPage.Navigation.NavigationStack.Count == 1) MainPage.Navigation.PushAsync(new AboutPage());
                },
                ToolbarItemOrder.Secondary));
            MainPage.ToolbarItems.Add(new ToolbarItem("Toggle average", "", () => { ToggleAverageMode(); },
                ToolbarItemOrder.Secondary));
            MainPage.ToolbarItems.Add(new ToolbarItem("Share your app...", "", () =>
            {
                CrossShare.Current.ShareLink("http://smarturl.it/ScolioMetro", "Share...", "Share");
            }, ToolbarItemOrder.Secondary));
        }

        private void ToggleAverageMode()
        {
            try
            {
                averaging ^= true;
                calibrating = true;
                tempTime = DateTime.Now;
                if (calibrating) CrossDeviceMotion.Current.SensorValueChanged += Calibrating;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Calibrating(object sender, SensorValueChangedEventArgs e)
        {
            if (averaging && calibrating)
            {
                accelTimeSpan = DateTime.Now - tempTime;
                if (accelTimeSpan.Milliseconds != 0)
                {
                    accelsPerSec = Math.Round(2000f / accelTimeSpan.Milliseconds, 0);
                    //Debug.WriteLine("Acceleromters per seconds:" + accelsPerSec);
                    calibrating = false;
                }
                else
                {
                    accelsPerSec = 25;
                    calibrating = false;
                }
            }

            if (!averaging || !calibrating)
                CrossDeviceMotion.Current.SensorValueChanged -= Calibrating;
        }

        public MotionVector Accelerometer { get; set; }
        public MotionVector Gyroscope { get; set; }

        private void Ps_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            //if ((e.ChangedButton != OxyMouseButton.Left))
            //{
            min = 0;
            max = 0;
            level = 0;
            //}
            e.Handled = true;
            //return;
            Debug.WriteLine("calibration mousedown event...");
        }

        private void Ps_TouchCompleted(object sender, OxyTouchEventArgs e)
        {
            min = 0;
            max = 0;
            level = 0;
            e.Handled = true;
            Debug.WriteLine("calibration touch event...");
        }

        /// <summary>
        ///     Handles when your app starts.
        /// </summary>
        protected override void OnStart()
        {
            try
            {
                //CrossDeviceMotion.Current.Start(MotionSensorType.Gyroscope);
                CrossDeviceMotion.Current.Start(MotionSensorType.Accelerometer, MotionSensorDelay.Fastest);
                CrossDeviceMotion.Current.SensorValueChanged += Current_SensorValueChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        /// <summary>
        ///     Handles when your app sleeps.
        /// </summary>
        protected override void OnSleep()
        {
            CrossDeviceMotion.Current.SensorValueChanged -= Current_SensorValueChanged;
            CrossDeviceMotion.Current.Stop(MotionSensorType.Accelerometer);
            //CrossDeviceMotion.Current.Stop(MotionSensorType.Gyroscope);
        }

        /// <summary>
        ///     Handles when your app resumes.
        /// </summary>
        protected override void OnResume()
        {
            //CrossDeviceMotion.Current.Start(MotionSensorType.Gyroscope);
            CrossDeviceMotion.Current.Start(MotionSensorType.Accelerometer, MotionSensorDelay.Fastest);
            CrossDeviceMotion.Current.SensorValueChanged += Current_SensorValueChanged;
        }

        private void Current_SensorValueChanged(object sender, SensorValueChangedEventArgs a)
        {
            switch (a.SensorType)
            {
                case MotionSensorType.Accelerometer:
                    
                    Accelerometer = (MotionVector)a.Value;

                    _accelPitch = 180 / Math.PI *
                                  Math.Atan(Accelerometer.X /
                                            Math.Sqrt(Math.Pow(Accelerometer.Y, 2) + Math.Pow(Accelerometer.Z, 2)));
                    _accelRoll = 180 / Math.PI *
                                 Math.Atan(Accelerometer.Y /
                                           Math.Sqrt(Math.Pow(Accelerometer.X, 2) + Math.Pow(Accelerometer.Z, 2))) -
                                 level;
                    _accelYaw = 180 / Math.PI *
                                Math.Atan(Math.Sqrt(Math.Pow(Accelerometer.X, 2) + Math.Pow(Accelerometer.Y, 2)) /
                                          Accelerometer.X);
                    break;

                case MotionSensorType.Gyroscope:

                    Gyroscope = (MotionVector)a.Value;
                    //_gyroPitch
                    //_gyroRoll
                    //_gyroYaw
                    break;
            }


            if (averaging)
            {
                min = _accelRoll;
                max = _accelRoll;
                Rolls.Add(_accelRoll);
                foreach (var m in Rolls)
                {
                    if (min > m) min = m;
                }                       
                foreach (var m in Rolls)
                {
                    if (max < m) max = m;
                }
                foreach (var roll in Rolls)
                {
                    _accelRoll += roll;
                }

                _accelRoll /= (Rolls.Count + 1);
                if (Rolls.Count > accelsPerSec) Rolls.RemoveAt(0);

            }
            else
            {
                if (_accelRoll * Math.Sign(_accelPitch) > 0)
                {
                    if (Math.Abs(_accelRoll) > max) max = Math.Abs(_accelRoll);
                }
                else
                {
                    if (Math.Abs(_accelRoll) > min) min = Math.Abs(_accelRoll);
                }
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                if (Math.Abs(min) < 5 && Math.Abs(max) < 5)
                {
                    ps.Slices[0].Fill = OxyColor.FromArgb(255, 0, 255, 0);
                }
                else
                {
                    if (Math.Abs(min) < 7 && Math.Abs(max) < 7)
                        ps.Slices[0].Fill = OxyColor.FromArgb(255, 255, 255, 0);
                    else
                        ps.Slices[0].Fill = OxyColor.FromArgb(255, 255, 0, 0);
                }

                if (averaging)
                {
                    ps.AngleSpan = Math.Abs(Math.Abs(max) - Math.Abs(min)) + 5;
                    ps.StartAngle = -90 - max * Math.Sign(_accelPitch) - 1.5;
                }
                else
                {
                    ps.AngleSpan = min + max;
                    ps.StartAngle = -90 - max;
                }

                psInner.StartAngle = -90 - _accelRoll * Math.Sign(_accelPitch);
                ps.InsideLabelFormat = Math.Round(Math.Abs(_accelRoll), 1) + "°";
                ps.OutsideLabelFormat = Math.Round(Math.Abs(min), 1) + "°-" + Math.Round(Math.Abs(max), 1) + "°";
                model.Series.Clear();
                model.Series.Add(ps);
                model.Series.Add(psInner);
                model.InvalidatePlot(true);
                //Pitch.Text = _accelPitch.ToString();
                //Roll.Text = _accelRoll.ToString();
                //Yaw.Text = _accelYaw.ToString();
            });
        }
    }
}