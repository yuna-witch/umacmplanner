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
    private const int CenterTextFontSize = 25;
    private const int EndTextFontSize = 20;
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
            
            DrawCenteredText(i.ToString(), sectionWidth * (i - 1) + sectionWidth / 2, canvas.ActualHeight - RowHeight);
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
        
        DrawRectangle(0, y,  canvas.ActualWidth, Brushes.LightCyan);

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

            DrawRectangle(x, y, width, brush);
            
            DrawCenteredText(label, x + width / 2, y);

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
                    FontSize = EndTextFontSize
                };

                startText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double startWidth = startText.DesiredSize.Width;

                Canvas.SetLeft(startText, x - startWidth / 2);
                Canvas.SetTop(startText, y + RowHeight / 2 + 25);
                Canvas.SetZIndex(startText, 10);

                canvas.Children.Add(startText);
            }
            
            DrawEndText($"{slope.End}m", x + width, y, 5);
        }
    }

    private void DrawNoMansLand(List<Section> courseNoMansLand, double scale)
    {
        var y = canvas.ActualHeight - RowHeight * 3;

        foreach (var noMansLand in courseNoMansLand)
        {
            double x = X(noMansLand.Start, scale);
            double width = X(noMansLand.End - noMansLand.Start, scale);

            DrawRectangle(x, y, width, Brushes.LightGray);

            DrawEndText($"{noMansLand.End}m", x + width, y, 10);
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

            DrawRectangle(x, y, width, brush);

            DrawCenteredText($"Corner ↪ {corner.Number}", x + width / 2, y);

            DrawEndText($"{corner.End}m", x + width, y, 10);
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

            DrawRectangle(x, y, width, Brushes.LightSkyBlue);

            DrawCenteredText("Straight →", x + width / 2, y);

            if (i == courseStraights.Count - 1) continue;

            DrawEndText($"{straight.End}m", x + width, y, 10);
        }
    }

    private void DrawPhases(List<Phase> coursePhases, double scale)
    {
        var y = canvas.ActualHeight - RowHeight * 2;
        
        foreach (var phase in coursePhases)
        {
            var brush = GetPhaseBrush(phase);
            var phaseLabel = GetPhaseLabel(phase);
            
            double x = X(phase.Start, scale);
            double width = X(phase.End - phase.Start, scale);
            
            DrawRectangle(x, y, width, brush);
            
            DrawCenteredText(phaseLabel, x + width / 2, y);

            if (phase.PhaseType == PhaseType.LastSpurt) continue;
            
            DrawEndText($"{phase.End}m", x + width, y, 10);
        }
    }

    private static string GetPhaseLabel(Phase phase)
    {
        string phaseLabel = phase.PhaseType switch
        {
            PhaseType.OpeningLeg => "Opening Leg",
            PhaseType.MiddleLeg => "Middle Leg",
            PhaseType.FinalLeg => "Final Leg",
            _ => "Last Spurt"
        };
        return phaseLabel;
    }

    private static Brush GetPhaseBrush(Phase phase)
    {
        Brush brush = phase.PhaseType switch
        {
            PhaseType.OpeningLeg => Brushes.CadetBlue,
            PhaseType.MiddleLeg => Brushes.Khaki,
            PhaseType.FinalLeg => Brushes.MediumOrchid,
            _ => Brushes.MediumPurple
        };
        return brush;
    }

    private double X(double meters, double scale)
    {
        return meters * scale;
    }
    
    private void DrawRectangle(double x, double y, double width, Brush fill)
    {
        Rectangle rect = new Rectangle
        {
            Width = width,
            Height = RowHeight,
            Fill = fill
        };

        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);

        canvas.Children.Add(rect);
    }
    
    private void DrawCenteredText(string text, double x, double y, Brush? foreground = null)
    {
        TextBlock tb = new TextBlock
        {
            Text = text,
            Foreground = foreground ?? Brushes.Black,
            FontWeight = FontWeights.Bold,
            FontSize = CenterTextFontSize
        };

        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Canvas.SetLeft(tb, x - tb.DesiredSize.Width / 2);
        Canvas.SetTop(tb, y + RowHeight / 2 - 20);
        Canvas.SetZIndex(tb, 10);

        canvas.Children.Add(tb);
    }
    
    private void DrawEndText(string text, double x, double y, int yAdjustment, Brush? foreground = null)
    {
        TextBlock tb = new TextBlock
        {
            Text = text,
            Foreground = foreground ?? Brushes.Black,
            FontWeight = FontWeights.Bold,
            FontSize = EndTextFontSize
        };

        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Canvas.SetLeft(tb, x - tb.DesiredSize.Width / 2);
        Canvas.SetTop(tb, y + RowHeight / 2 + yAdjustment);
        Canvas.SetZIndex(tb, 10);

        canvas.Children.Add(tb);
    }
}