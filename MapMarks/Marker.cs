using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MapMarks
{
    public class Marker : INotifyPropertyChanged
    {
        private Point _position;
        private UniqueMarker _type;
        private MarkerCollection _vertices = new MarkerCollection(); // Vertex (syn: Node) -> Vertices (syn: Nodes)

        /// <summary>
        /// Связующий маркер (дополнительный маркер, который
        /// ставится между основными). Нужен для того, чтобы
        /// определить расстояние на нелинейном маршруте.
        /// Маркер не является основным.
        /// Данное свойство определяет, является ли маркер
        /// связующим.
        /// </summary>
        public ObservableCollection<Line> Lines { get; set; } = new ObservableCollection<Line>();

        public bool IsChecked { get; set; } = false;
        public int Number { get; set; }

        public double X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position = new Point(value, Position.Y);
            }
        }
        public double Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position = new Point(Position.X, value);
            }
        }
        public UniqueMarker Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged();
                }
            }
        }
        public Point Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Margin");
                }
            }
        }
        public Thickness Margin
        {
            get { return new Thickness(Position.X, Position.Y, 0, 0); }
        }

        public double GetDistance(Marker m)
        {
            return Math.Sqrt(Math.Pow(X - m.X, 2) + Math.Pow(Y - m.Y, 2));
        }
        /// <summary>
        /// Набор близлежащих точек относительной этой (this) точки
        /// </summary>
        public MarkerCollection Vertices // Nodes
        {
            get
            {
                return _vertices;
            }
            set
            {
                if (_vertices != value)
                {
                    _vertices = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<short> LinkIndices { get; set; } = new List<short>();

        delegate void CheckMarker(CheckMarker check, Marker marker, MarkerCollection collection);
        public MarkerCollection GetAvailableMarkers()
        {
            void checkMarker(CheckMarker check, Marker marker, MarkerCollection collection)
            {
                marker.IsChecked = true;
                collection.Add(marker);
                foreach (var v in marker.Vertices)
                    if (!v.IsChecked) check(check, v, collection);
            }
            MarkerCollection availableCollection = new MarkerCollection();
            checkMarker(checkMarker, this, availableCollection);
            foreach (Marker m in availableCollection) m.IsChecked = false;
            return availableCollection;
        }

        delegate void GoToNext(GoToNext next, Marker marker, double len);
        public MarkerCollection GetRoute(out double total)
        {
            Stack<Marker> route = new Stack<Marker>();
            MarkerCollection actualRoute = new MarkerCollection();
            int avCount = GetAvailableMarkers().Where(m => !m.Type.IsConnectingLink).Count();
            double l = double.MaxValue;
            short ind = 1;
            int i = 0;
            string gygy2 = "";
            void gt(GoToNext next, Marker marker, double len)
            {
                i++;
                marker.IsChecked = true; route.Push(marker);
                if (marker.Type.IsConnectingLink && marker.LinkIndices.Contains(ind) == false) marker.LinkIndices.Add(ind);
                if (route.Where(m => !m.Type.IsConnectingLink).Count() == avCount && len < l)
                {
                    l = len;
                    actualRoute = new MarkerCollection(route.ToList());
                }
                string lline = "=================";
                string gygy = $"{i}) this: {marker.Type.Name}#{marker.Number}\n{lline}\nVertices:\n";
                foreach (var huip in marker.Vertices) gygy += $"{huip.Type.Name}#{huip.Number} ; ";
                gygy += $"\n{lline}\nPath:\n";
                foreach (var ft in route.ToArray().Reverse()) gygy += $"{ft.Type.Name}#{ft.Number} --> ";
                //MessageBox.Show(gygy);
                gygy2 += $"{i}) {marker.Type.Name}#{marker.Number}\n";
                //MessageBox.Show(gygy2);
                foreach (var v in marker.Vertices)
                {
                    if (v.Type.IsConnectingLink)
                    {
                        if (v.LinkIndices.Contains(ind) == false)
                        {
                            next(next, v, len + marker.GetDistance(v));
                        }
                    }
                    else if (!v.IsChecked)
                    {
                        ind++;
                        next(next, v, len + marker.GetDistance(v));
                        ind--;
                    }
                }
                if (marker.LinkIndices.Contains(ind)) marker.LinkIndices.Remove(ind);
                marker.IsChecked = false; route.Pop();
                i--;
            }
            gt(gt, this, 0);
            total = l;
            return actualRoute;
        }

        public string MainInfo { get { return ToString(); } }

        public override string ToString()
        {
            return $"{Type.Name} #{Number}:{string.Format("\nX - {0:0.00}\nY - {1:0.00}", Position.X, Position.Y)}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class MarkerCollection : ObservableCollection<Marker>
    {
        public void CheckAll() { foreach (var m in this) m.IsChecked = true; }
        public void UncheckAll() { foreach (var m in this) m.IsChecked = false; }

        public MarkerCollection() : base() { }
        public MarkerCollection(List<Marker> list) : base(list) { }
        public MarkerCollection(params Marker[] ms) : base(ms.ToList()) { }
    }
}
