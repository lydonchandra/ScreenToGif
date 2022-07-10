using ScreenToGif.Util.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ScreenToGif.Controls.Timeline;

/// <summary>
/// A simplified Track without the descrease/increase buttons and with a pre-defined thumb size based on Value/EndValue size.
/// </summary>
public class SimplifiedTrack : Track
{
    public static readonly DependencyProperty EndValueProperty = DependencyProperty.Register(nameof(EndValue), typeof(double), typeof(SimplifiedTrack),
        new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsArrange));

    public double EndValue
    {
        get => (double)GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }

    // Force length of one of track's pieces to be > 0 and less than tracklength
    private static void CoerceLength(ref double componentLength, double trackLength)
    {
        if (componentLength < 0)
            componentLength = 0.0;
        else if (componentLength > trackLength || double.IsNaN(componentLength))
            componentLength = trackLength;
    }

    // Computes the length of the decrease button, thumb and increase button
    // Thumb's size is based on viewport and extent
    // returns false if the track should be hidden
    private bool CalculateThumbSize(Size arrangeSize, bool isVertical, out double leftSpace, out double thumbLength, out double rightSpace)
    {
        var min = Minimum;
        var range = Math.Max(0.0, Maximum - min);
        var offset = Math.Min(range, Value - min);

        double trackLength;
        double thumbMinLength;

        if (isVertical)
        {
            trackLength = arrangeSize.Height;
            thumbMinLength = arrangeSize.Width * 0.5;
        }
        else
        {
            trackLength = arrangeSize.Width;
            thumbMinLength = arrangeSize.Height * 0.5;
        }

        //Get the thumb pixels length based on how much the ViewportSize represents.
        //thumbLength = trackLength * (viewportSize / Maximum);
        thumbLength = trackLength * ((EndValue - offset) / Maximum);
        
        CoerceLength(ref thumbLength, trackLength);

        thumbLength = Math.Max(thumbMinLength, thumbLength);
        
        //If we don't have enough content to scroll, disable the track.
        var notEnoughContentToScroll = range.SmallerThanOrClose(0.0);
        var thumbLongerThanTrack = thumbLength > trackLength;

        // if there's not enough content or the thumb is longer than the track, 
        // hide the track and don't arrange the pieces
        if (notEnoughContentToScroll || thumbLongerThanTrack)
        {
            if (Visibility != Visibility.Hidden)
                Visibility = Visibility.Hidden;

            leftSpace = 0.0;
            rightSpace = 0.0;

            return false;
        }

        if (Visibility != Visibility.Visible)
            Visibility = Visibility.Visible;
        
        //Compute lengths of increase and decrease button
        var remainingTrackLength = trackLength - thumbLength;

        leftSpace = trackLength * (offset / range);
        CoerceLength(ref leftSpace, remainingTrackLength);

        rightSpace = trackLength - thumbLength - leftSpace;
        CoerceLength(ref rightSpace, remainingTrackLength);

        return true;
    }
    
    /// <summary>
    /// Children will be stretched to fit horizontally (if vertically oriented) or vertically (if horizontally 
    /// oriented).
    /// There are essentially three possible layout states:
    /// 1. The track is enabled and the thumb is proportionally sizing.
    /// 2. The track is enabled and the thumb has reached its minimum size. 
    /// 3. The track is disabled or there is not enough room for the thumb. 
    ///    Track elements are not displayed, and will not be arranged.
    /// <seealso cref="FrameworkElement.ArrangeOverride" />
    /// </summary>
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var isVertical = Orientation == Orientation.Vertical;
        
        //Compute the thumb base on the viewport or don't arrange.
        if (Value.NearlyEquals(EndValue) || !CalculateThumbSize(arrangeSize, isVertical, out var decreaseButtonLength, out var thumbLength, out var increaseButtonLength))
            return arrangeSize;
        
        //Layout the pieces of track
        var offset = new Point();
        var pieceSize = arrangeSize;

        if (isVertical)
        {
            CoerceLength(ref thumbLength, arrangeSize.Height);
            
            offset.Y = increaseButtonLength;
            pieceSize.Height = thumbLength;

            Thumb?.Arrange(new Rect(offset, pieceSize));
        }
        else
        {
            CoerceLength(ref thumbLength, arrangeSize.Width);

            offset.X = decreaseButtonLength;
            pieceSize.Width = thumbLength;

            Thumb?.Arrange(new Rect(offset, pieceSize));
        }

        return arrangeSize;
    }

    public override double ValueFromPoint(Point pt)
    {
        var scale = IsDirectionReversed ? -1d : 1d;
        var range = Math.Max(0.0, Maximum - Minimum);

        if (Orientation == Orientation.Horizontal)
            return Math.Max(Minimum, Math.Min(Maximum, scale * (pt.X * range) / ActualWidth + Minimum));

        //Increases in y cause decreases in Sliders value
        return Math.Max(Minimum, Math.Min(Maximum, -1 * scale * pt.Y));
    }

    /// <summary>
    /// This function returns the delta in value that would be caused by moving the thumb the given pixel distances.
    /// The returned delta value is not guaranteed to be inside the valid Value range.
    /// </summary>
    /// <param name="horizontal">Total horizontal distance that the Thumb has moved.</param>
    /// <param name="vertical">Total vertical distance that the Thumb has moved.</param>        
    public override double ValueFromDistance(double horizontal, double vertical)
    {
        var scale = IsDirectionReversed ? -1d : 1d;
        var range = Math.Max(0.0, Maximum - Minimum);

        if (Orientation == Orientation.Horizontal)
            return scale * horizontal * (range / ActualWidth);

        //Increases in Y cause decreases in value.
        return -1 * scale * vertical;
    }
}