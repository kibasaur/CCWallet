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
using cf = CountryFlag;

namespace CCWallet
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

        private void Select_Currency(object sender, RoutedEventArgs e)
        {
            var countryCode = ((sender as MenuItem)?.Header as cf.CountryFlag).Code;
            HeadFlag.Code = countryCode;
            var menuItems = FiatMenu.Items;
            foreach (MenuItem item in menuItems) 
            {
                item.IsChecked = false;
            }
            (sender as MenuItem).IsChecked = true;
        }
    }
}
