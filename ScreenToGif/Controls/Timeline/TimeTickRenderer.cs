using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace ScreenToGif.Controls.Timeline
{
    public class TimeTickRenderer : FrameworkElement
    {
        private Pen _tickPen;

        #region Properties

        public static readonly DependencyProperty ViewportEndProperty = DependencyProperty.Register(nameof(ViewportEnd), typeof(TimeSpan), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ViewportStartProperty = DependencyProperty.Register(nameof(ViewportStart), typeof(TimeSpan), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(nameof(Current), typeof(TimeSpan), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(nameof(SelectionStart), typeof(TimeSpan?), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(default(TimeSpan?)));
        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(nameof(SelectionEnd), typeof(TimeSpan?), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(default(TimeSpan?)));
        public static readonly DependencyProperty TickBrushProperty = DependencyProperty.Register(nameof(TickBrush), typeof(Brush), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, TickBrush_PropertyChanged));
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(TimeTickRenderer), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public TimeSpan ViewportEnd
        {
            get => (TimeSpan)GetValue(ViewportEndProperty);
            set => SetValue(ViewportEndProperty, value);
        }

        public TimeSpan ViewportStart
        {
            get => (TimeSpan)GetValue(ViewportStartProperty);
            set => SetValue(ViewportStartProperty, value);
        }

        public TimeSpan Current
        {
            get => (TimeSpan)GetValue(CurrentProperty);
            set => SetValue(CurrentProperty, value);
        }

        public TimeSpan? SelectionStart
        {
            get => (TimeSpan)GetValue(SelectionStartProperty);
            set => SetValue(SelectionStartProperty, value);
        }

        public TimeSpan? SelectionEnd
        {
            get => (TimeSpan?)GetValue(SelectionEndProperty);
            set => SetValue(SelectionEndProperty, value);
        }

        public Brush TickBrush
        {
            get => (Brush)GetValue(TickBrushProperty);
            set => SetValue(TickBrushProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        #endregion
        
        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    var pen = new Pen(TickBrush, 1);

        //    //Zoom based on mouse position (Adjust time).

        //    var Zoom = 1;

        //    //Maybe instead of defaulting to 30s, I'll use a timeSpan that equates to 0% zoom on the current display width.
        //    var inView = (ViewportEnd - ViewportStart).TotalMilliseconds;

        //    if (inView < 0)
        //    {
        //        base.OnRender(drawingContext);
        //        return;
        //    }

        //    var perPixel = inView / ActualWidth;

        //    //Based on viewSpan, decide when to show each tick. Should be dynamic.

        //    // < 200ms in view:
        //    //Large tick: Every 1000ms
        //    //Medium tick: Every 500ms
        //    //Small tick: Every 100ms
        //    //Very small tick: Every 10ms
        //    //Timestamp: Every 500ms

        //    //< 15000ms in view:
        //    //Large tick: Every 10.000ms
        //    //Medium tick: Every 5.000ms
        //    //Small tick: Every 1.000ms
        //    //Very small tick: Every 100ms
        //    //Timestamp: Every 5.000ms

        //    //Large
        //    var largeTickFrequency = 1000 / perPixel;
        //    var offset = ViewportStart.TotalMilliseconds % 1000;
        //    var offsetSize = offset / perPixel;

        //    Console.WriteLine($"{Zoom}% • {inView}ms = {ViewportStart} ~ {offset} = {offsetSize}px");

        //    for (var x = offsetSize; x < ActualWidth; x += largeTickFrequency)
        //        drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, 16));

        //    if (perPixel > 9)
        //    {
        //        base.OnRender(drawingContext);
        //        return;
        //    }

        //    //Medium, printed for each 500ms.
        //    var mediumTickFrequency = 500 / perPixel;
        //    offset = ViewportStart.TotalMilliseconds % 500;
        //    offsetSize = offset / perPixel;

        //    for (var x = offsetSize; x < ActualWidth; x += mediumTickFrequency)
        //        drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, 12));

        //    if (perPixel > 5)
        //    {
        //        base.OnRender(drawingContext);
        //        return;
        //    }

        //    //Small, printed for each 100ms
        //    var smallTickFrequency = 100 / perPixel;
        //    offset = ViewportStart.TotalMilliseconds % 100;
        //    offsetSize = offset / perPixel;

        //    for (var x = offsetSize; x < ActualWidth; x += smallTickFrequency)
        //        drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, 8));

        //    if (perPixel > 1.5)
        //    {
        //        base.OnRender(drawingContext);
        //        return;
        //    }

        //    //Very small, printed for each 10ms.
        //    var verySmallTickFrequency = 10 / perPixel;
        //    offset = ViewportStart.TotalMilliseconds % 10;
        //    offsetSize = offset / perPixel;

        //    for (var x = offsetSize; x < ActualWidth; x += verySmallTickFrequency)
        //        drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, 4));

        //    base.OnRender(drawingContext);
        //}

        protected override void OnInitialized(EventArgs e)
        {
            _tickPen = new Pen(TickBrush, 1);

            base.OnInitialized(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //How many milliseconds are in view.
            var inView = (ViewportEnd - ViewportStart).TotalMilliseconds;

            if (inView < 1)
            {
                base.OnRender(drawingContext);
                return;
            }

            //How many milliseconds each pixel represents.
            var perPixel = inView / ActualWidth;

            //Viewport span:
            //1 day:    24x60x60x1000  86.400.000ms
            //1h/24h:   60x60x1000     3.600.000ms
            //1m/60m:   60x1000        60.000ms
            //1s/60s:                  1000ms

            //Tick frequency:
            //Max: 1 per hour.
            //Min: 1 per millisecond.

            //Decide granularity based on milliseconds per pixel (density).
            //10ms = 1 tall each 5ms (0, 5, 10), 1 short each 1ms
            //100ms = 1 tall each 50ms (0, 50, 100), 1 short each 10ms
            //1000ms = 1 tall each 500ms (0, 500, 1000), 1 short each 100ms

            //InView: 695ms
            //Width: 1390px
            //PerPixel: 0.5ms

            //Tall distance: 120px
            //Short distance: 30px

            var tickFrequency = inView / 10d / perPixel;
            var shorTickFrequency = inView / 100d / perPixel;
            var startOffset = ViewportStart.TotalMilliseconds % shorTickFrequency;
            
            System.Diagnostics.Debug.WriteLine($"{perPixel}ms");
            
            for (var x = startOffset; x <= ActualWidth; x += shorTickFrequency)
                drawingContext.DrawLine(_tickPen, new Point(x, 0), new Point(x, 4));

            for (var x = ViewportStart.TotalMilliseconds % tickFrequency; x <= ActualWidth; x += tickFrequency)
            {
                //Draw tick.
                drawingContext.DrawLine(_tickPen, new Point(x, 0), new Point(x, 8));

                //Draw timestamp text.
                var timestamp = ViewportStart.Add(TimeSpan.FromMilliseconds(x));
                var text = timestamp.ToString(timestamp.TotalMilliseconds < 1000 ? @"\:fff" : timestamp.TotalMinutes < 1 ? @"ss\:fff" : timestamp.TotalHours < 1 ? @"mm\:ss" : @"hh\:mm");

                var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 10d, TickBrush, 96d);
                
                if (x + formattedText.MinWidth / 2 > ActualWidth || x - formattedText.MinWidth / 2 < 0)
                    continue;

                drawingContext.DrawText(formattedText, new Point(x - formattedText.MinWidth / 2, 10));
            }
            
            base.OnRender(drawingContext);
            return;

            //Large
            var largeTickFrequency = 1000 / perPixel;
            var offset = ViewportStart.TotalMilliseconds % 1000;
            var offsetSize = offset / perPixel;

            Console.WriteLine($"{inView}ms = {ViewportStart} ~ {offset} = {offsetSize}px");

            for (var x = offsetSize; x < ActualWidth; x += largeTickFrequency)
                drawingContext.DrawLine(_tickPen, new Point(x, 0), new Point(x, 16));

            if (perPixel > 9)
            {
                base.OnRender(drawingContext);
                return;
            }

            //Medium, printed for each 500ms.
            var mediumTickFrequency = 500 / perPixel;
            offset = ViewportStart.TotalMilliseconds % 500;
            offsetSize = offset / perPixel;

            for (var x = offsetSize; x < ActualWidth; x += mediumTickFrequency)
                drawingContext.DrawLine(_tickPen, new Point(x, 0), new Point(x, 12));

            if (perPixel > 5)
            {
                base.OnRender(drawingContext);
                return;
            }

            //Small, printed for each 100ms
            var smallTickFrequency = 100 / perPixel;
            offset = ViewportStart.TotalMilliseconds % 100;
            offsetSize = offset / perPixel;

            for (var x = offsetSize; x < ActualWidth; x += smallTickFrequency)
                drawingContext.DrawLine(_tickPen, new Point(x, 0), new Point(x, 8));

            if (perPixel > 1.5)
            {
                base.OnRender(drawingContext);
                return;
            }

            //Very small, printed for each 10ms.
            var verySmallTickFrequency = 10 / perPixel;
            offset = ViewportStart.TotalMilliseconds % 10;
            offsetSize = offset / perPixel;

            for (var x = offsetSize; x < ActualWidth; x += verySmallTickFrequency)
                drawingContext.DrawLine(_tickPen, new Point(x, 0), new Point(x, 4));

            base.OnRender(drawingContext);
        }

        private static void TickBrush_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimeTickRenderer rendered)
                return;

            rendered._tickPen = new Pen(rendered.TickBrush, 1);
        }
    }
}