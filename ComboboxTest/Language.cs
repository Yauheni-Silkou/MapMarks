using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ComboboxTest
{
        public class Language : INotifyPropertyChanged
        {
            private string _languageValue;
            private string _languageId;

            public string LanguageValue
            {
                get
                {
                    return _languageValue;
                }
                set
                {
                    _languageValue = value;
                    OnPropertyChanged();
                }
            }

            public string LanguageId
            {
                get
                {
                    return _languageId;
                }
                set
                {
                    _languageId = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
}
