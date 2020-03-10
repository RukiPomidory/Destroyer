using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net;
using Squirrel;

namespace Destroyer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly double upAngleLimit = 31.5;
        readonly double downAngleLimit = 0;

        readonly double maxDestroyHandleValue = 1.3;
        readonly double minDestroyHandleValue = -1.3;
        readonly double lockStart = 0;

        readonly string blackHolePath = "./black hole";

        string filePath;
        bool hasFuel = true;
        bool canIdleRotate = true;
        double lastAngleSetTime = millis();

        readonly SolidColorBrush defaultGlassColor = new SolidColorBrush(Color.FromArgb(0x19, 0, 0x51, 0));
        readonly SolidColorBrush boomGlassColor = new SolidColorBrush(Colors.White);

        readonly DispatcherTimer animationTimer;

        public MainWindow()
        {
            InitializeComponent();

            animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            lockStart = Canvas.GetLeft(LockSlider);
            FileImage.Visibility = Visibility.Hidden;

            LockHandle.AngleChanged += LockHandle_ValueChanged;
            LockHandle.MaximumReached += LockHandle_MaximumReached;
            LockHandle.DragCompleted += () => canIdleRotate = true;
            LockHandle.DragStarted += () => canIdleRotate = false;

            LockHandle.MaxAngle = upAngleLimit;
            LockHandle.MinAngle = downAngleLimit;

            DestroyHandle.MaxAngle = maxDestroyHandleValue;
            DestroyHandle.MinAngle = minDestroyHandleValue;
            DestroyHandle.Angle = minDestroyHandleValue;
            DestroyHandle.DragCompleted += animationTimer.Start;
            DestroyHandle.MaximumReached += DestroyHandle_MaximumReached;

            if (!Directory.Exists(blackHolePath))
            {
                Directory.CreateDirectory(blackHolePath);
            }

            //sparkle = new Sparkle(appcastUrl);
            //sparkle.CheckForUpdatesAtUserRequest();

            //AutoUpdater.RunUpdateAsAdmin = true;
            //AutoUpdater.AppCastURL = autoUpdater;
            //AutoUpdater.ShowUpdateForm();
            //AutoUpdater.DownloadPath = 
            //AutoUpdater.Start(autoUpdater);

            //using (var mgr = new UpdateManager("C:\\Projects\\MyApp\\Releases"))
            //{
            //    //mgr.
            //}


        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (DestroyHandle.Angle <= DestroyHandle.MinAngle)
            {
                animationTimer.Stop();
                DestroyHandle.Angle = DestroyHandle.MinAngle;
                //return;
            }
            else
            {
                DestroyHandle.Angle -= 0.1;
            }

            if (false)//canIdleRotate)
            {
                var delta = (millis() - lastAngleSetTime) / 1000;
                var angleDelta = LockHandle.AngleSpeed * delta;
                LockHandle.Angle += angleDelta;
                lastAngleSetTime = millis();
                
                if (Math.Abs(LockHandle.AngleSpeed) < 1)
                {
                    if (Math.Abs(LockHandle.AngleSpeed) < 0.1)
                    {
                        LockHandle.AngleSpeed = 0;
                    }

                    LockHandle.AngleSpeed *= 0.9;
                }
                else
                {
                    LockHandle.AngleSpeed *= 0.99;
                }
            }
        }

        private void LockHandle_MaximumReached()
        {
            canIdleRotate = false;

            if (!hasFuel)
            {
                //hasFuel = true;
            }
        }

        private void DestroyHandle_MaximumReached()
        {
            if (LockHandle.MaxAngle - LockHandle.Angle < 0.1)
            {
                Destroy();
            }
        }

        private void LockHandle_ValueChanged(double angle)
        {
            Data.Text = LockHandle.AngleSpeed.ToString();
            Canvas.SetLeft(LockSlider, lockStart + 2 * angle);

            var act = LockHandle.Angle;
            var min = LockHandle.MinAngle;
            var max = LockHandle.MaxAngle;

            if ((act - min) / (max - min) < 0.5)
            {
                Glass.AllowDrop = true; 
            }
            else
            {
                Glass.AllowDrop = false;
            }
        }

        private void StartDragWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {

            }
        }

        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files is null) return;

                filePath = files[0];
                hasFuel = true;
                FileImage.Visibility = Visibility.Visible;

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                //HandleFileOpen(files[0]);
            }
        }

        private void Destroy()
        {
            if (filePath is null || !hasFuel)
            {
                return;
            }

            try
            {
                var fileName = System.IO.Path.GetFileName(filePath);
                File.Move(filePath, System.IO.Path.Combine(blackHolePath, fileName));
            }
            catch
            {
                return;
            }

            var animation = new ColorAnimation
            {
                From = boomGlassColor.Color,
                To = defaultGlassColor.Color,
                Duration = new Duration(TimeSpan.FromSeconds(4))
            };

            Glass.Fill.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            filePath = null;
            hasFuel = false;
            FileImage.Visibility = Visibility.Hidden;
        }

        static double millis() => CircularHandle.millis();
    }
}
