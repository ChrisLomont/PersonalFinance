using System.Windows;
using System.Windows.Input;
using Lomont.PersonalFinance.ViewModel;

namespace Lomont.PersonalFinance.View
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

        void GraphVariablesDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm.AddVariable();
        }
        void BoundGraphVariablesDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm.RemoveVariable();
        }
    }
}
