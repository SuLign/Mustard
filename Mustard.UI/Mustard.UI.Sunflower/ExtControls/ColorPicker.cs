using Mustard.UI.Sunflower.Controls;

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Mustard.Base.Toolset.WinAPI;

namespace Mustard.UI.Sunflower.ExControls;

public sealed class ColorPicker
{
    private Color result;
    private GradientStopCollection gradientStops;
    private bool procRes;
    private ArrayList _threadWindowHandles;
    private IntPtr _dialogOwnerHandle;
    private DispatcherFrame _dispatcherFrame;
    private double primeryScreenHeight;
    private HwndSource sourceHandle;
    private Button okBtn;
    private Button cancelBtn;
    private TextBlock captionTextBlock;
    private static Dispatcher invokeDispatcher;
    private Dispatcher currentDispatcher;
    private double newBrightness = 50;
    private double oldBrightness = 0;
    private double selectedH;
    private double selectedS;
    private double selectedL;
    private Image colorRing;

    public static bool PickColor(out Color v, string title = null, Color? defaultVal = default)
    {
        ColorPicker valueInput = new ColorPicker();
        invokeDispatcher = Application.Current.Dispatcher;

        valueInput.CreateHostVirtual(title ?? "选择颜色", defaultVal ?? Colors.Red);
        v = valueInput.result;
        return valueInput.procRes;
    }

    public static bool PickGradientColor(out GradientStopCollection gradientStops, string title = null, Color? defaultVal = default)
    {
        ColorPicker valueInput = new ColorPicker();
        invokeDispatcher = Application.Current.Dispatcher;

        valueInput.CreateHostVirtual(title ?? "颜色选取器", defaultVal);
        gradientStops = valueInput.gradientStops;
        return valueInput.procRes;
    }

    internal Popup GetPopup(string title, Color? defaultVal)
    {
        var mainBorder = new Border
        {
            Margin = new Thickness(20)
        };
        mainBorder.Effect = new DropShadowEffect
        {
            BlurRadius = 20,
            ShadowDepth = 2,
            Color = Colors.Black,
            RenderingBias = RenderingBias.Quality,
            Direction = -45,
            Opacity = .5,
        };
        var hostBorder = new Border
        {
        };
        mainBorder.Child = hostBorder;
        primeryScreenHeight = SystemParameters.PrimaryScreenHeight;
        hostBorder.SetResourceReference(Border.BackgroundProperty, "Major_Background");
        var popup = new Popup();
        popup.Focusable = true;
        popup.AllowsTransparency = true;
        popup.Child = mainBorder;
        popup.Width = 320;
        popup.Height = 480;
        popup.PreviewMouseDown += (_, _) =>
        {
            SetFocus(sourceHandle.Handle);
            SetForegroundWindow(sourceHandle.Handle);
        };
        var hostGrid = new Grid
        {
            ClipToBounds = true,
        };
        hostGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(26, GridUnitType.Pixel) });
        hostGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
        hostGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        hostGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
        hostBorder.Child = hostGrid;

        var titleBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x01, 0x45, 0x45, 0x45)),
        };
        var titleGrid = new Grid
        {
            Margin = new Thickness(0, 0, 10, 0),
        };
        var titlePanel = new StackPanel() { VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
        titleBorder.Child = titleGrid;
        titleGrid.Children.Add(new Rectangle { Fill = Brushes.Transparent });
        var closeBtn = new Button
        {
            Height = 20,
            Width = 20,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        closeBtn.Content = new Path
        {
            Data = Geometry.Parse("M13.46,12L19,17.54V19H17.54L12,13.46L6.46,19H5V17.54L10.54,12L5,6.46V5H6.46L12,10.54L17.54,5H19V6.46L13.46,12Z"),
            Stretch = Stretch.Uniform,
            Height = 8
        };
        (closeBtn.Content as Path).SetResourceReference(Path.FillProperty, "Major_Foreground");
        closeBtn.Click += (_, _) =>
        {
            popup.IsOpen = false;
            EnableThreadWindows(state: true);
            currentDispatcher.Invoke(() =>
            {
                Dispatcher.ExitAllFrames();
            });
            procRes = false;
        };
        titleGrid.Children.Add(closeBtn);

        titleGrid.Children.Add(titlePanel);
        Geometry pathData = Geometry.Parse("M17.5,12A1.5,1.5 0 0,1 16,10.5A1.5,1.5 0 0,1 17.5,9A1.5,1.5 0 0,1 19,10.5A1.5,1.5 0 0,1 17.5,12M14.5,8A1.5,1.5 0 0,1 13,6.5A1.5,1.5 0 0,1 14.5,5A1.5,1.5 0 0,1 16,6.5A1.5,1.5 0 0,1 14.5,8M9.5,8A1.5,1.5 0 0,1 8,6.5A1.5,1.5 0 0,1 9.5,5A1.5,1.5 0 0,1 11,6.5A1.5,1.5 0 0,1 9.5,8M6.5,12A1.5,1.5 0 0,1 5,10.5A1.5,1.5 0 0,1 6.5,9A1.5,1.5 0 0,1 8,10.5A1.5,1.5 0 0,1 6.5,12M12,3A9,9 0 0,0 3,12A9,9 0 0,0 12,21A1.5,1.5 0 0,0 13.5,19.5C13.5,19.11 13.35,18.76 13.11,18.5C12.88,18.23 12.73,17.88 12.73,17.5A1.5,1.5 0 0,1 14.23,16H16A5,5 0 0,0 21,11C21,6.58 16.97,3 12,3Z");

        Path titleIconImg = new Path
        {
            Stretch = Stretch.Uniform,
            Width = 15,
            Margin = new Thickness(10, 0, 0, 0),
            Height = 15,
            Opacity = 0.8,
            Data = pathData,
            VerticalAlignment = VerticalAlignment.Center,
        };
        titleIconImg.SetResourceReference(Path.FillProperty, "Major_Foreground");
        titlePanel.Children.Add(titleIconImg);
        var titleTextBlock = new TextBlock
        {
            Text = title,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            Margin = new Thickness(5, 0, 15, 0),
        };
        titleTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "Sub1_Foreground");
        titlePanel.Children.Add(titleTextBlock);
        titleTextBlock.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Microsoft YaHei UI"));

        Grid contentGrid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 0)
        };
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(130, GridUnitType.Pixel) });
        var valueEditorStackPanel = new StackPanel
        {
            Margin = new Thickness(0, 10, 0, 5),
        };
        var colorValueTextBlock = new TextBlock
        {
            Height = 20,
            Margin = new Thickness(0, 10, 0, 10),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 15,
            Text = "#000000",
        };
        if (defaultVal != null) colorValueTextBlock.Text = defaultVal.Value.ToString();
        colorValueTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "Sub1_Foreground");
        colorValueTextBlock.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Microsoft YaHei Bold"));
        colorRing = new Image
        {
            Height = 202,
            Width = 202,
        };
        valueEditorStackPanel.Children.Add(colorValueTextBlock);

        var colorRingCanvas = new Canvas
        {
            Height = 202,
            Width = 202,
            Margin = new Thickness(0, 25, 0, 20),
        };
        colorRingCanvas.Children.Add(colorRing);

        var colorRingGrid = new Grid();
        colorRingGrid.Children.Add(new Rectangle { Fill = Brushes.Transparent });
        colorRingGrid.Children.Add(colorRingCanvas);

        valueEditorStackPanel.Children.Add(colorRingGrid);

        Slider brightnessSlider = new Slider
        {
            Maximum = 100,
            Minimum = 0,
            SmallChange = 0.1,
            Value = newBrightness,
            Margin = new Thickness(0, 0, 0, 0),
        };
        valueEditorStackPanel.Children.Add(brightnessSlider);
        brightnessSlider.ValueChanged += (_, _) =>
        {
            newBrightness = brightnessSlider.Value;
        };

        Grid colorPatternGrid = new Grid();
        colorPatternGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        colorPatternGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var colorPartternHolderStackPanel = new StackPanel();
        Grid.SetColumn(colorPartternHolderStackPanel, 0);

        StackPanel colorPatternStackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5, 0, 0, 0) };
        var colorPatternR = new TextBox() { VerticalAlignment = VerticalAlignment.Center, Width = 45, Height = 24, VerticalContentAlignment = VerticalAlignment.Center };
        var colorPatternG = new TextBox() { VerticalAlignment = VerticalAlignment.Center, Width = 45, Height = 24, VerticalContentAlignment = VerticalAlignment.Center };
        var colorPatternB = new TextBox() { VerticalAlignment = VerticalAlignment.Center, Width = 45, Height = 24, VerticalContentAlignment = VerticalAlignment.Center };
        colorPatternStackPanel.Children.Add(new TextBlock { Text = "R", Margin = new Thickness(5, 0, 0, 0), Width = 15, VerticalAlignment = VerticalAlignment.Center, Foreground = colorValueTextBlock.Foreground });
        colorPatternStackPanel.Children.Add(colorPatternR);
        colorPatternStackPanel.Children.Add(new TextBlock { Text = "G", Margin = new Thickness(15, 0, 0, 0), Width = 15, VerticalAlignment = VerticalAlignment.Center, Foreground = colorValueTextBlock.Foreground });
        colorPatternStackPanel.Children.Add(colorPatternG);
        colorPatternStackPanel.Children.Add(new TextBlock { Text = "B", Margin = new Thickness(15, 0, 0, 0), Width = 15, VerticalAlignment = VerticalAlignment.Center, Foreground = colorValueTextBlock.Foreground });
        colorPatternStackPanel.Children.Add(colorPatternB);

        colorPartternHolderStackPanel.Children.Add(colorPatternStackPanel);

        StackPanel colorHSLPatternStackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5, 2, 0, 0) };
        var colorHSLPatternH = new TextBox() { VerticalAlignment = VerticalAlignment.Center, Width = 45, Height = 24, VerticalContentAlignment = VerticalAlignment.Center };
        var colorHSLPatternS = new TextBox() { VerticalAlignment = VerticalAlignment.Center, Width = 45, Height = 24, VerticalContentAlignment = VerticalAlignment.Center };
        var colorHSLPatternL = new TextBox() { VerticalAlignment = VerticalAlignment.Center, Width = 45, Height = 24, VerticalContentAlignment = VerticalAlignment.Center };
        colorHSLPatternStackPanel.Children.Add(new TextBlock { Text = "H", Margin = new Thickness(5, 0, 0, 0), Width = 15, VerticalAlignment = VerticalAlignment.Center, Foreground = colorValueTextBlock.Foreground });
        colorHSLPatternStackPanel.Children.Add(colorHSLPatternH);
        colorHSLPatternStackPanel.Children.Add(new TextBlock { Text = "S", Margin = new Thickness(15, 0, 0, 0), Width = 15, VerticalAlignment = VerticalAlignment.Center, Foreground = colorValueTextBlock.Foreground });
        colorHSLPatternStackPanel.Children.Add(colorHSLPatternS);
        colorHSLPatternStackPanel.Children.Add(new TextBlock { Text = "L", Margin = new Thickness(15, 0, 0, 0), Width = 15, VerticalAlignment = VerticalAlignment.Center, Foreground = colorValueTextBlock.Foreground });
        colorHSLPatternStackPanel.Children.Add(colorHSLPatternL);

        colorPartternHolderStackPanel.Children.Add(colorHSLPatternStackPanel);
        colorPatternGrid.Children.Add(colorPartternHolderStackPanel);

        var colorDisplayBorder = new Border
        {
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5, 0, 5, 0),
        };
        Grid.SetColumn(colorDisplayBorder, 1);
        colorPatternGrid.Children.Add(colorDisplayBorder);

        valueEditorStackPanel.Children.Add(colorPatternGrid);
        contentGrid.Children.Add(valueEditorStackPanel);

        var colorSelectRing = new Ellipse
        {
            Width = 10,
            Height = 10,
            StrokeThickness = 1
        };
        colorRingCanvas.Children.Add(colorSelectRing);

        if (defaultVal != null)
        {
            var hsl = RGBToHSL(defaultVal.Value);
            selectedH = hsl.H;
            selectedS = hsl.S;
            selectedL = newBrightness = hsl.L;

            Canvas.SetLeft(colorSelectRing, Math.Cos(selectedH * Math.PI / 180) * selectedS + 96);
            Canvas.SetTop(colorSelectRing, -Math.Sin(selectedH * Math.PI / 180) * selectedS + 96);

            colorPatternR.Text = defaultVal.Value.R.ToString();
            colorPatternG.Text = defaultVal.Value.G.ToString();
            colorPatternB.Text = defaultVal.Value.B.ToString();

            colorHSLPatternH.Text = ((int)(selectedH + 360) % 360).ToString();
            colorHSLPatternS.Text = ((int)selectedS).ToString();
            colorHSLPatternL.Text = ((int)selectedL).ToString();

            brightnessSlider.Value = selectedL;
        }

        colorRingGrid.MouseDown += (_, _) =>
        {
            var mouseRelatedPosition = Mouse.GetPosition(colorRing);
            var position = mouseRelatedPosition - new Point(colorRing.Width / 2, colorRing.Height / 2);
            var dst = position.Length;
            position.X = -position.X;
            var satuation = Math.Min(dst, 100);
            var angle = 0.0;
            if (position.X > 0)
            {
                angle = 180 + Math.Atan(position.Y / position.X) / Math.PI * 180;
            }
            else
            {
                angle = Math.Atan(position.Y / position.X) / Math.PI * 180;
            }
            var brightness = newBrightness;
            var color = HSLToRGB((float)angle, (float)satuation, (float)brightness);
            selectedH = angle;
            selectedS = satuation;
            selectedL = brightness;
            colorSelectRing.Stroke = new SolidColorBrush(HSLToRGB((float)((selectedH + 180) % 360), 100, 50));
            Canvas.SetLeft(colorSelectRing, Math.Cos(Math.PI * angle / 180) * satuation + 96);
            Canvas.SetTop(colorSelectRing, -Math.Sin(Math.PI * angle / 180) * satuation + 96);
            colorDisplayBorder.Background = new SolidColorBrush(color);
            colorDisplayBorder.BorderBrush = new SolidColorBrush(HSLToRGB((float)((selectedH + 180) % 360), 100, 50));
            colorValueTextBlock.Text = color.ToString();

            colorPatternR.Text = color.R.ToString();
            colorPatternG.Text = color.G.ToString();
            colorPatternB.Text = color.B.ToString();

            colorHSLPatternH.Text = ((int)(angle + 360) % 360).ToString();
            colorHSLPatternS.Text = ((int)satuation).ToString();
            colorHSLPatternL.Text = ((int)brightness).ToString();

            result = color;
        };
        colorRingGrid.MouseMove += (_, _) =>
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed) return;
            var mouseRelatedPosition = Mouse.GetPosition(colorRing);
            var position = Mouse.GetPosition(colorRing) - new Point(colorRing.Width / 2, colorRing.Height / 2);
            var dst = position.Length;
            position.X = -position.X;
            var satuation = Math.Min(dst, 100);
            var angle = 0.0;
            if (position.X > 0)
            {
                angle = 180 + Math.Atan(position.Y / position.X) / Math.PI * 180;
            }
            else
            {
                angle = Math.Atan(position.Y / position.X) / Math.PI * 180;
            }
            var brightness = newBrightness;
            var color = HSLToRGB((float)angle, (float)satuation, (float)brightness);
            selectedH = angle;
            selectedS = satuation;
            selectedL = brightness;
            colorSelectRing.Stroke = new SolidColorBrush(HSLToRGB((float)((selectedH + 180) % 360), 100, 50));
            Canvas.SetLeft(colorSelectRing, Math.Cos(Math.PI * angle / 180) * satuation + 96);
            Canvas.SetTop(colorSelectRing, -Math.Sin(Math.PI * angle / 180) * satuation + 96);
            colorDisplayBorder.Background = new SolidColorBrush(color);
            colorDisplayBorder.BorderBrush = new SolidColorBrush(HSLToRGB((float)((selectedH + 180) % 360), 100, 50));
            colorValueTextBlock.Text = color.ToString();

            colorPatternR.Text = color.R.ToString();
            colorPatternG.Text = color.G.ToString();
            colorPatternB.Text = color.B.ToString();

            colorHSLPatternH.Text = ((int)(angle + 360) % 360).ToString();
            colorHSLPatternS.Text = ((int)satuation).ToString();
            colorHSLPatternL.Text = ((int)brightness).ToString();

            result = color;
        };

        CompositionTarget.Rendering += (_, _) =>
        {
            if (newBrightness != oldBrightness)
            {
                oldBrightness = newBrightness;
                selectedL = newBrightness;
                colorSelectRing.Stroke = new SolidColorBrush(HSLToRGB((float)((selectedH + 180) % 360), 100, 50));
                var selColor = HSLToRGB((float)selectedH, (float)selectedS, (float)selectedL);
                colorValueTextBlock.Text = selColor.ToString();

                colorPatternR.Text = selColor.R.ToString();
                colorPatternG.Text = selColor.G.ToString();
                colorPatternB.Text = selColor.B.ToString();

                colorHSLPatternH.Text = ((int)(selectedH + 360) % 360).ToString();
                colorHSLPatternS.Text = ((int)selectedS).ToString();
                colorHSLPatternL.Text = ((int)selectedL).ToString();
                colorDisplayBorder.Background = new SolidColorBrush(selColor);
                colorDisplayBorder.BorderBrush = new SolidColorBrush(HSLToRGB((float)((selectedH + 180) % 360), 100, 50));

                result = selColor;
                double ringWidth = 200;
                double ringHeight = 200;
                int side = 2;
                WriteableBitmap writeableBitmap = new WriteableBitmap((int)ringWidth + side, (int)ringHeight + side, 96, 96, PixelFormats.Bgra32, null);
                var pixels = new int[(int)((ringWidth + side) * (ringHeight + side))];
                var radius = Math.Min(ringWidth, ringHeight) / 2;
                for (int i = 0; i < ringHeight + side; i++)
                {
                    for (int j = 0; j < ringWidth + side; j++)
                    {
                        var dst = Math.Sqrt((i - (ringHeight + side) / 2) * (i - (ringHeight + side) / 2) + (j - (ringWidth + side) / 2) * (j - (ringWidth + side) / 2)) - radius;
                        if (dst < 1)
                        {
                            double angle = 0;
                            if (((ringWidth + side) / 2 - j) > 0)
                            {
                                angle = 180 + Math.Atan(((ringHeight + side) / 2 - i) / (j - (ringWidth + side) / 2)) / Math.PI * 180;
                            }
                            else
                            {
                                angle = Math.Atan(((ringHeight + side) / 2 - i) / (j - (ringWidth + side) / 2)) / Math.PI * 180;
                            }
                            var satu = Math.Sqrt((i - (ringHeight + side) / 2) * (i - (ringHeight + side) / 2) + (j - (ringWidth + side) / 2) * (j - (ringWidth + side) / 2)) / radius;
                            if (dst >= 0) satu = 1;
                            var brightness = newBrightness;
                            var color = HSLToRGB((float)angle, (float)satu * 100, (float)brightness);
                            unsafe
                            {
                                fixed (int* pixelsByte = pixels)
                                {
                                    ((byte*)pixelsByte)[(int)((i * (ringWidth + side) + j) * 4 + 0)] = color.B;
                                    ((byte*)pixelsByte)[(int)((i * (ringWidth + side) + j) * 4 + 1)] = color.G;
                                    ((byte*)pixelsByte)[(int)((i * (ringWidth + side) + j) * 4 + 2)] = color.R;
                                    if (dst >= 0)
                                    {
                                        ((byte*)pixelsByte)[(int)((i * (ringWidth + side) + j) * 4 + 3)] = (byte)Math.Ceiling(255 * (1 - dst));
                                    }
                                    else ((byte*)pixelsByte)[(int)((i * (ringWidth + side) + j) * 4 + 3)] = 255;
                                }
                            }
                        }
                        else
                        {
                            unsafe
                            {
                                fixed (int* pixelsByte = pixels)
                                {
                                    ((byte*)pixelsByte)[(int)((i * (ringWidth + side) + j) * 4 + 0)] = 0;
                                    ((byte*)pixelsByte)[(int)((i * (ringWidth + side) + j) * 4 + 1)] = 0;
                                    ((byte*)pixelsByte)[(int)((i * (ringWidth + side) + j) * 4 + 2)] = 0;
                                    ((byte*)pixelsByte)[(int)((i * (ringWidth + side) + j) * 4 + 3)] = 0;
                                }
                            }
                        }
                    }
                }
                writeableBitmap.WritePixels(new Int32Rect(0, 0, (int)(ringWidth + side), (int)(ringHeight + side)), pixels, (int)(ringWidth + side) * 4, 0);
                colorRing.Source = writeableBitmap;
            }
        };
        StackPanel btnStackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 0)
        };

        var okBtnIcon = new Path
        {
            Data = Geometry.Parse("M21,7L9,19L3.5,13.5L4.91,12.09L9,16.17L19.59,5.59L21,7Z"),
        };
        okBtn = new Button
        {
            Width = 80,
            Height = 25,
            Margin = new Thickness(0, 0, 5, 0),
            Content = "确定",
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
        };
        okBtn.SetValue(ButtonStyle.ButtonIconProperty, Geometry.Parse("M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z"));
        //okBtn.SetValue(ButtonStyle.ButtonIconMarginProperty, new Thickness(0, 4, 5, 4));

        cancelBtn = new Button
        {
            Width = 80,
            Height = 25,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Margin = new Thickness(0, 0, 5, 0),
            Content = "取消"
        };
        cancelBtn.SetValue(ButtonStyle.ButtonIconProperty, Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z"));
        //cancelBtn.SetValue(ButtonStyle.ButtonIconMarginProperty, new Thickness(0, 4, 5, 4));

        btnStackPanel.Children.Add(okBtn);
        okBtn.IsDefault = true;
        okBtn.Focus();
        btnStackPanel.Children.Add(cancelBtn);

        okBtn.Click += (_, _) =>
        {
            popup.IsOpen = false;
            EnableThreadWindows(state: true);
            currentDispatcher.Invoke(() =>
            {
                Dispatcher.ExitAllFrames();
            });
            procRes = true;
        };
        cancelBtn.Click += (_, _) =>
        {
            result = default;
            popup.IsOpen = false;
            EnableThreadWindows(state: true);
            currentDispatcher.Invoke(() =>
            {
                Dispatcher.ExitAllFrames();
            });
            procRes = false;
        };

        Grid.SetRow(titleBorder, 0);
        Grid.SetRow(contentGrid, 2);
        Grid.SetRow(btnStackPanel, 3);

        Path msgBoxImg = new Path
        {
            Stretch = Stretch.Uniform,
            Width = 250,
            Height = 250,
            Opacity = 0.2,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(300, -50, 0, 0),
        };
        msgBoxImg.SetResourceReference(Path.FillProperty, "Major_Foreground");

        Grid.SetRowSpan(msgBoxImg, 10);

        hostGrid.Children.Add(msgBoxImg);
        hostGrid.Children.Add(contentGrid);
        hostGrid.Children.Add(btnStackPanel);
        hostGrid.Children.Add(titleBorder);

        popup.PlacementRectangle = new System.Windows.Rect(
            new Point((SystemParameters.PrimaryScreenWidth - popup.Width) / 2, SystemParameters.PrimaryScreenHeight / 2 - popup.Height * 1.5),
            new Size(popup.Width, popup.Height));
        popup.IsOpen = true;
        sourceHandle = (HwndSource)PresentationSource.FromVisual(popup.Child);
        SetFocus(sourceHandle.Handle);
        SetForegroundWindow(sourceHandle.Handle);
        var oldPos = new Point(popup.PlacementRectangle.X, popup.PlacementRectangle.Y);
        var cursorRelatedPos = new Point();
        bool isCaptured = false;
        titleBorder.MouseMove += (_, _) =>
        {
            while ((GetAsyncKeyState(VirtualKeyStates.VK_LBUTTON) & 0x8000) != 0)
            {
                GetCursorPos(out var pos);
                if (!isCaptured)
                {
                    oldPos = new Point(popup.PlacementRectangle.X, popup.PlacementRectangle.Y);
                    cursorRelatedPos = new Point(pos.X - oldPos.X, pos.Y - oldPos.Y);
                    isCaptured = true;
                }
                var pt = new Point(pos.X - cursorRelatedPos.X, pos.Y - cursorRelatedPos.Y);
                if (pt.Y + popup.Height * 2 >= primeryScreenHeight)
                {
                    pt.Y = (int)(primeryScreenHeight - popup.Height * 2);
                }
                popup.PlacementRectangle = new Rect(
                    pt,
                    new Size(popup.Width, popup.Height));
            }
            isCaptured = false;
            oldPos = new Point(popup.PlacementRectangle.X, popup.PlacementRectangle.Y);
        };

        return popup;
    }

    internal string Title { get; set; }
    internal string Caption { get; set; }
    internal object Content { get; set; }

    internal void CreateHostVirtual(string title, Color? defaultVal)
    {
        if (defaultVal != null) result = (Color)defaultVal;
        _threadWindowHandles = new ArrayList();
        _dialogOwnerHandle = GetActiveWindow();
        if (_dialogOwnerHandle != IntPtr.Zero && _dialogOwnerHandle == GetDesktopWindow())
        {
            _dialogOwnerHandle = IntPtr.Zero;
        }

        if (_dialogOwnerHandle != IntPtr.Zero)
        {
            int num = 0;
            while (_dialogOwnerHandle != IntPtr.Zero)
            {
                num = WGetWindowLong(new HandleRef(this, _dialogOwnerHandle), -16);
                if ((num & 0x40000000) != 1073741824)
                {
                    break;
                }
                _dialogOwnerHandle = GetParent(new HandleRef(null, _dialogOwnerHandle));
            }
        }
        EnumThreadWindows(GetCurrentThreadId(), ThreadWindowsCallback, new HandleRef(null, IntPtr.Zero));
        EnableThreadWindows(false);
        var capture = GetCapture();
        if (capture != IntPtr.Zero)
        {
            IntReleaseCapture();
        }
        var thread = new Thread(() =>
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
            invokeDispatcher.Invoke(() =>
            {
                GetPopup(title, defaultVal);
            });
            Dispatcher.Run();
            _dispatcherFrame.Continue = false;
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ComponentDispatcher.PushModal();
        _dispatcherFrame = new DispatcherFrame();
        Dispatcher.PushFrame(_dispatcherFrame);
        ComponentDispatcher.PopModal();
        if (_dialogOwnerHandle != IntPtr.Zero)
        {
            SetFocus(_dialogOwnerHandle);
            SetForegroundWindow(_dialogOwnerHandle);
        }
    }

    internal static int WGetWindowLong(HandleRef hWnd, int nIndex)
    {
        int num = 0;
        IntPtr zero = IntPtr.Zero;
        int num2 = 0;
        if (IntPtr.Size == 4)
        {
            num = GetWindowLong(hWnd, nIndex);
            num2 = Marshal.GetLastWin32Error();
            zero = new IntPtr(num);
        }
        else
        {
            zero = GetWindowLongPtr(hWnd, nIndex);
            num2 = Marshal.GetLastWin32Error();
            num = (int)(zero);
        }

        if (zero == IntPtr.Zero)
        {
        }
        return num;
    }

    internal ColorPicker()
    {
    }

    private void EnableThreadWindows(bool state)
    {
        for (int i = 0; i < _threadWindowHandles.Count; i++)
        {
            IntPtr handle = (IntPtr)_threadWindowHandles[i];
            if (IsWindow(new HandleRef(null, handle)))
            {
                EnableWindow(new HandleRef(null, handle), state);
            }
        }

        if (state)
        {
            _threadWindowHandles = null;
        }
    }

    internal static IntPtr WGetParent(HandleRef hWnd)
    {
        IntPtr parent = GetParent(hWnd);
        int lastWin32Error = Marshal.GetLastWin32Error();
        if (parent == IntPtr.Zero && lastWin32Error != 0)
        {
            throw new Win32Exception(lastWin32Error);
        }
        return parent;
    }

    internal bool ThreadWindowsCallback(IntPtr hWnd, IntPtr lparam)
    {
        if (IsWindowVisible(new HandleRef(null, hWnd)) && IsWindowEnabled(new HandleRef(null, hWnd)))
        {
            _threadWindowHandles.Add(hWnd);
        }

        return true;
    }

    /// <summary>
    /// HSL色彩转换为RGB
    /// </summary>
    /// <param name="h">色相（Hue）（0 - 360）</param>
    /// <param name="s">饱和度（Saturation） 0 - 100</param>
    /// <param name="l">亮度（Lightness）0 - 100</param>
    /// <returns>RGB色彩</returns>
    public static Color HSLToRGB(float h, float s, float l)
    {
        l /= 100;
        s /= 100;

        byte r, g, b;
        if (s == 0)
        {
            r = g = b = (byte)(l * 255);
        }
        else
        {
            float v1, v2;
            float hue = h / 360;
            v2 = (l < 0.5) ? l * (1 + s) : ((l + s) - (l * s));
            v1 = 2 * l - v2;
            r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
            g = (byte)(255 * HueToRGB(v1, v2, hue));
            b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
        }
        return Color.FromRgb(r, g, b);

        float HueToRGB(float v1, float v2, float vH)
        {
            if (vH < 0) vH += 1;
            if (vH > 1) vH -= 1;
            if ((6 * vH) < 1) return (v1 + (v2 - v1) * 6 * vH);
            if ((2 * vH) < 1) return v2;
            if ((3 * vH) < 2) return (v1 + (v2 - v1) * ((2.0f / 3.0f) - vH) * 6);
            return v1;
        }
    }

    public (double H, double S, double L) RGBToHSL(Color color)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));

        var lightness = (max + min) / 2;
        var saturation = 0d;
        if (max != min)
        {
            saturation = lightness < 0.5 ? (max - min) / (max + min) : (max - min) / (2.0 - max - min);
        }
        var h = CalcHue(r, g, b, max, min);
        return new(h, saturation * 100, lightness * 100);

        double CalcHue(double r, double g, double b, double max, double min)
        {
            double hue = 0;
            if (max != min)
            {
                double delta = max - min;
                if (max == r)
                {
                    hue = (g - b) / delta + (g < b ? 6 : 0);
                }
                else if (max == g)
                {
                    hue = (b - r) / delta + 2;
                }
                else if (max == b)
                {
                    hue = (r - b) / delta + 4;
                }
                hue *= 60;
            }
            return hue;
        }
    }
}
