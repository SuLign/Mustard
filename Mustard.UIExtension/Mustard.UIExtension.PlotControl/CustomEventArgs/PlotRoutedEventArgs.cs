﻿using System.Windows;

namespace Mustard.UIExtension.PlotControl.CustomEventArgs;

public class PlotRoutedEventArgs : RoutedEventArgs
{

    public Point Location { get; set; }
    public PlotRoutedEventArgs(RoutedEvent routedEvent, Point location) : base(routedEvent)
    {
        Location = location;
    }
}
