using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Newtonsoft.Json;
using System.Windows.Input;
using Classes;

namespace CCWallet.Models
{
    public class CCWalletContext : DbContext
    {
        public DbSet<Wallet> Wallets { get; set; }

        public CCWalletContext()
        { }
        public CCWalletContext(string connectionString)
        {
            this.Database.SetConnectionString(connectionString);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=CryptoCurrencyWallet");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Wallet>()
                .Property(w => w.Cryptos)
                .HasConversion(
                    e => JsonConvert.SerializeObject(e),
                    e => JsonConvert.DeserializeObject<List<CryptoInstance>>(e)
                );
        }

    }
}