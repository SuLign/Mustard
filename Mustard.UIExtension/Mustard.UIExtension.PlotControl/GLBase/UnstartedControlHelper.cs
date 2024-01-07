using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Mustard.UIExtension.PlotControl.GLBase;

internal static class UnstartedControlHelper
{
    public static void DrawUnstartedControlHelper(GLControlBase control, DrawingContext drawingContext)
    {
        if (control.Visibility == Visibility.Visible && control.ActualWidth > 0.0 && control.ActualHeight > 0.0)
        {
            double actualWidth = control.ActualWidth;
            double actualHeight = control.ActualHeight;
            drawingContext.DrawRectangle(Brushes.Gray, null, new Rect(0.0, 0.0, actualWidth, actualHeight));
            if (Debugger.IsAttached)
            {
                Typeface typeface = new Typeface("Arial");
                FormattedText formattedText = new FormattedText("OpenGL content. Call Start() on the control to begin rendering.", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 12.0, Brushes.White, 1.25)
                {
                    TextAlignment = TextAlignment.Left,
                    MaxTextWidth = actualWidth
                };
                drawingContext.DrawText(formattedText, new Point(0.0, 0.0));
            }
        }
    }
}
