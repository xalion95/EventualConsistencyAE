using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using EventualConsistencyAE.Web;
using Service.Model;

namespace EventualConsistencyAE
{
    public static class DrawHelper
    {
        public static void DrawConnectionMap(Canvas canvas, List<Server> servers)
        {
            var centerX = canvas.ActualWidth * 0.5;
            var centerY = canvas.ActualHeight * 0.5;

            var angle = 2.0 * Math.PI / servers.Count;

            canvas.Children.Clear();

            for (var i = 0; i < servers.Count; i++)
            {
                var server = servers[i];

                var text = new Label {Content = server.Port};
                Canvas.SetTop(text, -Math.Cos(i * angle) * centerY * 0.9 + centerY);
                Canvas.SetLeft(text, Math.Sin(i * angle) * centerX * 0.9 + centerX);
                canvas.Children.Add(text);

                var circle = new Ellipse
                {
                    Width = 50,
                    Height = 50,
                    StrokeThickness = 1,
                    Stroke = Brushes.Black,
                    Fill = Brushes.Transparent
                };
                Canvas.SetTop(circle, -Math.Cos(i * angle) * centerY * 0.9 + centerY);
                Canvas.SetLeft(circle, Math.Sin(i * angle) * centerX * 0.9 + centerX);
                canvas.Children.Add(circle);

                for (var j = 0; j < servers.Count; j++)
                {
                    if (i == j || servers[j].Service.Clients.All(client => client.Port != server.Port)) continue;

                    var line = new Line
                    {
                        X1 = Math.Sin(i * angle) * centerX * 0.9 + centerX,
                        Y1 = -Math.Cos(i * angle) * centerY * 0.9 + centerY,
                        X2 = Math.Sin(j * angle) * centerX * 0.9 + centerX,
                        Y2 = -Math.Cos(j * angle) * centerY * 0.9 + centerY,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };

                    canvas.Children.Add(line);
                }
            }
        }

        private static Size GetTextSize(Label label)
        {
            var formattedText = new FormattedText((string) label.Content,
                CultureInfo.CurrentCulture,
                label.FlowDirection,
                new Typeface(label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch),
                label.FontSize,
                Brushes.Black,
                1);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}