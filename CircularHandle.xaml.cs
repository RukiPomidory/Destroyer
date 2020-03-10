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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Destroyer
{
    /// <summary>
    /// Логика взаимодействия для CircularHandle.xaml
    /// </summary>
    public partial class CircularHandle : UserControl
    {
        public static DependencyProperty AngleProperty;

        public event Action<double> AngleChanged = (a) => { };
        public event Action MaximumReached = () => { };
        public event Action MinimumReached = () => { };
        public event Action DragCompleted = () => { };
        public event Action DragStarted = () => { };

        public double MaxAngle { get; set; }
        public double MinAngle { get; set; }

        public double AngleForProperty
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public double Angle
        {
            get
            {
                return totalAngle;
            }

            set
            {
                if (value > MaxAngle)
                {
                    value = MaxAngle;
                    AngleSpeed = 0;
                    MaximumReached();
                }
                else if (value < MinAngle)
                {
                    value = MinAngle;
                    AngleSpeed = 0;
                    MinimumReached();
                }

                SetAngle(value);
                totalAngle = value;
                AngleChanged(value);
            }
        }

        public double HandleRadius
        {
            get
            {
                return CircleThumb.Width / 2;
            }

            set
            {
                CircleThumb.Width = value * 2;
                CircleThumb.Height = value * 2;

                SetAngle(totalAngle);
            }
        }

        public double StickThickness
        {
            get
            {
                return Stick.Height;
            }

            set
            {
                Stick.Height = value;
                Canvas.SetTop(Stick, yCenter - Stick.Height / 2);
            }
        }

        public double StickLength
        {
            get
            {
                return radius;
            }

            set
            {
                radius = value;
                Stick.Width = value;
                SetAngle(totalAngle);
            }
        }

        public double AngleSpeed { get; set; }

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
        double totalAngle = 0;
        double lastAngleChangedTime = millis();

        static CircularHandle()
        {
            AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(CircularHandle));
        }

        public CircularHandle()
        {
            InitializeComponent();

            MaxAngle = Double.NaN;
            MinAngle = Double.NaN;

            xCenter = Canvas.GetLeft(CenterOfRotate) + CenterOfRotate.Width / 2;
            yCenter = Canvas.GetTop(CenterOfRotate) + CenterOfRotate.Height / 2;
            radius = Stick.Width;

            Canvas.SetLeft(Stick, xCenter);
            Canvas.SetTop(Stick, yCenter - Stick.Height / 2);

            SetAngle(0);
            totalAngle = GetAngle();
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

            double newThumbX = xCenter + cos * radius;
            double newThumbY = yCenter + sin * radius;

            double angle = GetAngle(newThumbX, newThumbY);
            UpdateAngle(angle);

            if (!Double.IsNaN(MinAngle) && this.totalAngle < MinAngle)
            {
                if (GetAngle() != MinAngle)
                {
                    SetAngle(MinAngle);
                }

                MinimumReached();

                return;
            }
            else if (!Double.IsNaN(MaxAngle) && this.totalAngle > MaxAngle)
            {
                if (GetAngle() != MaxAngle)
                {
                    SetAngle(MaxAngle);
                }

                MaximumReached();

                return;
            }

            SetThumbPosition(newThumbX, newThumbY);

            RotateTransform rotateTransform = new RotateTransform(angle / Math.PI * 180);
            Stick.RenderTransform = rotateTransform;

            AngleChanged(totalAngle);
        }

        private void Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (totalAngle < MinAngle)
            {
                totalAngle = MinAngle;
            }
            else if (totalAngle > MaxAngle)
            {
                totalAngle = MaxAngle;
            }

            DragCompleted();
        }

        private void UpdateAngle(double newAngle)
        {
            if (newAngle < 0 || newAngle > Math.PI * 2)
            {
                return;
            }

            double RotationCount = totalAngle / (Math.PI * 2);
            int count = (int)RotationCount;
            double localAngle = totalAngle - count * Math.PI * 2;

            if (localAngle < 0)
            {
                localAngle += Math.PI * 2;
            }

            double change = newAngle - localAngle;

            if (change > Math.PI)
            {
                change -= Math.PI * 2;

                //change = (change % (Math.PI * 2));
            }
            else if (change < -Math.PI)
            {
                change += Math.PI * 2;
            }

            if (change > 0)
            {

            }

            double alpha = 0.01;

            var timeDelta = (millis() - lastAngleChangedTime) / 1000;
            AngleSpeed = (change / timeDelta) * alpha + AngleSpeed * (1 - alpha);
            totalAngle += change;
            lastAngleChangedTime = millis();
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

        public static double millis()
        {
            return TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMilliseconds;
        }

        private void CircleThumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            AngleSpeed = 0;
            DragStarted();
        }
    }
}
