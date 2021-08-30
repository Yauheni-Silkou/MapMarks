using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ComboboxTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
    public class MainWindowViewModel : INotifyPropertyChanged
        {
            private Language _currentSelection;
            private string _labelText;

            public MainWindowViewModel()
            {
                CurrentSelection = Languages.First();
            }

            public List<Language> Languages { get; } = new List<Language>
        {
            new Language {LanguageValue = "English", LanguageId = "en"},
            new Language {LanguageValue = "German", LanguageId = "de"},
            new Language {LanguageValue = "Italian", LanguageId = "it"}
        };

            public Language CurrentSelection
            {
                get { return _currentSelection; }
                set
                {
                    _currentSelection = value;
                    LabelText = "Current language selected: " + _currentSelection.LanguageValue;
                    OnPropertyChanged(nameof(CurrentSelection));
                }
            }

            public string LabelText
            {
                get { return _labelText; }
                set
                {
                    _labelText = value;
                    OnPropertyChanged(nameof(LabelText));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
}
