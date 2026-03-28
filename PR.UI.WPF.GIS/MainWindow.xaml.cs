using Craft.ViewModels.Geometry2D.Scrolling;
using System;
using System.Collections.Generic;
using System.Linq;
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
using PR.ViewModel.GIS;

namespace PR.UI.WPF.GIS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(
            object sender, 
            RoutedEventArgs e)
        {
            // Læg lige mærke til dette kald, hvor der bruges en Dispatcher - det er nødvendigt, når man
            // "opdaterer UI elementer fra en anden tråd end main tråd"
            Dispatcher.Invoke(async () =>
            {
                await ViewModel.Initialize();
                await ViewModel.AutoFindIfEnabled();
            });
        }
    }
}
