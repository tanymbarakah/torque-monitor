using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using TorqueMonitor.Models;

namespace TorqueMonitor.ViewModels
{
    public class GaugeViewModel : INotifyPropertyChanged
    {
        private const int MaxHistoryPoints = 40;
        private readonly Queue<double> _historyQueue = new();

        private double _currentNeedleAngle;
        private double _targetNeedleAngle;
        private double _needleVelocity;
        private double _displayValue;
        private string _formattedValue = "0";
        private string _subtitle = string.Empty;
        private string _extraInfo = string.Empty;
        private bool _isSensorAvailable = true;
        private PointCollection _graphPoints = new();

        public GaugeType Type { get; }
        public string Title { get; }
        public string Unit { get; }
        public double MinValue { get; }
        public double MaxValue { get; }

        public Color AccentColor { get; }
        public Color GlowColor { get; }
        public bool HasZones { get; }
        public double WarningThreshold { get; }
        public double DangerThreshold { get; }
        public Color LowZoneColor { get; }
        public Color MidZoneColor { get; }
        public Color HighZoneColor { get; }

        // Start at -120° (7 o'clock) and sweep 240° to +120° (5 o'clock)
        public const double StartAngleDegrees = -120.0;
        public const double SweepAngleDegrees = 240.0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double CurrentNeedleAngle
        {
            get => _currentNeedleAngle;
            set
            {
                if (Math.Abs(_currentNeedleAngle - value) > 0.01)
                {
                    _currentNeedleAngle = value;
                    OnPropertyChanged();
                }
            }
        }

        public double DisplayValue
        {
            get => _displayValue;
            set
            {
                if (Math.Abs(_displayValue - value) > 0.05)
                {
                    _displayValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FormattedValue
        {
            get => _formattedValue;
            set
            {
                if (_formattedValue != value)
                {
                    _formattedValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Subtitle
        {
            get => _subtitle;
            set
            {
                if (_subtitle != value)
                {
                    _subtitle = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ExtraInfo
        {
            get => _extraInfo;
            set
            {
                if (_extraInfo != value)
                {
                    _extraInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSensorAvailable
        {
            get => _isSensorAvailable;
            set
            {
                if (_isSensorAvailable != value)
                {
                    _isSensorAvailable = value;
                    OnPropertyChanged();
                }
            }
        }

        public PointCollection GraphPoints
        {
            get => _graphPoints;
            set
            {
                _graphPoints = value;
                OnPropertyChanged();
            }
        }

        public GaugeViewModel(GaugeData data)
        {
            Type = data.Type;
            Title = data.Title;
            Unit = data.Unit;
            MinValue = data.MinValue;
            MaxValue = data.MaxValue;
            AccentColor = data.AccentColor;
            GlowColor = data.GlowColor;
            HasZones = data.HasZones;
            WarningThreshold = data.WarningThreshold;
            DangerThreshold = data.DangerThreshold;
            LowZoneColor = data.LowZoneColor;
            MidZoneColor = data.MidZoneColor;
            HighZoneColor = data.HighZoneColor;

            _currentNeedleAngle = StartAngleDegrees;
            _targetNeedleAngle = StartAngleDegrees;

            // Fill initial zero history
            for (int i = 0; i < MaxHistoryPoints; i++)
            {
                _historyQueue.Enqueue(0);
            }
            UpdateGraphPoints();
        }

        public void UpdateTelemetry(double? val, string subtitle, string extraInfo, bool available = true)
        {
            IsSensorAvailable = available;
            Subtitle = subtitle;
            ExtraInfo = extraInfo;

            if (!available || !val.HasValue)
            {
                FormattedValue = "N/A";
                _targetNeedleAngle = StartAngleDegrees;
                AddHistoryPoint(0);
                return;
            }

            double clamped = Math.Clamp(val.Value, MinValue, MaxValue);
            double ratio = (clamped - MinValue) / (MaxValue - MinValue);
            _targetNeedleAngle = StartAngleDegrees + (ratio * SweepAngleDegrees);

            // Format reading
            if (Unit == "°C")
            {
                FormattedValue = $"{Math.Round(clamped):F0}°C";
            }
            else if (Unit == "%")
            {
                FormattedValue = $"{Math.Round(clamped):F0}%";
            }
            else if (Unit == "GHz")
            {
                FormattedValue = $"{clamped:F2}";
            }
            else if (Unit == "GB")
            {
                FormattedValue = $"{clamped:F1}";
            }
            else
            {
                FormattedValue = $"{Math.Round(clamped):F0}";
            }

            AddHistoryPoint(ratio);
        }

        private void AddHistoryPoint(double ratio)
        {
            if (_historyQueue.Count >= MaxHistoryPoints)
            {
                _historyQueue.Dequeue();
            }
            _historyQueue.Enqueue(ratio);
            UpdateGraphPoints();
        }

        private void UpdateGraphPoints()
        {
            // Graph fits inside width: 80, height: 26 in the lower dial quadrant
            const double graphWidth = 84.0;
            const double graphHeight = 24.0;

            var points = new PointCollection();
            double step = graphWidth / (MaxHistoryPoints - 1);
            int idx = 0;

            foreach (var h in _historyQueue)
            {
                double x = idx * step;
                // Invert Y because WPF coordinates go down
                double y = graphHeight - (Math.Clamp(h, 0.0, 1.0) * graphHeight);
                points.Add(new System.Windows.Point(x, y));
                idx++;
            }

            GraphPoints = points;
        }

        /// <summary>
        /// Frame update for smooth organic needle animation (called at 60 FPS)
        /// Damped spring physics model for realistic automotive tachometer movement.
        /// </summary>
        public void AnimatePhysics(double dt)
        {
            // Spring stiffness and damping coefficient
            const double stiffness = 65.0;
            const double damping = 0.78;

            double displacement = _targetNeedleAngle - _currentNeedleAngle;

            // Apply acceleration
            _needleVelocity += displacement * stiffness * dt;
            _needleVelocity *= Math.Pow(damping, dt * 60.0);

            _currentNeedleAngle += _needleVelocity * dt;

            // Clamp small oscillations
            if (Math.Abs(displacement) < 0.05 && Math.Abs(_needleVelocity) < 0.1)
            {
                _currentNeedleAngle = _targetNeedleAngle;
                _needleVelocity = 0;
            }

            OnPropertyChanged(nameof(CurrentNeedleAngle));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
