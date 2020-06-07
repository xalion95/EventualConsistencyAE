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
            var centerY = canvas.ActualHeight * 0.5 - 10;

            var angle = 2.0 * Math.PI / servers.Count;

            canvas.Children.Clear();

            for (var i = 0; i < servers.Count; i++)
            {
                var server = servers[i];

                for (var j = 0; j < servers.Count; j++)
                {
                    if (i == j || servers[j].Service.Clients.All(client => client.Port != server.Port)) continue;

                    var v1 = new Vector(Math.Sin(i * angle) * centerX * 0.9 + centerX,
                        -Math.Cos(i * angle) * centerY * 0.9 + centerY);
                    var v2 = new Vector(Math.Sin(j * angle) * centerX * 0.9 + centerX,
                        -Math.Cos(j * angle) * centerY * 0.9 + centerY);

                    var center = new Vector(centerX, centerY);
                    var a = GetAngleBetweenVectors(v1 - center, v2 - center);

                    var curve = GetBezier(VectorToPoint(v1), GetBezierPoint(v1, v2, a), VectorToPoint(v2), GetGradient(i, j));

                    Canvas.SetZIndex(curve, 1);

                    canvas.Children.Add(curve);
                }

                var circle = new Ellipse
                {
                    Width = 60,
                    Height = 60,
                    StrokeThickness = 1,
                    Stroke = Brushes.Black,
                    Fill = server.IsRunning ? Brushes.Green : Brushes.Red
                };

                Canvas.SetTop(circle, -Math.Cos(i * angle) * centerY * 0.9 + centerY - 25);
                Canvas.SetLeft(circle, Math.Sin(i * angle) * centerX * 0.9 + centerX - 25);
                Canvas.SetZIndex(circle, 2);
                canvas.Children.Add(circle);

                var text = new Label
                {
                    Content = server.Port,
                    Foreground = Brushes.White,
                    FontSize = 16,
                    Width = 60,
                    Height = 60,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                Canvas.SetTop(text, -Math.Cos(i * angle) * centerY * 0.9 + centerY - 25);
                Canvas.SetLeft(text, Math.Sin(i * angle) * centerX * 0.9 + centerX - 25);
                Canvas.SetZIndex(text, 3);
                canvas.Children.Add(text);
            }
        }

        private static Path GetBezier(Point p1, Point p2, Point p3, LinearGradientBrush gradientBrush)
        {
            var curve = new QuadraticBezierSegment(p2, p3, true);
            var path = new PathGeometry();
            var pathFigure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = false
            };
            pathFigure.Segments.Add(curve);
            path.Figures.Add(pathFigure);

            return new Path
            {
                Stroke = gradientBrush,
                StrokeThickness = 1.5,
                Data = path
            };
        }

        private static double GetAngleBetweenVectors(Vector v1, Vector v2)
        {
            return Math.Atan2(v2.Y - v1.Y, v2.X - v1.X);
        }

        private static Point GetBezierPoint(Vector p1, Vector p2, double angle)
        {
            var center = (p1 + p2) * 0.5;

            return new Point(center.X + 50.0 * Math.Sin(angle), center.Y + 50.0 * Math.Cos(angle));
        }

        private static Point VectorToPoint(Vector v)
        {
            return new Point(v.X, v.Y);
        }

        private static LinearGradientBrush GetGradient(int i, int j)
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush();

            if (j <= i)
            {
                linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
                linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Blue, 1.0));
            }
            else
            {
                linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0.0));
                linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
            }

            return linearGradientBrush;
        }
    }
}