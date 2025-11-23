
using System.Text;

/// <summary>
/// CoinBase request handler
/// </summary>
public class CurrencyExchangeRoutesHandler : ISimplisticRoutesHandler {

	/// <summary>
	/// Registers functions of the handler to process requests later. 
	/// </summary>
	/// <param name="routeMap">map to store functions</param>
    public void RegisterRoutes(RouteMap routeMap) {
        Console.WriteLine("+++ CurrencyExchangeRoutesHandler.RegisterRoutes() called.");
		// TODO: Register relevant routes in the routeMap
		routeMap.Map("/v2/currencies", GetCurrencies);
		routeMap.Map("/v2/exchange-rates", GetExchangeRatesFor);
    }

    // TODO: Add service methods implementing Coinbase service with example data
    //		 (stored in strong-typed objects - in similar manner as PostRoutesHandler does)
    // TODO: Add "currencies" method returning info about EUR, USD, CZK currencies

    /// <summary>
    /// Currencies request handler. 
    /// </summary>
    /// <returns>info about EUR, USD, CZK currencies</returns>
    public CoinBaseCurrencies GetCurrencies()
	{
		return new CoinBaseCurrencies(new List<Currency>()
		{
			new Currency("EUR", "Euro", "0.01"),
			new Currency("USD", "United States Dollar", "0.01"),
			new Currency("CZK", "Czech Koruna", "0.01"),
		});
	}

    // TODO: Add "exchange-rates" method, that is able to return exchange rates for EUR and for CZK

    /// <summary>
    /// Exchange rates request handler
    /// </summary>
    /// <param name="currency">Currency, for which exchange rates are retrieved</param>
    /// <returns>exchange rates for Currency</returns>
    public CoinBaseRates GetExchangeRatesFor(string currency)
	{
		return currency switch
		{
			"EUR" => new CoinBaseRates(new ExchangeRate(
					currency,
					new Dictionary<string, string> { { "EUR", "1.0" }, { "CZK", "25.168513" }, { "USD", "1.0676515425585898" } }
			)),
			"CZK" => new CoinBaseRates(new ExchangeRate(
					currency,
					new Dictionary<string, string> { { "EUR", "0.0397336320878641" }, { "CZK", "1.0" }, { "USD", "0.0424649180293685" } }
			)),
			_ => new CoinBaseRates(new ExchangeRate(currency, new Dictionary<string, string>()))
		};
	}
}

// TODO: Add additional types if necessary
// Were copied from CallingWebApi project for compatibility

/// <summary>
/// Class that represents currency entity.
/// </summary>
/// <param name="Id">id of currency (CZK,..)</param>
/// <param name="Name">Official name</param>
/// <param name="Min_size">Minimal smth</param>
public record Currency(string Id, string Name, string Min_size);


/// <summary>
/// Class that represents exchange rate of the Currency 
/// </summary>
/// <param name="Currency">Currency ID</param>
/// <param name="Rates">Dictionary that holds {currencyID: rate} pairs</param>
public record ExchangeRate(string Currency, Dictionary<string, string> Rates) 
{
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
		sb.Append(Currency);
		sb.Append('\n');
		foreach (var pair in Rates)
		{
			sb.Append(pair.Key);
			sb.Append(": ");
			sb.Append(pair.Value);
			sb.Append('\n');
		}
		return sb.ToString();
	}
}


/// <summary>
/// Helping class for storing multiple currencies data
/// </summary>
/// <param name="Data"></param>
public record CoinBaseCurrencies(List<Currency> Data)
{
    public override string ToString()
    {
		StringBuilder sb = new StringBuilder();
		foreach (var d in Data)
		{
			sb.Append(d.ToString());
			sb.Append('\n');
		}
        return sb.ToString();
    }
}


/// <summary>
/// Helping class for storing exchange rate data
/// </summary>
/// <param name="Data"></param>
public record CoinBaseRates(ExchangeRate Data);