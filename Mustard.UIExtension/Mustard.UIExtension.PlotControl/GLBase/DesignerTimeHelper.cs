using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Mustard.UIExtension.PlotControl.GLBase;

internal static class DesignTimeHelper
{
    public static void DrawDesignTimeHelper(GLControlBase control, DrawingContext drawingContext)
    {
        if (control.Visibility == Visibility.Visible && control.ActualWidth > 0.0 && control.ActualHeight > 0.0)
        {
            double actualWidth = control.ActualWidth;
            double actualHeight = control.ActualHeight;
            double emSize = 1.5 * Math.Min(actualWidth, actualHeight) / "Antenna Layout Control".Length;
            Typeface typeface = new Typeface("Arial");
            FormattedText formattedText = new("GraphicsBoarderBase", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, emSize, Brushes.White, 1.25)
            {
                TextAlignment = TextAlignment.Center
            };
            Pen pen = new Pen(Brushes.DarkBlue, 2.0);
            drawingContext.DrawRectangle(rectangle: new Rect(1.0, 1.0, actualWidth - 1.0, actualHeight - 1.0), brush: Brushes.Black, pen: pen);
            drawingContext.DrawLine(new Pen(Brushes.DarkBlue, 2.0), new Point(0.0, 0.0), new Point(control.ActualWidth, control.ActualHeight));
            drawingContext.DrawLine(new Pen(Brushes.DarkBlue, 2.0), new Point(control.ActualWidth, 0.0), new Point(0.0, control.ActualHeight));
            drawingContext.DrawText(formattedText, new Point(actualWidth / 2.0, (actualHeight - formattedText.Height) / 2.0));
        }
    }
}
