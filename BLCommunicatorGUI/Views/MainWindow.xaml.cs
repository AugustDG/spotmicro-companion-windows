using System.Windows;
using BLCommunicatorGUI.ViewModels;

namespace BLCommunicatorGUI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            DataContext = new MainWindowVm(this);
        }
    }
}