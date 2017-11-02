using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Animator
{
    public static class Extensions
    {
        #region Spin

        public static void StartSpinning(this FrameworkElement element, int spin, int speed)
        {
            var anim = new DoubleAnimation
            {
                To = spin,
                Duration = new Duration(TimeSpan.FromMilliseconds(speed)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            element.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, anim);
        }

        public static void EndSpinning(this FrameworkElement element, int spin, int speed)
        {
            var anim = new DoubleAnimation
            {
                To = spin,
                Duration = new Duration(TimeSpan.FromMilliseconds(speed)),
                EasingFunction = new CubicEase()
            };

            element.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, anim);
        }

        public static void Spin(this FrameworkElement element, int spin,
            int speed, bool end = false)
        {
            var anim = new DoubleAnimation
            {
                From = 0,
                To = spin,
                Duration = new Duration(TimeSpan.FromMilliseconds(speed)),
                EasingFunction = end ? new CubicEase() : null
            };

            element.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, anim);
        }

        #endregion

        #region Fade

        public static async Task FadeWait(this FrameworkElement element, bool fadeIn)
        {
            await fade(element, fadeIn, true);
        }

        public static void Fade(this FrameworkElement element, bool fadeIn)
        {
            fade(element, fadeIn, false).Wait();
        }

        static async Task fade(FrameworkElement element, bool fadeIn, bool wait)
        {
            const double duration = 200;

            element.Visibility = Visibility.Visible;

            var anim = new DoubleAnimation()
            {
                From = fadeIn ? 0 : 1,
                To = fadeIn ? 1 : 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                EasingFunction = new CubicEase()
            };

            element.BeginAnimation(FrameworkElement.OpacityProperty, anim);

            if (wait)
                await Task.Factory.StartNew(() => Thread.Sleep((int)duration));

            if (!fadeIn)
                element.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Height

        public static async Task ResizeHeightWait(this FrameworkElement element,
            double oldHeight, double newHeight, double duration)
        {
            await resizeHeight(element, oldHeight, newHeight, duration, true);
        }

        public static void ResizeHeight(this FrameworkElement element,
            double oldHeight, double newHeight, double duration)
        {
            resizeHeight(element, oldHeight, newHeight, duration, false).Wait();
        }

        public static async Task resizeHeight(FrameworkElement element,
            double oldHeight, double newHeight, double duration, bool wait)
        {
            var anim = new DoubleAnimation
            {
                From = oldHeight,
                To = newHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                EasingFunction = new CubicEase()
            };

            element.BeginAnimation(FrameworkElement.HeightProperty, anim);

            if (wait)
                await Task.Factory.StartNew(() => Thread.Sleep((int)duration));
        }

        #endregion

        public static async Task ChangeText(this Button button, string newText)
        {
            while (button.Content.ToString().Length > 0)
            {
                button.Content = button.Content.ToString()
                    .Substring(0, button.Content.ToString().Length - 1);
                await sleep();
            }

            for (int i = 0; i < newText.Length; i++)
            {
                button.Content = button.Content.ToString() + newText[i];
                await sleep();
            }
        }

        static async Task sleep()
        { await Task.Factory.StartNew(() => Thread.Sleep(20)); }
    }
}
