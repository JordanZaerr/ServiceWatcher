using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ServiceWatcher
{
    [TemplatePart(Name = ElementCanvas, Type = typeof(Canvas))]
    public class ProgressIndicator : RangeBase
    {
        private const string ElementCanvas = "PART_Canvas";
        public static readonly DependencyProperty ElementStoryboardProperty = DependencyProperty
            .Register("ElementStoryboard", typeof(Storyboard), typeof(ProgressIndicator));
        
        public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty
            .Register("IsIndeterminate", typeof(bool), typeof(ProgressIndicator), new PropertyMetadata(true));

        public static readonly DependencyProperty IsRunningProperty = DependencyProperty.
            Register("IsRunning",typeof(bool),typeof(ProgressIndicator),
            new FrameworkPropertyMetadata(IsRunningPropertyChanged));

        private readonly DispatcherTimer updateDisplayTimer;
        private Canvas canvas;
        private Array canvasElements;
        private int index;

        static ProgressIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ProgressIndicator),
                new FrameworkPropertyMetadata(typeof(ProgressIndicator)));
            MaximumProperty.OverrideMetadata(
                typeof(ProgressIndicator), new FrameworkPropertyMetadata(100.0));
        }

        public ProgressIndicator()
        {
            updateDisplayTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
            updateDisplayTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            updateDisplayTimer.Tick += OnUpdateDisplayTimerElapsed;
        }

        public Storyboard ElementStoryboard
        {
            get { return (Storyboard)GetValue(ElementStoryboardProperty); }
            set { SetValue(ElementStoryboardProperty, value); }
        }

        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            canvas = GetTemplateChild(ElementCanvas) as Canvas;
            if (canvas != null)
            {
                // Get the center of the canvas. This will be the base of the rotation.
                double centerX = canvas.Width / 2;
                double centerY = canvas.Height / 2;

                // Get the no. of degrees between each circles.
                double interval = 360.0 / canvas.Children.Count;
                double angle = -135;

                canvasElements = Array.CreateInstance(
                    typeof(UIElement), canvas.Children.Count);
                canvas.Children.CopyTo(canvasElements, 0);
                canvas.Children.Clear();

                foreach (UIElement element in canvasElements)
                {
                    var contentControl = new ContentControl();
                    contentControl.Content = element;

                    var rotateTransform = new RotateTransform(angle, centerX, centerY);
                    contentControl.RenderTransform = rotateTransform;
                    angle += interval;

                    canvas.Children.Add(contentControl);
                }
            }
        }

        private static void IsRunningPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressIndicator = (ProgressIndicator)d;

            if ((bool)e.NewValue)
            {
                progressIndicator.Start();
            }
            else
            {
                progressIndicator.Stop();
            }
        }

        private void OnUpdateDisplayTimerElapsed(object sender, EventArgs e)
        {
            if (canvasElements != null && ElementStoryboard != null)
            {
                var element = canvasElements.GetValue(index) as FrameworkElement;
                StartStoryboard(element);
                index = (index + 1) % canvasElements.Length;
            }
        }

        private void Start()
        {
            updateDisplayTimer.Start();
        }

        private void StartStoryboard(FrameworkElement element)
        {
            NameScope.SetNameScope(this, new NameScope());
            element.Name = "Element";

            NameScope.SetNameScope(element, NameScope.GetNameScope(this));
            NameScope.GetNameScope(this).RegisterName(element.Name, element);

            var storyboard = new Storyboard();
            NameScope.SetNameScope(storyboard, NameScope.GetNameScope(this));

            foreach (Timeline timeline in ElementStoryboard.Children)
            {
                Timeline timelineClone = timeline.Clone();
                storyboard.Children.Add(timelineClone);
                Storyboard.SetTargetName(timelineClone, element.Name);
            }

            storyboard.Begin(element);
        }

        private void Stop()
        {
            updateDisplayTimer.Stop();
            updateDisplayTimer.Tick -= OnUpdateDisplayTimerElapsed;
        }
    }
}