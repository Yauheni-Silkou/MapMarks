using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapMarks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Point _pos;
        private UniqueMarkerCollection _uniqueMarkers = new UniqueMarkerCollection();
        private MarkerCollection _markers = new MarkerCollection();
        private ObservableCollection<Line> _lines = new ObservableCollection<Line>();
        private ObservableCollection<Line> _routeLines = new ObservableCollection<Line>();
        private Colour _lineColor, _routeLineColor;
        private Marker _selectedMarker;
        private double _zoomS = 1;
        private int _zoom = 100;
        private int _widthX = 0;
        private int _heightX = 0;
        private string _routeButtonText = "Create Route";
        private byte[] _mapBytes;
        private double _total = 0;

        private enum Mode
        {
            None, SelectMarker1, SelectMarker2
        }
        Mode mode = Mode.None;
        Marker marker1;
        Marker marker2;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public UniqueMarkerCollection UniqueMarkers
        {
            get { return _uniqueMarkers; }
            set
            {
                _uniqueMarkers = value;
                OnPropertyChanged();
            }
        }

        public MarkerCollection Markers
        {
            get { return _markers; }
            set
            {
                _markers = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Line> Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Line> RouteLines
        {
            get { return _routeLines; }
            set
            {
                _routeLines = value;
                OnPropertyChanged();
            }
        }
        public ColourList Colours
        {
            get
            {
                ColourList cs = new ColourList();
                foreach (var clrProp in typeof(Colors).GetProperties())
                    cs.Add(clrProp.Name, (Color)clrProp.GetValue(null));
                return cs;
            }
        }

        public Marker SelectedMarker
        {
            get { return _selectedMarker; }
            set
            {
                if (_selectedMarker != value)
                {
                    _selectedMarker = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Zoom
        {
            get { return _zoom; }
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    _widthX = (int)(_widthX * _zoomS);
                    _heightX = (int)(_heightX * _zoomS);
                    OnPropertyChanged();
                    OnPropertyChanged("WidthX");
                    OnPropertyChanged("HeightX");

                    foreach (var m in Markers)
                    {
                        m.Position = new Point(m.Position.X * _zoomS, m.Position.Y * _zoomS);
                    }
                }
            }
        }

        public Point Pos
        {
            get { return _pos; }
            set
            {
                if (_pos != value)
                {
                    _pos = value;
                    OnPropertyChanged();
                    OnPropertyChanged("StringPos");
                }
            }
        }

        public string StringPos
        {
            get
            {
                return string.Format("Position: {0:0.00} - {1:0.00}", Pos.X, Pos.Y);
            }
        }

        public int WidthX
        {
            get { return _widthX; }
            set
            {
                if (_widthX != value)
                {
                    _widthX = value;
                    OnPropertyChanged();
                }
            }
        }

        public int HeightX
        {
            get { return _heightX; }
            set
            {
                if (_heightX != value)
                {
                    _heightX = value;
                    OnPropertyChanged();
                }
            }
        }

        public Colour LineColor
        {
            get { return _lineColor; }
            set
            {
                if (_lineColor != value)
                {
                    _lineColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public Colour RouteLineColor
        {
            get { return _routeLineColor; }
            set
            {
                if (_routeLineColor != value)
                {
                    _routeLineColor = value;
                    OnPropertyChanged();
                }
            }
        }
        public string RouteButtonText
        {
            get { return _routeButtonText; }
            set
            {
                if (_routeButtonText != value)
                {
                    _routeButtonText = value;
                    OnPropertyChanged();
                }
            }
        }
        public BitmapImage Map
        {
            get
            {
                if (MapBytes == null || MapBytes.Length == 0) return new BitmapImage();
                using (var ms = new MemoryStream(MapBytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
            }
        }

        public byte[] MapBytes
        {
            get { return _mapBytes; }
            set
            {
                if (_mapBytes != value)
                {
                    _mapBytes = value;
                    _zoomS = 1;
                    Zoom = 100;
                    Markers.Clear();
                    SelectedMarker = null;
                    Lines.Clear();
                    RouteLines.Clear();
                    OnPropertyChanged();
                    WidthX = (int)Map.Width;
                    HeightX = (int)Map.Height;
                    OnPropertyChanged("Map");
               }
            }
        }

        public double Total
        {
            get { return _total; }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            Grid map = FindName("map") as Grid;
            Pos = e.GetPosition(map);
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            _zoomS = 2;
            Zoom = (int)(Zoom * _zoomS);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom > 25)
            {
                _zoomS = 0.5;
                Zoom = (int)(Zoom * _zoomS);
            }
        }

        private void AddMarkerTypeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "PNG|*.png|All files|*.*",
                //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string fn in openFileDialog.FileNames)
                    UniqueMarkers.Add(new UniqueMarker
                    {
                        ImageBytes = File.ReadAllBytes(fn),
                        Name = fn.Substring(fn.LastIndexOf('\\') + 1, fn.IndexOf('.') - fn.LastIndexOf('\\') - 1)
                    });
            }
        }

        private void SetMarker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (markerTypesComboBox.SelectedIndex == -1 || mode != Mode.None) return;
            int num = 1;
            var mkrs = Markers.Where(x => x.Type.Name == (markerTypesComboBox.SelectedItem as UniqueMarker).Name).ToList().OrderBy(y => y.Number).OrderBy(y => y.Type.Name);
            if (mkrs.Count() > 0)
                foreach (var x in mkrs)
                {
                    if (num == x.Number) num++;
                    else break;
                }
            Markers.Add(new Marker
            {
                Number = num,
                Position = new Point(Pos.X * Zoom / 100, Pos.Y * Zoom / 100),
                Type = (FindName("markerTypesComboBox") as ComboBox).SelectedItem as UniqueMarker
            });
            Markers = new MarkerCollection(Markers.OrderBy(x => x.Number).OrderBy(x => x.Type.Name).ToList());
        }

        private void MarkerButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode == Mode.None)
            {
                SelectedMarker = (sender as Button).Tag as Marker;
            }
            else if (mode == Mode.SelectMarker1)
            {
                mode = Mode.SelectMarker2;
                marker1 = (sender as Button).Tag as Marker;
                RouteButtonText = "Choose Marker 2";
            }
            else
            {
                marker2 = (sender as Button).Tag as Marker;
                if (marker1 != marker2 && !marker1.Vertices.Contains(marker2) && !marker2.Vertices.Contains(marker1))
                {
                    mode = Mode.None;
                    Lines.Add(new Line(marker1, marker2));
                    Cursor = Cursors.Arrow;
                    RouteButtonText = "Create Route";
                }
            }
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mode == Mode.None)
            {
                SelectedMarker = (sender as Button).Tag as Marker;
                Grid grid = FindName("propertyGrid") as Grid;
                grid.Visibility = Visibility.Visible;
            }
        }

        private void DeleteMarkerButton_Click(object sender, RoutedEventArgs e)
        {
            Markers.Remove(SelectedMarker);
            foreach(Line l in SelectedMarker.Lines)
            {
                if (SelectedMarker != l.Marker1) l.Marker1.Lines.Remove(l);
                if (SelectedMarker != l.Marker2) l.Marker2.Lines.Remove(l);
                Lines.Remove(l);
            }
            foreach (Marker m in Markers) m.Vertices.Remove(SelectedMarker);
            Grid grid = FindName("propertyGrid") as Grid;
            grid.Visibility = Visibility.Collapsed;
            RouteLines.Clear();
        }

        private void ClosePropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            Grid grid = FindName("propertyGrid") as Grid;
            grid.Visibility = Visibility.Collapsed;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sndr = sender as ComboBox;
            if (sndr.SelectedItem == SelectedMarker) sndr.SelectedIndex = -1;
        }

        private void ComboBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (sender as ComboBox).SelectedIndex = -1;
        }

        private void CreateRouteButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode == Mode.None)
            {
                Cursor = Cursors.Hand;
                mode = Mode.SelectMarker1;
                RouteButtonText = "Choose Marker 1";
            }
            else
            {
                Cursor = Cursors.Arrow;
                mode = Mode.None;
                RouteButtonText = "Create Route";
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var v = (sender as ListView).SelectedItem as Marker;
            if (v == null) return;
            var line = (from ls in Lines
                       where (ls.Marker1 == v && ls.Marker2 == SelectedMarker) || (ls.Marker2 == v && ls.Marker1 == SelectedMarker)
                       select ls).ToList()[0];
            Lines.Remove(line);
            SelectedMarker.Vertices.Remove(v);
            v.Vertices.Remove(SelectedMarker);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lineColorComboBox.SelectedIndex = 122;
            routeLineColorComboBox.SelectedIndex = 113;
        }

        private void ChooseMapButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "PNG|*.png|JPEG|*.jpeg;*.jpg|BMP|*.bmp|All files|*.*",
                //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (openFileDialog.ShowDialog() == true)
                MapBytes = File.ReadAllBytes(openFileDialog.FileName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            blockGrid.Visibility = Visibility.Visible;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "BIN|*.bin",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                OverwritePrompt = true
            };
            if (saveFileDialog.ShowDialog() == false)
            {
                blockGrid.Visibility = Visibility.Collapsed;
                return;
            }
            if (Zoom < 100) do ZoomInButton_Click(null, null); while (Zoom != 100);
            else if (Zoom > 100) do ZoomOutButton_Click(null, null); while (Zoom != 100);
            string filename = saveFileDialog.FileName;
            using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                writer.Write(MapBytes.Length);
                writer.Write(MapBytes);
                writer.Write(UniqueMarkers.Count);
                foreach (var u in UniqueMarkers)
                {
                    writer.Write(u.Name);
                    writer.Write(u.ImageBytes.Length);
                    writer.Write(u.ImageBytes);
                    writer.Write(u.IsConnectingLink);
                }
                writer.Write(Markers.Count);
                foreach (var m in Markers)
                {
                    writer.Write(UniqueMarkers.IndexOf(m.Type));
                    writer.Write(m.Number);
                    writer.Write(m.X);
                    writer.Write(m.Y);
                }
                writer.Write(Lines.Count);
                foreach (var l in Lines)
                {
                    writer.Write(UniqueMarkers.IndexOf(l.Marker1.Type));
                    writer.Write(l.Marker1.Number);
                    writer.Write(UniqueMarkers.IndexOf(l.Marker2.Type));
                    writer.Write(l.Marker2.Number);
                }
                writer.Write(RouteLines.Count);
                foreach (var r in RouteLines)
                {
                    writer.Write(UniqueMarkers.IndexOf(r.Marker1.Type));
                    writer.Write(r.Marker1.Number);
                    writer.Write(UniqueMarkers.IndexOf(r.Marker2.Type));
                    writer.Write(r.Marker2.Number);
                }
                writer.Write(lineColorComboBox.SelectedIndex);
                writer.Write(routeLineColorComboBox.SelectedIndex);
            }
            blockGrid.Visibility = Visibility.Collapsed;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            blockGrid.Visibility = Visibility.Visible;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "BIN|*.bin",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (openFileDialog.ShowDialog() == false)
            {
                blockGrid.Visibility = Visibility.Collapsed;
                return;
            }
            if (Zoom < 100) do ZoomInButton_Click(null, null); while (Zoom != 100);
            else if (Zoom > 100) do ZoomOutButton_Click(null, null); while (Zoom != 100);
            string filename = openFileDialog.FileName;
            using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                MapBytes = reader.ReadBytes(reader.ReadInt32());

                UniqueMarkers.Clear();
                int ucount = reader.ReadInt32();
                for (int u = 0; u < ucount; u++)
                {
                    UniqueMarkers.Add(new UniqueMarker
                    {
                        Name = reader.ReadString(),
                        ImageBytes = reader.ReadBytes(reader.ReadInt32()),
                        IsConnectingLink = reader.ReadBoolean()
                    });
                }

                Markers.Clear(); SelectedMarker = null;
                int mcount = reader.ReadInt32();
                for (int m = 0; m < mcount; m++)
                {
                    Markers.Add(new Marker
                    {
                        Type = UniqueMarkers[reader.ReadInt32()],
                        Number = reader.ReadInt32(),
                        X = reader.ReadDouble(),
                        Y = reader.ReadDouble()
                    });
                }

                Lines.Clear();
                int lcount = reader.ReadInt32();
                for (int l = 0; l < lcount; l++)
                {
                    int ind1 = reader.ReadInt32(), num1 = reader.ReadInt32();
                    int ind2 = reader.ReadInt32(), num2 = reader.ReadInt32();
                    Lines.Add(new Line(
                        Markers.Where(m => m.Number == num1 && m.Type == UniqueMarkers[ind1]).ToList()[0],
                        Markers.Where(m => m.Number == num2 && m.Type == UniqueMarkers[ind2]).ToList()[0]));
                }

                RouteLines.Clear();
                int rcount = reader.ReadInt32();
                for (int r = 0; r < rcount; r++)
                {
                    int ind1 = reader.ReadInt32(), num1 = reader.ReadInt32();
                    int ind2 = reader.ReadInt32(), num2 = reader.ReadInt32();
                    RouteLines.Add(new Line(
                        Markers.Where(m => m.Number == num1 && m.Type == UniqueMarkers[ind1]).ToList()[0],
                        Markers.Where(m => m.Number == num2 && m.Type == UniqueMarkers[ind2]).ToList()[0]));
                }
                lineColorComboBox.SelectedIndex = reader.ReadInt32();
                routeLineColorComboBox.SelectedIndex = reader.ReadInt32();
            }
            blockGrid.Visibility = Visibility.Collapsed;
        }

        private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mode == Mode.None)
            {
                Cursor = Cursors.Hand;
                mode = Mode.SelectMarker2;
                marker1 = (sender as Button).Tag as Marker;
                RouteButtonText = "Choose Marker 2";
            }
            else if (mode == Mode.SelectMarker2)
            {
                marker2 = (sender as Button).Tag as Marker;
                if (marker1 != marker2 && !marker1.Vertices.Contains(marker2) && !marker2.Vertices.Contains(marker1))
                {
                    mode = Mode.None;
                    Lines.Add(new Line(marker1, marker2));
                    Cursor = Cursors.Arrow;
                    RouteButtonText = "Create Route";
                }
            }
        }

        private void FindRouteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMarker == null) return;
            var rt = SelectedMarker.GetRoute(out double total);
            Total = total;
            if (rt.Count < 2) return;
            RouteLines.Clear();
            for (int i = 0; i < rt.Count - 1; i++)
                RouteLines.Add(new Line(rt[i], rt[i + 1], routeLines: true));
        }
    }
    public class Colour
    {
        public string Name { get; set; }
        public Color Color { get; set; }
    }
    public class ColourList : List<Colour>
    {
        public void Add(string name, Color color)
        {
            Add(new Colour
            {
                Name = name,
                Color = color
            });
        }
    }
    public class UniqueMarker : INotifyPropertyChanged
    {
        private bool _isConnectingLink;

        public byte[] ImageBytes { get; set; }
        public BitmapImage Image
        {
            get
            {
                if (ImageBytes == null || ImageBytes.Length == 0) return new BitmapImage();
                using (var ms = new MemoryStream(ImageBytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
            }
        }
        public string Name { get; set; }
        public bool IsConnectingLink
        {
            get { return _isConnectingLink; }
            set
            {
                if (_isConnectingLink != value)
                {
                    _isConnectingLink = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
    public class UniqueMarkerCollection : ObservableCollection<UniqueMarker> { }

    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (Thickness)value;
            return new Thickness(val.Left - 24, val.Top - 24, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
