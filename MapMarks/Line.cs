using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MapMarks
{
    public class Line : INotifyPropertyChanged
    {
        private double zoom;
        public Marker Marker1 { get; }
        public Marker Marker2 { get; }
        public Line(Marker m1, Marker m2, bool routeLines = false)
        {
            Marker1 = m1;
            if (!routeLines)
            {
                m1.Lines.Add(this);
                m1.Vertices.Add(m2);
            }
            Marker2 = m2;
            if (!routeLines)
            {
                m2.Lines.Add(this);
                m2.Vertices.Add(m1);
            }
        }
        public double Zoom
        {
            set
            {
                zoom = value;
                OnPropertyChanged("From");
                OnPropertyChanged("To");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
