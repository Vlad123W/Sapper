using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace saper1
{
    class Animation : IAnimateble
    {
        private double from;

        public double From
        {
            get => from;
            set => from = value;
        }

        private double to;

        public double To
        {
            get => to;
            set => to = value;
        }

        private TimeSpan duration;

        public TimeSpan Duration
        {
            get => duration;
            set => duration = value;
        }

        private object? objforanim;

        public object ObjForAnim
        {
            get => objforanim!;
            set => objforanim = value;
        }

        public void HorizontalAnimation()
        {
            DoubleAnimation? animation = new DoubleAnimation();
            animation = new DoubleAnimation();
            animation.From = From;
            animation.To = To;
            animation.Duration = Duration;
            animation.BeginAnimation(Border.WidthProperty, animation);
        }

        public void VerticalAnimation()
        {
            throw new NotImplementedException();
        }
    }
}
