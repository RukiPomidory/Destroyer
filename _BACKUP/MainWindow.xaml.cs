using System;
using System.Collections.Generic;
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

namespace Destroyer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double upAngleLimit = 31.5;
        const double downAngleLimit = 0;

        double ThumbX
        {
            get
            {
                return Canvas.GetLeft(CircleThumb) + CircleThumb.Width / 2;
            }

            set
            {
                Canvas.SetLeft(CircleThumb, value - CircleThumb.Width / 2);
            }
        }

        double ThumbY
        {
            get
            {
                return Canvas.GetTop(CircleThumb) + CircleThumb.Height / 2;
            }

            set
            {
                Canvas.SetTop(CircleThumb, value - CircleThumb.Height / 2);
            }
        }

        double xCenter = 200;
        double yCenter = 200;
        double radius = 100;
        double angle = 0;
        double lockStart = 0;

        SolidColorBrush defaultGlassColor = new SolidColorBrush(Color.FromArgb(0x19, 0, 0x51, 0));
        SolidColorBrush boomGlassColor = new SolidColorBrush(Colors.White);

        public MainWindow()
        {
            InitializeComponent();

            xCenter = Canvas.GetLeft(CenterOfRotate) + CenterOfRotate.Width / 2;
            yCenter = Canvas.GetTop(CenterOfRotate) + CenterOfRotate.Height / 2;
            radius = Stick.Width;

            Canvas.SetLeft(Stick, xCenter);
            Canvas.SetTop(Stick, yCenter - Stick.Height / 2);

            lockStart = Canvas.GetLeft(LockSlider);

            SetAngle(0);
            angle = GetAngle();

        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double xChange = e.HorizontalChange;
            double yChange = e.VerticalChange;

            double x = ThumbX + xChange;
            double y = ThumbY + yChange;

            double sin = GetSin(x, y);
            double cos = GetCos(x, y);

            x -= xCenter;
            y -= yCenter;
            double distance = Math.Sqrt(x * x + y * y);

            if (sin > 1 || cos > 1 || sin < -1 || cos < -1)
            {
                Resources["ThumbColor"] = new SolidColorBrush(Colors.Red);
            }

            if (distance < radius / 10)
            {
                CircleThumb.CancelDrag();
                return;
            }

            if (Double.IsNaN(sin))
            {
                sin = 0;
            }

            if (Double.IsNaN(cos))
            {
                cos = 0;
            }

            double this_angle_backup = this.angle;

            double newThumbX = xCenter + cos * radius;
            double newThumbY = yCenter + sin * radius;

            double angle = GetAngle(newThumbX, newThumbY);
            UpdateAngle(angle);
            Data.Text = $"{this_angle_backup}\n{this.angle}\n{angle}";

            double currentAngle = this.angle;

            if (this.angle < downAngleLimit)
            {
                if (GetAngle() != downAngleLimit)
                {
                    SetAngle(downAngleLimit);
                }

                return;
            }
            else if (this.angle > upAngleLimit)
            {
                if (GetAngle() != upAngleLimit)
                {
                    SetAngle(upAngleLimit);
                    Destroy();
                }

                return;
            }

            SetThumbPosition(newThumbX, newThumbY);

            Canvas.SetLeft(LockSlider, lockStart + 2 * currentAngle);

            RotateTransform rotateTransform = new RotateTransform(angle / Math.PI * 180);
            Stick.RenderTransform = rotateTransform;
        }

        private void CircleThumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (angle < downAngleLimit)
            {
                angle = downAngleLimit;
            }
            else if (angle > upAngleLimit)
            {
                angle = upAngleLimit;
            }
        }

        private void SetThumbPosition(double x, double y)
        {
            ThumbX = x;
            ThumbY = y;
        }

        private double GetCos(double x, double y)
        {
            x -= xCenter;
            y -= yCenter;

            double distance = Math.Sqrt(x * x + y * y);

            return x / distance;
        }

        private double GetSin(double x, double y)
        {
            x -= xCenter;
            y -= yCenter;

            double distance = Math.Sqrt(x * x + y * y);

            return y / distance;
        }

        private void SetAngle(double newAngle)
        {
            double cos = Math.Cos(newAngle);
            double sin = Math.Sin(newAngle);

            SetThumbPosition(xCenter + cos * radius, yCenter + sin * radius);

            RotateTransform rotateTransform = new RotateTransform(newAngle / Math.PI * 180);
            Stick.RenderTransform = rotateTransform;
        }

        private double GetAngle()
        {
            return GetAngle(ThumbX, ThumbY);
        }

        private double GetAngle(double x, double y)
        {
            double sin = GetSin(x, y);
            double cos = GetCos(x, y);

            double angle = Math.Acos(cos);

            if (sin < 0)
            {
                angle = 2 * Math.PI - angle;
            }

            return angle;
        }

        private void UpdateAngle(double newAngle)
        {
            if (newAngle < 0 || newAngle > Math.PI * 2)
            {
                return;
            }

            double RotationCount = angle / (Math.PI * 2);
            int count = (int)RotationCount;
            double localAngle = angle - count * Math.PI * 2;


            double change = newAngle - localAngle;

            if (change > Math.PI)
            {
                change -= Math.PI * 2;
            }
            else if (change < -Math.PI)
            {
                change += Math.PI * 2;
            }

            angle += change;


            // первый вариант

            //if (newAngle > localAngle) 
            //{
            //    if (newAngle - localAngle > Math.PI)
            //    {
            //        angle -= localAngle + Math.PI * 2 - newAngle;
            //    }
            //    else
            //    {
            //        angle += newAngle - localAngle;
            //    }
            //}
            //else if (localAngle - newAngle > Math.PI)
            //{
            //    angle += newAngle + Math.PI * 2 - localAngle;
            //}
            //else
            //{
            //    angle -= localAngle - newAngle;
            //}
        }

        private void StartDragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                //HandleFileOpen(files[0]);
            }
        }

        private void Destroy()
        {
            var animation = new ColorAnimation();
            animation.From = boomGlassColor.Color;
            animation.To = defaultGlassColor.Color;
            animation.Duration = new Duration(TimeSpan.FromSeconds(4));

            Glass.Fill.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
    }
}
