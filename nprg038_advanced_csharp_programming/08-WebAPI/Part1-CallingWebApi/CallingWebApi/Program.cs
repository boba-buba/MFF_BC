using System.Text.Json;
using Refit;
/*
const string DefaultAddress = "https://b2c.cpost.cz";

Console.Write($"Enter web API address ({DefaultAddress} is default): ");
string? address = Console.ReadLine();
if (string.IsNullOrEmpty(address)) {
    address = DefaultAddress;
}

Console.WriteLine("+++ via HttpClient & JsonSerializer:");

var httpClient = new HttpClient();
var json = httpClient.GetStringAsync(address + "/services/PostCode/getDataAsJson?cityOrPart=Sokolov&nameStreet=Karla+Hynka+Machy").Result;
Console.WriteLine(json);
Console.WriteLine();

var items = JsonSerializer.Deserialize<IReadOnlyList<PostCodeItem>>(json);
Console.WriteLine("Item 0:");
Console.WriteLine(items![0]);
Console.WriteLine();

var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
items = JsonSerializer.Deserialize<IReadOnlyList<PostCodeItem>>(json, options);
Console.WriteLine("Item 0:");
Console.WriteLine(items![0]);
Console.WriteLine();


    Console.WriteLine("+++ via Refit:");

    var postApi = RestService.For<ICeskaPostaWebApi>(address);

    var items = postApi.GetPostCodesByCityAndStreetAsync("Praha", "Patkova").Result;
    PrintPostCodeItems(items);
    items = postApi.GetPostCodesByCityAndStreetAsync("Sokolov", "Karla Hynka Machy").Result;
    PrintPostCodeItems(items);
    items = postApi.GetPostCodesByCityAndStreetAsync("Praha", "Malostranske namesti").Result;
    PrintPostCodeItems(items);

    static void PrintPostCodeItems(IEnumerable<PostCodeItem> items)
    {
        foreach (var item in items)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine();
    }
*/


Console.WriteLine("~~~~~ Coinbase Api via Refit ~~~~~");

const string DefaultCoinBaseAddress = "https://api.coinbase.com";
Console.Write($"Enter web API address ({DefaultCoinBaseAddress} is default): ");
string? coinbaseAddress = Console.ReadLine();
if (string.IsNullOrEmpty(coinbaseAddress))
{
    coinbaseAddress = DefaultCoinBaseAddress;
}

var coinBasePostApi = RestService.For<ICoinBaseWebApi>(coinbaseAddress);

// currencies request example
var coinBaseItems = coinBasePostApi.GetKnownCurrenciesAsync().Result;
PrintCoinBaseCurrencies(coinBaseItems);

// TODO: Add example using Coinbase API via Refit to fetch and print all currencies
// TODO: Add example using Coinbase API via Refit to fecht and print all exchange rates for:
//		 EUR
//       CZK

var coinBaseRates = coinBasePostApi.GetExchangeRateAsync("EUR").Result;
PrintCoinBaseRates(coinBaseRates);

coinBaseRates = coinBasePostApi.GetExchangeRateAsync("CZK").Result;
PrintCoinBaseRates(coinBaseRates);
static void PrintCoinBaseRates(CoinBaseRates rates)
{
    Console.WriteLine(rates.Data.Currency);
    foreach (var pair in rates.Data.Rates)
    {
        Console.WriteLine(pair.Key + " " + pair.Value);
    }
    Console.WriteLine();
    
}
static void PrintCoinBaseCurrencies(CoinBaseCurrencies items)
{
    foreach (var item in items.Data)
    {
        Console.WriteLine(item);
    }
    Console.WriteLine();
}

Console.WriteLine();

// TODO: Add type(s) for Coinbase entities
public record Currency(string Id, string Name, string Min_size);
public record ExchangeRate(string Currency, Dictionary<string, string> Rates);

public record CoinBaseCurrencies(List<Currency> Data); // those type were added, bc root element v jsonu je data

public record CoinBaseRates(ExchangeRate Data);


// TODO: Add interface for Coinbase methods:
// TODO: Add method for "currencies" method
// TODO: Add method for "exchange-rates" method

public interface ICoinBaseWebApi
{
    [Get("/v2/currencies")]
    Task<CoinBaseCurrencies> GetKnownCurrenciesAsync();

    [Get("/v2/exchange-rates?currency={currencyId}")]
    Task <CoinBaseRates> GetExchangeRateAsync(string currencyId);
}

