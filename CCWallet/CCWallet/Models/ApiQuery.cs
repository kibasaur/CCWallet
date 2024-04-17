using Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CCWallet.Models
{
    public class ApiQuery
    {
        public ApiQuery() 
        {
            _api_key = "05e0cf9f-7128-4d88-8598-e6e0f0066d3d";
            AllCryptos = new ObservableCollection<CryptoInfo>();
        }
        private string _api_key { get; set; }
        public string Error { get; set; }

        public ObservableCollection<CryptoInfo> AllCryptos { get; set; }

        public void GetApiData(string currency, string fiatSymbol)
        {
            try
            {
                AllCryptos.Clear();

                var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest");

                var queryString = HttpUtility.ParseQueryString(string.Empty);

                queryString["start"] = "1";
                queryString["limit"] = "5000";
                queryString["convert"] = currency;

                URL.Query = queryString.ToString();

                var client = new WebClient();
                client.Headers.Add("X-CMC_PRO_API_KEY", _api_key);
                client.Headers.Add("Accepts", "application/json");

                JObject jObject = JObject.Parse(client.DownloadString(URL.ToString()));
                var tokens = jObject["data"];

                bool skip = false;

                // This would probably look better with LINQ
                // Nested foreach loops to access properties from the API response
                foreach (var token in tokens)
                {
                    CryptoInfo crypto = new CryptoInfo();
                    //var smallTokens = token.Children().Children().Children().ToList();
                    var smallTokens = token.Children().ToList();
                    foreach (JProperty smallToken in smallTokens)
                    {
                        string switchString = smallToken.Name;
                        try
                        {

                            switch (switchString)
                            {
                                case "name":
                                    crypto.Name = (string)smallToken.First();
                                    break;
                                case "id":
                                    crypto.Id = (int)smallToken.First();
                                    break;
                                case "symbol":
                                    crypto.Symbol = (string)smallToken.First();
                                    break;
                                case "cmc_rank":
                                    crypto.Rank = (int)smallToken.First();
                                    break;
                                case "quote":
                                    crypto.Price = (double)smallToken.First().Children().First().First().Children().First(x => ((JProperty)x).Name == "price");
                                    if (fiatSymbol == "kr")
                                        crypto.PriceStr = crypto.Price.ToString("#,##0.0000000000").Replace(",", ".") + fiatSymbol;
                                    else
                                        crypto.PriceStr = fiatSymbol + crypto.Price.ToString("#,##0.0000000000").Replace(",", ".");
                                    break;
                                default:
                                    break;

                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("There was an exception:" + ex.Message);
                            skip = true;
                        }
                    }
                    if (!skip)
                        AllCryptos.Add(crypto);
                    skip = false;
                }
                Error = "";
            }
            catch (Exception e)
            {
                // Quickfix that probably doesn't look so good in the app
                Error = e.ToString();
            }
        }
    }
}
