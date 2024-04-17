using CCWallet.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
using Classes;
using Country_Flag = CountryFlag.CountryFlag;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text.RegularExpressions;

namespace CCWallet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields and init

        private string _connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
        
        private ApiQuery _apiManager = new ApiQuery();
        public ObservableCollection<Country_Flag> currentCurrency { get; set; }
        public ObservableCollection<Wallet> wallets { get; set; }
        public ObservableCollection<CryptoInstance> walletCryptos { get; set; }
        
        public string currentFiatSymbol = "$";
        
        ICollectionView listSrc;


        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }   

        private Wallet _currentWallet;
        public Wallet currentWallet {
            get
            {
                return _currentWallet;
            }
            set
            {
                _currentWallet = value;
                OnPropertyChanged("currentWallet");
            }
        }

        private string _eAddBalance;
        public string eAddBalance
        {
            get
            {
                return _eAddBalance;
            }
            set
            {
                _eAddBalance = value;
                OnPropertyChanged("eAddBalance");
            }
        }

        private string _eSubBalance;
        public string eSubBalance
        {
            get
            {
                return _eSubBalance;
            }
            set
            {
                _eSubBalance = value;
                OnPropertyChanged("eSubBalance");
            }
        }

        private string _addCryptoStr;
        public string addCryptoStr
        {
            get
            {
                return _addCryptoStr;
            }
            set
            {
                _addCryptoStr = value;
                OnPropertyChanged("addCryptoStr");
            }
        }
        private string _removeValue;
        public string removeValue
        {
            get
            {
                return _removeValue;
            }
            set
            {
                _removeValue = value;
                OnPropertyChanged("removeValue");
            }
        }

        private string _promptLabel;
        public string promptLabel
        {
            get
            {
                return _promptLabel;
            }
            set
            {
                _promptLabel = value;
                OnPropertyChanged("promptLabel");
            }
        }

        private Dictionary<string, string> currencySymbols = new Dictionary<string, string>
        {
            { "USD", "$" },
            { "EUR", "€" },
            { "JPY", "¥" },
            { "GBP", "£" },
            { "CNY", "¥" },
            { "AUD", "$" },
            { "CAD", "$" },
            { "CHF", "₣" },
            { "SEK", "kr" },
            { "NOK", "kr" },
        };

        public List<MenuItem> currencyFlags = new List<MenuItem>
        {
            new MenuItem() { Header = new Country_Flag() { Name = "USD", Width = 30, Code = CountryFlag.CountryCode.US, Margin = new Thickness(0,3,0,3)}, IsChecked = true},
            new MenuItem() { Header = new Country_Flag() { Name = "EUR", Width = 30, Code = CountryFlag.CountryCode.EU, Margin = new Thickness(0,3,0,3)} },
            new MenuItem() { Header = new Country_Flag() { Name = "JPY", Width = 30, Code = CountryFlag.CountryCode.JP, Margin = new Thickness(0,3,0,3)} },
            new MenuItem() { Header = new Country_Flag() { Name = "GBP", Width = 30, Code = CountryFlag.CountryCode.GB, Margin = new Thickness(0,3,0,3)} },
            new MenuItem() { Header = new Country_Flag() { Name = "CNY", Width = 30, Code = CountryFlag.CountryCode.CN, Margin = new Thickness(0,3,0,3)} },
            new MenuItem() { Header = new Country_Flag() { Name = "AUD", Width = 30, Code = CountryFlag.CountryCode.AU, Margin = new Thickness(0,3,0,3)} },
            new MenuItem() { Header = new Country_Flag() { Name = "CAD", Width = 30, Code = CountryFlag.CountryCode.CA, Margin = new Thickness(0,3,0,3)} },
            new MenuItem() { Header = new Country_Flag() { Name = "CHF", Width = 30, Code = CountryFlag.CountryCode.CH, Margin = new Thickness(0,3,0,3)} },
            new MenuItem() { Header = new Country_Flag() { Name = "SEK", Width = 30, Code = CountryFlag.CountryCode.SE, Margin = new Thickness(0,3,0,3)} },
            new MenuItem() { Header = new Country_Flag() { Name = "NOK", Width = 30, Code = CountryFlag.CountryCode.NO, Margin = new Thickness(0,3,0,3)} }
        };

        private void Setup_Wallet_Collection()
        {
            // Could not make it work in XAML for some reason...
            CompositeCollection compCollection = new CompositeCollection();
            CollectionContainer walletContainer = new CollectionContainer();
            wallets = new ObservableCollection<Wallet>(wallets.OrderBy(w => int.TryParse(w.Name, out _) ? int.Parse(w.Name.Split(" ")[0]) : int.MaxValue).ThenBy(w => w.Name));

            if (wallets.Count > 0)
            {
                currentWallet = wallets[0];
                Update_Wallet_Values();
            }
            else
            {
                ShowWallet.Visibility = Visibility.Hidden;
                AddCryptoGrid.Visibility = Visibility.Hidden;
                NoWallets.Visibility = Visibility.Visible;
            }

            walletContainer.Collection = wallets;
            compCollection.Add(walletContainer);

            if (wallets.Count > 0) // To add separator when menu has more than 
                compCollection.Add(new Separator());

            compCollection.Add(new MenuItem() { Header = "New Wallet...", Name = "static" });

            menuWallet.ItemsSource = compCollection;
        }
        #endregion

        #region Constructor
        public MainWindow()
        {
            wallets = new ObservableCollection<Wallet>();
            walletCryptos = new ObservableCollection<CryptoInstance>();
            currentCurrency = new ObservableCollection<Country_Flag>
            {
                new Country_Flag() {Name = "USD", Width = 30, Code = CountryFlag.CountryCode.US, Margin = new Thickness(0,3,0,3)}
            };

            AddHandler(FrameworkElement.MouseDownEvent, new MouseButtonEventHandler(MouseButtonDownHandler), true);
            AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(HeaderClick));

            _apiManager.GetApiData(currentCurrency[0].Name, currentFiatSymbol);

            if (_apiManager.Error != "")
                return;

            InitializeComponent();

            FiatMenu.ItemsSource = currencyFlags;
            WalletLV.ItemsSource = walletCryptos;
            MarketLV.ItemsSource = _apiManager.AllCryptos;
            listSrc = CollectionViewSource.GetDefaultView(MarketLV.ItemsSource);
            listSrc.SortDescriptions.Clear();
            listSrc.SortDescriptions.Add(new SortDescription("Rank", ListSortDirection.Ascending));
            listSrc.Filter = MarketFilter;
            listSrc = CollectionViewSource.GetDefaultView(WalletLV.ItemsSource);
            listSrc.SortDescriptions.Clear();
            listSrc.SortDescriptions.Add(new SortDescription("Rank", ListSortDirection.Ascending));


            using (var db = new CCWalletContext(_connectionString))
            {
                if (!db.Database.CanConnect())
                {
                    try
                    {
                        db.Database.EnsureCreated();
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("There was an exception: " + ex.GetBaseException().ToString());
                    }
                }
                else
                {
                    foreach (var wallet in db.Wallets)
                    {
                        wallets.Add(wallet);
                    }

                }    
            }
            Setup_Wallet_Collection();
        }
        #endregion

        #region Menu functions

        private void Refresh_Market(object sender, RoutedEventArgs e)
        {
            _apiManager.GetApiData(currentCurrency[0].Name, currencySymbols[currentCurrency[0].Name]);
            string err = _apiManager.Error;
            if (err != "")
            {
                eAddBalance = err;
                eSubBalance = err;
                return;
            }
            AddCryptoGrid.Visibility = Visibility.Hidden;
            addCryptoStr = "";
        }

        private void Refresh_Wallet(object sender, RoutedEventArgs e)
        {
            ButtonGridWallets.Visibility = Visibility.Hidden;
            if (currentWallet == null)
                return;
            Update_Wallet_Values();
        }

        private void Wallet_Click(object sender, RoutedEventArgs e)
        {
            // if/else on whether or not the collection was clicked or the static item
            if (((MenuItem)e.Source).Name != null && ((MenuItem)e.Source).Name != "")
            {
                New_Wallet_Prompt(sender, e);
            }
            else
            {
                Open_Wallet(sender, e);
            }
        }

        private void Select_Currency(object sender, RoutedEventArgs e)
        {
            var countryCode = ((sender as MenuItem)?.Header as Country_Flag).Code;
            Country_Flag cf = currencyFlags.First(f => ((Country_Flag)f.Header).Code == countryCode).Header as Country_Flag;

            // Deep copy
            currentCurrency[0] = new Country_Flag() { Name = cf.Name, Width = cf.Width, Code = cf.Code, Margin = cf.Margin };
            currentFiatSymbol = currencySymbols[cf.Name];

            var menuItems = FiatMenu.Items;

            foreach (MenuItem item in menuItems)
            {
                item.IsChecked = false;
            }

            ((MenuItem)sender).IsChecked = true;

            Refresh_Market(sender, e);
            Refresh_Wallet(sender, e);
        }
        #endregion

        #region Wallet functions

        private void Cancel_Prompt(object sender, RoutedEventArgs e)
        {
            ConfirmPrompt.Visibility = Visibility.Hidden;
            ConfirmMask.Visibility = Visibility.Hidden;
            NewWalletStackPanel.Visibility = Visibility.Hidden;
            DeleteWalletStackPanel.Visibility = Visibility.Hidden;
            MenuPanel.IsEnabled = true;
            ContentGrid.IsEnabled = true;
        }

        private void Create_Wallet(object sender, RoutedEventArgs e)
        {
            if (WalletName.Text == null || WalletName.Text == "")
            {
                promptLabel = "Please Enter a Name For Your New Wallet!";
                return;
            }

            if (wallets.Any(w => w.Name == WalletName.Text))
            {
                promptLabel = "There is Already a Wallet With That Name!";
                return;
            }


            Wallet wallet = new Wallet(WalletName.Text);
            currentWallet = wallet;
            Update_Db_Wallets();
            wallets.Add(wallet);
            Update_Wallet_Cryptos();

            if (wallets.Count == 1)
            {
                Setup_Wallet_Collection();
                ShowWallet.Visibility = Visibility.Visible;
                NoWallets.Visibility = Visibility.Hidden;
            }
            else
            {
                var temp = new ObservableCollection<Wallet>(wallets.OrderBy(w => int.TryParse(w.Name, out _) ? int.Parse(w.Name.Split(" ")[0]) : int.MaxValue).ThenBy(w => w.Name));
                wallets.Clear();
                foreach (Wallet w in temp)
                    wallets.Add(w);
            }
            ButtonGridWallets.Visibility = Visibility.Hidden;
            ConfirmPrompt.Visibility = Visibility.Hidden;
            ConfirmMask.Visibility = Visibility.Hidden;
            NewWalletStackPanel.Visibility = Visibility.Hidden;

            MenuPanel.IsEnabled = true;
            ContentGrid.IsEnabled = true;
        }

        private void Delete_Wallet(object sender, RoutedEventArgs e)
        {
            wallets.Remove(currentWallet);
            ButtonGridWallets.Visibility = Visibility.Hidden;

            using (var db = new CCWalletContext(_connectionString))
            {
                db.Wallets.Remove(currentWallet);
                db.SaveChanges();
            }

            if (wallets.Count > 0)
            {
                currentWallet = wallets[0];
                Update_Wallet_Values();
            }
            else
            {
                Setup_Wallet_Collection();
                currentWallet = null;
                ShowWallet.Visibility = Visibility.Hidden;
                NoWallets.Visibility = Visibility.Visible;
            }
            Cancel_Prompt(sender, e);
        }

        private void Delete_Wallet_Prompt(object sender, RoutedEventArgs e)
        {
            promptLabel = "Delete " + currentWallet.Name + "?";
            ConfirmPrompt.Visibility = Visibility.Visible;
            ConfirmMask.Visibility = Visibility.Visible;
            DeleteWalletStackPanel.Visibility = Visibility.Visible;
            MenuPanel.IsEnabled = false;
            ContentGrid.IsEnabled = false;
        }

        private void New_Wallet_Prompt(object sender, RoutedEventArgs e)
        {
            WalletName.Text = null;
            promptLabel = "Wallet Name";
            ConfirmPrompt.Visibility = Visibility.Visible;
            ConfirmMask.Visibility = Visibility.Visible;
            NewWalletStackPanel.Visibility = Visibility.Visible;
            MenuPanel.IsEnabled = false;
            ContentGrid.IsEnabled = false;
            WalletName.Focus();
        }

        private void Open_Wallet(object sender, RoutedEventArgs e)
        {
            currentWallet = ((MenuItem)e.Source).DataContext as Wallet;
            Refresh_Wallet(sender, e);
        }
        private void Update_Wallet_Values()
        {
            currentWallet.FiatBalance = 0;
            for (int i = 0; i < currentWallet.Cryptos.Count; i++)
            {
                int idx = _apiManager.AllCryptos.IndexOf(_apiManager.AllCryptos.First(c => c.Id == currentWallet.Cryptos[i].Id));
                currentWallet.Cryptos[i].Rank = _apiManager.AllCryptos[idx].Rank;
                currentWallet.Cryptos[i].Value = currentWallet.Cryptos[i].Amount * _apiManager.AllCryptos[idx].Price;

                if (currentFiatSymbol == "kr")
                    currentWallet.Cryptos[i].ValueStr = currentWallet.Cryptos[i].Value.ToString("#,##0.0000").Replace(",",".") + currentFiatSymbol;
                else
                    currentWallet.Cryptos[i].ValueStr = currentFiatSymbol + currentWallet.Cryptos[i].Value.ToString("#,##0.0000").Replace(",",".");

                currentWallet.FiatBalance += currentWallet.Cryptos[i].Value;
            }

            if (currentFiatSymbol == "kr")
                currentWallet.FiatBalanceStr = currentWallet.FiatBalance.ToString("#,##0.0000").Replace(",",".") + currentFiatSymbol;
            else
                currentWallet.FiatBalanceStr = currentFiatSymbol + currentWallet.FiatBalance.ToString("#,##0.0000").Replace(",",".");

            wallets[wallets.IndexOf(wallets.First(w => w.Id == currentWallet.Id))] = currentWallet;
            Update_Wallet_Cryptos();
            Update_Db_Wallets();
        }

        private void Update_Db_Wallets()
        {
            using (var db = new CCWalletContext(_connectionString))
            {
                if (wallets.Any(w => w.Id == currentWallet.Id))
                    db.Wallets.Update(currentWallet);
                else
                    db.Wallets.Add(currentWallet);
                db.SaveChanges();
            }
        }

        private void Update_Wallet_Cryptos()
        {
            walletCryptos.Clear();
            foreach (CryptoInstance crypto in currentWallet.Cryptos)
                walletCryptos.Add(crypto);
        }

        #endregion

        #region ListView functions

        string _lastHeader = null;

        // Sorts columns
        private void HeaderClick(object sender, RoutedEventArgs e)
        {
            // Nullcheck to see if a header was clicked
            var columnHeader = e.OriginalSource as GridViewColumnHeader;
            if (columnHeader == null)
                return;

            // Sorts market or wallet depending on which ListView was clicked
            if ( ((ListView)e.Source).Name == "MarketLV" )  {
                listSrc = CollectionViewSource.GetDefaultView(MarketLV.ItemsSource);
            }
            else
            {
                listSrc = CollectionViewSource.GetDefaultView(WalletLV.ItemsSource);
            }

            string header = (string)(columnHeader.Column.DisplayMemberBinding as Binding).Path.Path;
            bool strSort = false;

            listSrc.SortDescriptions.Clear();

            if (header == "PriceStr")
            {
                header = "Price";
                strSort = true;
            }
            else if (header == "ValueStr")
            {
                header = "Value";
                strSort = true;
            }
            else if (header == "AmountStr")
            {
                header = "Amount";
                strSort = true;
            }

            if (strSort)
            {
                if (_lastHeader == header)
                    _lastHeader = null;
                else
                    _lastHeader = header;
            }


            if (header != _lastHeader)
            {
                listSrc.SortDescriptions.Add(new SortDescription(header, ListSortDirection.Ascending));
                if (!strSort)
                    _lastHeader = header;
            }
            else
            {
                listSrc.SortDescriptions.Add(new SortDescription(header, ListSortDirection.Descending));
                if (!strSort)
                    _lastHeader = null;
            }

            listSrc.Refresh();
            e.Handled = true;
        }

        private bool MarketFilter(object item)
        {
            if (String.IsNullOrEmpty(FilterMarket.Text))
                return true;
            else if (((item as CryptoInfo).Name.IndexOf(FilterMarket.Text, StringComparison.OrdinalIgnoreCase) >= 0))
                return true;
            else
                return ((item as CryptoInfo).Symbol.IndexOf(FilterMarket.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void FilterMarket_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(MarketLV.ItemsSource).Refresh();
        }

        private void MarketLV_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MarketLV.SelectedValue as CryptoInfo == null)
                return;
            addCryptoStr = (MarketLV.SelectedValue as CryptoInfo).Symbol;
            AddCryptoGrid.Visibility = Visibility.Visible;
            AmountToWallet.Focus();
            AmountToWallet.Text = "";
        }

        private void WalletLV_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (WalletLV.SelectedValue as CryptoInstance == null)
                return;
            ButtonGridWallets.Visibility = Visibility.Visible;
            WithdrawValue.Focus();
            Slide.Value = 50;
            removeValue = ((WalletLV.SelectedValue as CryptoInstance).Amount * Slide.Value * 0.01).ToString().Replace(",",".");
            WithdrawValue.CaretIndex = int.MaxValue;
        }
        #endregion

        #region Adding and Removing Crypto
        private void AddCrypto_Click(object sender, RoutedEventArgs e)
        {
            string str = AmountToWallet.Text.Replace(".", ",");
            CryptoInfo selectedCrypto = MarketLV.SelectedValue as CryptoInfo;

            if (currentWallet == null)
            {
                eAddBalance = "Please create a wallet!";
                return;
            }
            if (selectedCrypto == null)
            {
                eAddBalance = "Please select a crypto currency!";
                return;
            }
            if (str == null || str == "")
            {
                eAddBalance = "Please input an amount to add!";
                return;
            }

            double amount = double.Parse(str);

            if (currentWallet.Cryptos.Any(c => c.Name == selectedCrypto.Name))
            {
                int idx = currentWallet.Cryptos.IndexOf(currentWallet.Cryptos.First(c => c.Id == selectedCrypto.Id));
                currentWallet.Cryptos[idx].Rank = selectedCrypto.Rank;
                currentWallet.Cryptos[idx].Amount += amount;
                currentWallet.Cryptos[idx].Value = currentWallet.Cryptos[idx].Amount * selectedCrypto.Price;
                currentWallet.Cryptos[idx].AmountStr = currentWallet.Cryptos[idx].Amount.ToString("#,##0.0000").Replace(",",".");

                if (currentFiatSymbol == "kr")
                    currentWallet.Cryptos[idx].ValueStr = currentWallet.Cryptos[idx].Value.ToString("#,##0.0000").Replace(",",".") + currentFiatSymbol;
                else
                    currentWallet.Cryptos[idx].ValueStr = currentFiatSymbol + currentWallet.Cryptos[idx].Value.ToString("#,##0.0000").Replace(",",".");

                currentWallet.FiatBalance += amount * selectedCrypto.Price;
            }
            else
            {
                CryptoInstance crypto = new CryptoInstance() {
                    Id = selectedCrypto.Id,
                    Rank = selectedCrypto.Rank,
                    Name = selectedCrypto.Name,
                    Symbol = selectedCrypto.Symbol,
                    Amount = amount,
                    Value = amount * selectedCrypto.Price
                };

                crypto.AmountStr = amount.ToString("#,##0.0000").Replace(",",".");

                if (currentFiatSymbol == "kr")
                    crypto.ValueStr = crypto.Value.ToString("#,##0.0000").Replace(",",".") + currentFiatSymbol; 
                else
                    crypto.ValueStr = currentFiatSymbol + crypto.Value.ToString("#,##0.0000").Replace(",",".");

                currentWallet.FiatBalance += crypto.Value;
                currentWallet.Cryptos.Add(crypto);
            }

            if (currentFiatSymbol == "kr")
                currentWallet.FiatBalanceStr = currentWallet.FiatBalance.ToString("#,##0.0000").Replace(",",".") + currentFiatSymbol;
            else
                currentWallet.FiatBalanceStr = currentFiatSymbol + currentWallet.FiatBalance.ToString("#,##0.0000").Replace(",",".");

            Update_Db_Wallets();
            Update_Wallet_Cryptos();
            ButtonGridWallets.Visibility = Visibility.Hidden;
            eAddBalance = amount + " " + selectedCrypto.Symbol + " added to " + currentWallet.Name + "!";
        }

        private void AmountToWallet_TextChanged(object sender, TextChangedEventArgs e)
        {
            int idx = AmountToWallet.CaretIndex;
            AmountToWallet.Text = AmountToWallet.Text.Replace(",", ".");
            AmountToWallet.CaretIndex = idx;

        }

        private void Remove_Value(object sender, RoutedEventArgs e)
        {
            CryptoInstance selectedCrypto = WalletLV.SelectedValue as CryptoInstance;
            double amount = double.Parse(removeValue.Replace(".",","));
            double price = _apiManager.AllCryptos.First(c => c.Id == selectedCrypto.Id).Price;

            if (Slide.Value == 0)
            {
                eSubBalance = "Please select an amount greater than 0!";
                return;
            }

            if (Slide.Value == 100)
            {
                currentWallet.Cryptos.Remove(selectedCrypto);
                ButtonGridWallets.Visibility = Visibility.Hidden;
            }
            else
            {
                int idx = currentWallet.Cryptos.IndexOf(currentWallet.Cryptos.First(c => c.Id == selectedCrypto.Id));
                currentWallet.Cryptos[idx].Amount -= amount;
                currentWallet.Cryptos[idx].Value = currentWallet.Cryptos[idx].Amount * price;
                currentWallet.Cryptos[idx].AmountStr = currentWallet.Cryptos[idx].Amount.ToString("#,##0.0000").Replace(",", ".");

                if (currentFiatSymbol == "kr")
                    currentWallet.Cryptos[idx].ValueStr = currentWallet.Cryptos[idx].Value.ToString("#,##0.0000").Replace(",", ".") + currentFiatSymbol;
                else
                    currentWallet.Cryptos[idx].ValueStr = currentFiatSymbol + currentWallet.Cryptos[idx].Value.ToString("#,##0.0000").Replace(",", ".");

                currentWallet.FiatBalance -= amount * price;

                removeValue = (currentWallet.Cryptos[idx].Amount / 2).ToString().Replace(",",".");
            }
            eSubBalance = amount.ToString("#,##0.0000").Replace(",", ".") + " " + selectedCrypto.Symbol + " removed from " + currentWallet.Name;
            Update_Wallet_Values();
            WalletLV.SelectedValue = selectedCrypto;
        }

        private void String_Is_Number(object sender, TextCompositionEventArgs e)
        {

            if (e.Text == "\r")
                return;

            string pattern = "^(([1-9]{1}[0-9]*([,.]?[0-9]*)?)|(0{1}([,.]{1}[0-9]*)?))$";
            string input = ((TextBox)sender).Text.Remove(((TextBox)sender).SelectionStart, ((TextBox)sender).SelectionLength);
            input = input.Insert(((TextBox)sender).SelectionStart, e.Text);

            if (!Regex.IsMatch(input, pattern))
            {
                if (((TextBox)sender).Name == "WithdrawValue")
                    eSubBalance = "Please enter a double or int value!";
                else
                    eAddBalance = "Please enter a double or int value!";
                e.Handled = true;
            }
            else
            {
                eAddBalance = "";
                eSubBalance = "";

                if (((TextBox)sender).Name == "WithdrawValue" && (WalletLV.SelectedValue as CryptoInstance).Amount < double.Parse(input.Replace(".",",")))
                {
                    removeValue = (WalletLV.SelectedValue as CryptoInstance).Amount.ToString().Replace(",",".");
                    WithdrawValue.CaretIndex = int.MaxValue;
                    e.Handled = true;
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            for (int i = 0; i < 10; i++)
            {
                Key k1 = (Key)Enum.Parse(typeof(Key), ("NumPad" + i).ToString());
                Key k2 = (Key)Enum.Parse(typeof(Key), ("D" + i).ToString());
                if (Keyboard.IsKeyDown(k1) || Keyboard.IsKeyDown(k2))
                    return;
            }
            if (Keyboard.IsKeyDown(Key.OemPeriod) || Keyboard.IsKeyDown(Key.OemComma) ||
                    Keyboard.IsKeyDown(Key.Decimal) || Keyboard.IsKeyDown(Key.Back))
                return;

            removeValue = ((WalletLV.SelectedValue as CryptoInstance).Amount * Slide.Value * 0.01).ToString().Replace(",", ".");
            WithdrawValue.CaretIndex = int.MaxValue;
        }

        private void WithdrawValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            string str = WithdrawValue.Text.Replace(".", ",");

            int idx = WithdrawValue.CaretIndex;
            removeValue = str.Replace(",",".");
            WithdrawValue.CaretIndex = idx;


            if (removeValue == "" || removeValue == null)
                Slide.Value = 0;
            else if ((WalletLV.SelectedValue as CryptoInstance) != null && double.TryParse(str, out _))
            {
                double input = double.Parse(str);
                if ((WalletLV.SelectedValue as CryptoInstance).Amount < input)
                {
                    double amount = (WalletLV.SelectedValue as CryptoInstance).Amount;
                    removeValue = amount.ToString().Replace(",", ".");
                    Slide.Value = 100;
                    WithdrawValue.CaretIndex = int.MaxValue;
                }
                else
                {
                    Slide.Value = (input / (WalletLV.SelectedValue as CryptoInstance).Amount) * 100;
                }
            }

        }
        #endregion


        // Added this for additional feedback 
        private void MouseButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            eAddBalance = "";
            eSubBalance = "";
        }

        private void Enter_Pressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                switch (((TextBox)sender).Name)
                {
                    case "AmountToWallet":
                        AddCrypto_Click(new object(), new RoutedEventArgs());
                        break;
                    case "WalletName":
                        Create_Wallet(new object(), new RoutedEventArgs());
                        break;
                    case "WithdrawValue":
                        Remove_Value(new object(), new RoutedEventArgs());
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
