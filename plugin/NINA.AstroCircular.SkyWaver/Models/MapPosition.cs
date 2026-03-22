using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NINA.AstroCircular.SkyWaver.Models {

    public enum PositionState {
        Pending,
        Active,
        Done,
        Failed
    }

    /// <summary>
    /// A position dot on the sensor map visualization.
    /// Canvas coordinates + state for coloring.
    /// </summary>
    public class MapPosition : INotifyPropertyChanged {
        public string Label { get; set; }
        public double CanvasX { get; set; }
        public double CanvasY { get; set; }
        public bool IsCenter { get; set; }

        private PositionState state = PositionState.Pending;
        public PositionState State {
            get => state;
            set { state = value; OnPropertyChanged(); OnPropertyChanged(nameof(FillColor)); OnPropertyChanged(nameof(StrokeColor)); }
        }

        public string FillColor => State switch {
            PositionState.Done => "#4ade80",
            PositionState.Active => "#ef4444",
            PositionState.Failed => "#f59e0b",
            _ => "#3a4060"
        };

        public string StrokeColor => State switch {
            PositionState.Done => "#22c55e",
            PositionState.Active => "#dc2626",
            PositionState.Failed => "#d97706",
            _ => "#555a70"
        };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
