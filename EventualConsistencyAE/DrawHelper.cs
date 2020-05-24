using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using EventualConsistencyAE.Web;

// ReSharper disable AccessToStaticMemberViaDerivedType

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
                        StrokeThickness = 1,
                    };

                    Canvas.SetZIndex(line, 1);

                    canvas.Children.Add(line);
                }

                var circle = new Ellipse
                {
                    Width = 50,
                    Height = 50,
                    StrokeThickness = 1,
                    Stroke = Brushes.Black,
                    Fill = Brushes.White
                };
                Canvas.SetTop(circle, -Math.Cos(i * angle) * centerY * 0.9 + centerY - 25);
                Canvas.SetLeft(circle, Math.Sin(i * angle) * centerX * 0.9 + centerX - 25);
                Canvas.SetZIndex(circle, 2);
                canvas.Children.Add(circle);

                var text = new Label
                {
                    Content = server.Port,
                    Width = 50,
                    Height = 50,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                Canvas.SetTop(text, -Math.Cos(i * angle) * centerY * 0.9 + centerY - 25);
                Canvas.SetLeft(text, Math.Sin(i * angle) * centerX * 0.9 + centerX - 25);
                Canvas.SetZIndex(text, 3);
                canvas.Children.Add(text);
            }
        }
    }
}