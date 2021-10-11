using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BLCommunicatorGUI.ViewModels
{
    /// <summary>
    /// Base View Model class to simplify implementation in other View Models. 
    /// </summary>
    /// <typeparam name="T">View class.</typeparam>
    public class BaseViewModel<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected readonly T ViewWindow;

        protected BaseViewModel(T window)
        {
            ViewWindow = window;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}