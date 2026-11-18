using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TorqueMonitor.ViewModels;

namespace TorqueMonitor.Controls
{
    public partial class AnalogueGaugeControl : UserControl
    {
        private GaugeViewModel? _viewModel;

        public AnalogueGaugeControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is GaugeViewModel vm)
            {
                _viewModel = vm;
                BuildDialScale();
                ApplyThemeColors();
            }
        }

        private void ApplyThemeColors()
        {
            if (_viewModel == null) return;

            var brush = new SolidColorBrush(_viewModel.AccentColor);
            GraphPolyline.Stroke = brush;
            GraphPolylineGlow.Stroke = brush;
        }

        private void BuildDialScale()
        {
            if (_viewModel == null) return;
            ScaleCanvas.Children.Clear();

            const double centerX = 120.0;
            const double centerY = 120.0;
            const double radius = 86.0;

            double minVal = _viewModel.MinValue;
            double maxVal = _viewModel.MaxValue;
            double range = maxVal - minVal;

            // Draw colored zones or colored arc track
            if (_viewModel.HasZones)
            {
                // Temperature Gauge: 0-50 Blue, 50-80 Yellow, 80-120 Red
                DrawArcSegment(centerX, centerY, radius + 2, minVal, 50, minVal, maxVal, _viewModel.LowZoneColor, 2.5);
                DrawArcSegment(centerX, centerY, radius + 2, 50, 80, minVal, maxVal, _viewModel.MidZoneColor, 2.5);
                DrawArcSegment(centerX, centerY, radius + 2, 80, maxVal, minVal, maxVal, _viewModel.HighZoneColor, 3.5);
            }
            else
            {
                // Usage Gauges: Accent colored subtle track
                DrawArcSegment(centerX, centerY, radius + 2, minVal, maxVal, minVal, maxVal, _viewModel.AccentColor, 1.8);
            }

            // Determine tick intervals
            int majorCount = (int)(range <= 100 ? 5 : 6); // 0, 20, 40, 60, 80, 100 (5 segments) or 6 for 120
            double majorStep = range / majorCount;
            int minorPerMajor = 4; // 4 minor ticks between majors
            int totalTicks = majorCount * minorPerMajor;

            for (int i = 0; i <= totalTicks; i++)
            {
                double tickVal = minVal + (i * (range / totalTicks));
                double ratio = (tickVal - minVal) / range;
                double angleDeg = GaugeViewModel.StartAngleDegrees + (ratio * GaugeViewModel.SweepAngleDegrees);
                // In standard polar coords: 0° is right (3 o'clock). 
                // Our needle angle 0° is UP (12 o'clock, which is 270° in math polar).
                double mathAngleRad = (angleDeg - 90.0) * Math.PI / 180.0;

                bool isMajor = (i % minorPerMajor == 0);
                double tickLen = isMajor ? 11.0 : 6.0;
                double tickThickness = isMajor ? 1.8 : 0.9;

                // Color for ticks
                Brush tickBrush = Brushes.White;
                if (_viewModel.HasZones)
                {
                    if (tickVal >= 80)
                        tickBrush = new SolidColorBrush(_viewModel.HighZoneColor);
                    else if (tickVal >= 50)
                        tickBrush = new SolidColorBrush(_viewModel.MidZoneColor);
                    else
                        tickBrush = new SolidColorBrush(Color.FromRgb(220, 230, 242));
                }
                else
                {
                    // Accent colored at top end
                    if (ratio >= 0.85)
                        tickBrush = new SolidColorBrush(Color.FromRgb(255, 60, 60)); // Redline
                    else if (isMajor)
                        tickBrush = new SolidColorBrush(Color.FromRgb(240, 245, 255));
                    else
                        tickBrush = new SolidColorBrush(Color.FromArgb(180, 180, 195, 210));
                }

                double innerR = radius - tickLen;
                double x1 = centerX + Math.Cos(mathAngleRad) * innerR;
                double y1 = centerY + Math.Sin(mathAngleRad) * innerR;
                double x2 = centerX + Math.Cos(mathAngleRad) * radius;
                double y2 = centerY + Math.Sin(mathAngleRad) * radius;

                var tickLine = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = tickBrush,
                    StrokeThickness = tickThickness,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                ScaleCanvas.Children.Add(tickLine);

                // Add Numerical Labels for major ticks
                if (isMajor)
                {
                    double textR = radius - 20.0;
                    double tx = centerX + Math.Cos(mathAngleRad) * textR;
                    double ty = centerY + Math.Sin(mathAngleRad) * textR;

                    string labelText = $"{Math.Round(tickVal)}";
                    var tb = new TextBlock
                    {
                        Text = labelText,
                        FontFamily = new FontFamily("Bahnschrift, Segoe UI, sans-serif"),
                        FontSize = 9.5,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = tickBrush,
                        IsHitTestVisible = false
                    };

                    tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(tb, tx - (tb.DesiredSize.Width / 2.0));
                    Canvas.SetTop(tb, ty - (tb.DesiredSize.Height / 2.0));
                    ScaleCanvas.Children.Add(tb);
                }
            }
        }

        private void DrawArcSegment(double cx, double cy, double r, double v1, double v2, double minV, double maxV, Color color, double strokeThickness)
        {
            double r1 = (v1 - minV) / (maxV - minV);
            double r2 = (v2 - minV) / (maxV - minV);

            double a1 = (GaugeViewModel.StartAngleDegrees + (r1 * GaugeViewModel.SweepAngleDegrees) - 90.0) * Math.PI / 180.0;
            double a2 = (GaugeViewModel.StartAngleDegrees + (r2 * GaugeViewModel.SweepAngleDegrees) - 90.0) * Math.PI / 180.0;

            double startX = cx + Math.Cos(a1) * r;
            double startY = cy + Math.Sin(a1) * r;
            double endX = cx + Math.Cos(a2) * r;
            double endY = cy + Math.Sin(a2) * r;

            bool isLargeArc = (a2 - a1) > Math.PI;

            var pathFig = new PathFigure
            {
                StartPoint = new Point(startX, startY),
                IsClosed = false
            };
            pathFig.Segments.Add(new ArcSegment(new Point(endX, endY), new Size(r, r), 0, isLargeArc, SweepDirection.Clockwise, true));

            var pathGeo = new PathGeometry();
            pathGeo.Figures.Add(pathFig);

            // Subtle glow
            var glowPath = new Path
            {
                Data = pathGeo,
                Stroke = new SolidColorBrush(Color.FromArgb(90, color.R, color.G, color.B)),
                StrokeThickness = strokeThickness + 3,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 4 }
            };
            ScaleCanvas.Children.Add(glowPath);

            var path = new Path
            {
                Data = pathGeo,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = strokeThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            ScaleCanvas.Children.Add(path);
        }
    }
}
