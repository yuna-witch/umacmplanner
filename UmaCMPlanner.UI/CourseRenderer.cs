using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using UmaCMPlanner.BusinessLogic;
using UmaCMPlanner.BusinessLogic.Enums;
using Brushes = System.Windows.Media.Brushes;

namespace UmaCMPlanner.UI;

public class CourseRenderer
{
    private const double RowHeight = 100;
    private const double TrackY = RowHeight * 5;
    private readonly Canvas canvas;

    public double Width => canvas.ActualWidth;
    public double Height => canvas.ActualHeight;

    public CourseRenderer(Canvas canvas)
    {
        this.canvas = canvas;
    }

    public void Render(Course course)
    {
        canvas.Children.Clear();

        double scale = Width / course.Length;

        DrawSections(course, scale);
        DrawPhases(course.Phases, scale);
        DrawStraights(course.Straights, scale);
        DrawCorners(course.Corners, scale);
        DrawNoMansLand(course.NoMansLand, scale);
        DrawSlopes(course.Slopes, scale);
        DrawTrackWithSlopes(course, course.Slopes, scale);
    }

    private void DrawSections(Course course, double scale)
    {
        var sectionWidth = Width / 24;
        for (int i = 1; i <= 24; i++)
        {
            Rectangle rect = new Rectangle
            {
                Width = sectionWidth,
                Height = RowHeight,
                Fill = Brushes.AliceBlue,
                Stroke = Brushes.Gray,
                StrokeThickness = 0.5
            };
            
            Canvas.SetLeft(rect, sectionWidth * (i - 1));
            Canvas.SetTop(rect, canvas.ActualHeight - RowHeight);
            Canvas.SetZIndex(rect, 1);

            canvas.Children.Add(rect);
            
            TextBlock centerText = new TextBlock
            {
                Text = i.ToString(),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 25
            };
            
            centerText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var textWidth = centerText.DesiredSize.Width;

            Canvas.SetLeft(centerText, sectionWidth * (i - 1) + sectionWidth / 2 - textWidth / 2);
            Canvas.SetTop(centerText, canvas.ActualHeight - RowHeight / 2 - 20);
            Canvas.SetZIndex(centerText, 10);

            canvas.Children.Add(centerText);
        }
    }

    private void DrawTrackWithSlopes(Course course, List<Slope> courseSlopes, double scale)
    {
        double baseY = canvas.ActualHeight - TrackY + RowHeight;
        double currentY = canvas.ActualHeight - TrackY + RowHeight / 2;

        List<Point> topPoints = new();
        
        topPoints.Add(new Point(0, currentY));
        
        foreach (var slope in courseSlopes)
        {
            double startX = X(slope.Start, scale);
            double endX = X(slope.End, scale);
            
            topPoints.Add(new Point(startX, currentY));
            
            currentY -= slope.Value / 1000.0 * 2;
            
            topPoints.Add(new Point(endX, currentY));
        }
        
        if (courseSlopes.Count == 0 || courseSlopes[^1].End != course.Length)
        {
            topPoints.Add(new Point(canvas.ActualWidth, currentY));
        }
        
        PathFigure figure = new PathFigure
        {
            StartPoint = topPoints[0],
            IsClosed = true
        };

        foreach (var topPoint in topPoints)
        {
            figure.Segments.Add(new LineSegment(topPoint, true));
        }
        
        figure.Segments.Add(new LineSegment(new Point(topPoints[^1].X, baseY), true));
        figure.Segments.Add(new LineSegment(new Point(0, baseY), true));

        PathGeometry geometry = new PathGeometry();
        geometry.Figures.Add(figure);

        Path path = new Path
        {
            Data = geometry,
            Fill = Brushes.YellowGreen,
            Stroke = Brushes.YellowGreen,
            StrokeThickness = 2
        };

        canvas.Children.Add(path);
    }

    private void DrawSlopes(List<Slope> courseSlopes, double scale)
    {
        var y = canvas.ActualHeight - RowHeight * 4;
        
        Rectangle rect = new Rectangle
        {
            Width = canvas.ActualWidth,
            Height = RowHeight,
            Fill = Brushes.LightCyan
        };
        
        Canvas.SetLeft(rect, 0);
        Canvas.SetTop(rect, y);
        Canvas.SetZIndex(rect, 1);
        
        canvas.Children.Add(rect);

        foreach (var slope in courseSlopes)
        {
            double x = X(slope.Start, scale);
            double width = X(slope.End - slope.Start, scale);
            string label;
            Brush brush;

            if (slope.Value > 0)
            {
                label = "↗";
                brush = Brushes.SandyBrown;
            }
            else
            {
                label = "↘";
                brush = Brushes.LightSeaGreen;
            }

            rect = new Rectangle
            {
                Width = width,
                Height = RowHeight,
                Fill = brush
            };

            Canvas.SetLeft(rect, X(slope.Start, scale));
            Canvas.SetTop(rect, y);
            Canvas.SetZIndex(rect, 1);

            canvas.Children.Add(rect);
            
            TextBlock centerText = new TextBlock
            {
                Text = label,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 25
            };

            centerText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var textWidth = centerText.DesiredSize.Width;

            Canvas.SetLeft(centerText, x + width / 2 - textWidth / 2);
            Canvas.SetTop(centerText, y + RowHeight / 2 - 20);
            Canvas.SetZIndex(centerText, 10);

            canvas.Children.Add(centerText);

            var index = courseSlopes.IndexOf(slope);

            bool hasPrevious = index > 0;

            bool showStart = slope.Start > 0 && (!hasPrevious || slope.Start != courseSlopes[index - 1].End);
            
            if (showStart)
            {
                TextBlock startText = new TextBlock
                {
                    Text = $"{slope.Start}m",
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    FontSize = 20
                };

                startText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double startWidth = startText.DesiredSize.Width;

                Canvas.SetLeft(startText, x - startWidth / 2);
                Canvas.SetTop(startText, y + RowHeight / 2 + 25);
                Canvas.SetZIndex(startText, 10);

                canvas.Children.Add(startText);
            }
            
            TextBlock endText = new TextBlock
            {
                Text = $"{slope.End}m",
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 20
            };

            endText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double endWidth = endText.DesiredSize.Width;

            Canvas.SetLeft(endText, x + width - endWidth / 2);
            Canvas.SetTop(endText, y + RowHeight / 2 + 5);
            Canvas.SetZIndex(endText, 10);

            canvas.Children.Add(endText);
        }
    }

    private void DrawNoMansLand(List<Section> courseNoMansLand, double scale)
    {
        var y = canvas.ActualHeight - RowHeight * 3;

        foreach (var noMansLand in courseNoMansLand)
        {
            double x = X(noMansLand.Start, scale);
            double width = X(noMansLand.End - noMansLand.Start, scale);

            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = RowHeight,
                Fill = Brushes.LightGray
            };

            Canvas.SetLeft(rect, X(noMansLand.Start, scale));
            Canvas.SetTop(rect, y);
            Canvas.SetZIndex(rect, 1);

            canvas.Children.Add(rect);

            TextBlock endText = new TextBlock
            {
                Text = $"{noMansLand.End}m",
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 25
            };

            endText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double endWidth = endText.DesiredSize.Width;

            Canvas.SetLeft(endText, x + width - endWidth / 2);
            Canvas.SetTop(endText, y + RowHeight / 2);
            Canvas.SetZIndex(endText, 10);

            canvas.Children.Add(endText);
        }
    }

    private void DrawCorners(List<CornerSection> courseCorners, double scale)
    {
        var y = canvas.ActualHeight - RowHeight * 3;

        foreach (var corner in courseCorners)
        {
            double x = X(corner.Start, scale);
            double width = X(corner.End - corner.Start, scale);

            Brush brush = corner.Number % 2 == 0 ? Brushes.Coral : Brushes.Orange;

            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = RowHeight,
                Fill = brush
            };

            Canvas.SetLeft(rect, X(corner.Start, scale));
            Canvas.SetTop(rect, y);
            Canvas.SetZIndex(rect, 1);

            canvas.Children.Add(rect);

            TextBlock centerText = new TextBlock
            {
                Text = $"Corner ↪ {corner.Number}",
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 25
            };

            centerText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var textWidth = centerText.DesiredSize.Width;

            Canvas.SetLeft(centerText, x + width / 2 - textWidth / 2);
            Canvas.SetTop(centerText, y + RowHeight / 2 - 20);
            Canvas.SetZIndex(centerText, 10);

            canvas.Children.Add(centerText);

            TextBlock endText = new TextBlock
            {
                Text = $"{corner.End}m",
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 25
            };

            endText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double endWidth = endText.DesiredSize.Width;

            Canvas.SetLeft(endText, x + width - endWidth / 2);
            Canvas.SetTop(endText, y + RowHeight / 2);
            Canvas.SetZIndex(endText, 10);

            canvas.Children.Add(endText);
        }
    }

    private void DrawStraights(List<Section> courseStraights, double scale)
    {
        var y = canvas.ActualHeight - RowHeight * 3;

        for (var i = 0; i < courseStraights.Count; i++)
        {
            var straight = courseStraights[i];
            double x = X(straight.Start, scale);
            double width = X(straight.End - straight.Start, scale);

            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = RowHeight,
                Fill = Brushes.LightSkyBlue
            };

            Canvas.SetLeft(rect, X(straight.Start, scale));
            Canvas.SetTop(rect, y);
            Canvas.SetZIndex(rect, 1);

            canvas.Children.Add(rect);

            TextBlock centerText = new TextBlock
            {
                Text = "Straight →",
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 25
            };

            centerText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var textWidth = centerText.DesiredSize.Width;

            Canvas.SetLeft(centerText, x + width / 2 - textWidth / 2);
            Canvas.SetTop(centerText, y + RowHeight / 2 - 20);
            Canvas.SetZIndex(centerText, 10);

            canvas.Children.Add(centerText);

            if (i == courseStraights.Count - 1) continue;

            TextBlock endText = new TextBlock
            {
                Text = $"{straight.End}m",
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 25
            };

            endText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double endWidth = endText.DesiredSize.Width;

            Canvas.SetLeft(endText, x + width - endWidth / 2);
            Canvas.SetTop(endText, y + RowHeight / 2);
            Canvas.SetZIndex(endText, 10);

            canvas.Children.Add(endText);
        }
    }

    private void DrawPhases(List<Phase> coursePhases, double scale)
    {
        var y = canvas.ActualHeight - RowHeight * 2;
        
        foreach (var phase in coursePhases)
        {
            Brush brush = phase.PhaseType switch
            {
                PhaseType.OpeningLeg => Brushes.CadetBlue,
                PhaseType.MiddleLeg => Brushes.Khaki,
                PhaseType.FinalLeg => Brushes.MediumOrchid,
                _ => Brushes.MediumPurple
            };

            string phaseLabel = phase.PhaseType switch
            {
                PhaseType.OpeningLeg => "Opening Leg",
                PhaseType.MiddleLeg => "Middle Leg",
                PhaseType.FinalLeg => "Final Leg",
                _ => "Last Spurt"
            };
            
            double x = X(phase.Start, scale);
            double width = X(phase.End - phase.Start, scale);
            
            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = RowHeight,
                Fill = brush
            };
            
            Canvas.SetLeft(rect, X(phase.Start, scale));
            Canvas.SetTop(rect, y);
            Canvas.SetZIndex(rect, 1);

            canvas.Children.Add(rect);
            
            TextBlock centerText = new TextBlock
            {
                Text = phaseLabel,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 25
            };
            
            centerText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var textWidth = centerText.DesiredSize.Width;

            Canvas.SetLeft(centerText, x + width / 2 - textWidth / 2);
            Canvas.SetTop(centerText, y + RowHeight / 2 - 20);
            Canvas.SetZIndex(centerText, 10);

            canvas.Children.Add(centerText);

            if (phase.PhaseType == PhaseType.LastSpurt) continue;
            
            TextBlock endText = new TextBlock
            {
                Text = $"{phase.End}m",
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 25
            };

            endText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double endWidth = endText.DesiredSize.Width;

            Canvas.SetLeft(endText, x + width - endWidth / 2);
            Canvas.SetTop(endText, y + RowHeight / 2);
            Canvas.SetZIndex(endText, 10);

            canvas.Children.Add(endText);
        }
    }
    
    private double X(double meters, double scale)
    {
        return meters * scale;
    }
}