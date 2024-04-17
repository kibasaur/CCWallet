using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Classes
{
    public class Wallet : INotifyPropertyChanged
    {

        public Wallet() { }
        public Wallet(string WalletName) 
        {
            Name = WalletName;
            Id = Guid.NewGuid();
            Cryptos = new List<CryptoInstance>();
            FiatBalance = 0;
            FiatBalanceStr = "0";
        }

        [Required]
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<CryptoInstance> Cryptos { get; set; }
        public double FiatBalance { get; set; }

        private string _FiatBalanceStr;
        public string FiatBalanceStr 
        {
            get 
            {
                return _FiatBalanceStr;
            } 
            set
            {
                _FiatBalanceStr = value;
                OnPropertyChanged("FiatBalanceStr");
            } 

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }   
        
    }
}
