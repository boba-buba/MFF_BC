#pragma once
#ifndef data_h
#include <vector>
#include <memory>
#include <iostream>
#include "gumbo.h"
#include <map>
#include "DataRetrieval.h"
#include <syslog.h>



class AllStocks {
public:
	using D = std::unique_ptr<Data>;
	AllStocks()
	{
		const char* spy = "https://finance.yahoo.com/quote/SPY/history?p=SPY";
		Standard SPY(spy);
		SPY.GetData();
		stocks.emplace( "SPY", std::make_unique<Standard>(SPY) );

	}
	void ShowCompanies()
	{
		for (auto it = stocks.begin(); it != stocks.end(); ++it)
		{
			if (it->first != "SPY")
				std::cout << it->first << std::endl;
		}
	}

	int RemoveCompany(std::string& ticker)
	{
		syslog(LOG_INFO, "AllStocks::RemoveCompany");
		return stocks.erase(ticker);
	}

	auto findStockInMem(std::string& ticker)
	{
		syslog(LOG_INFO, "AllStocks::findStockInMem");
		return stocks.find(ticker);
	}

	bool StockInMem(std::string& ticker)
	{
		syslog(LOG_INFO, "AllStocks::StockInMem");
		auto it = stocks.find(ticker);
		if (it != stocks.end())
		{
			return true;
		}
		return false;
	}

	auto GetStock(std::string& ticker)
	{
		syslog(LOG_INFO, "AllStocks::GetStock");
		if (StockInMem(ticker))
		{
			syslog(LOG_INFO, "Data with name %s are found in memory\n", ticker);
			auto it = findStockInMem(ticker);
			return it;
		}
		else
		{
			syslog(LOG_NOTICE, "Data with name %s are not in memory\n", ticker);
			throw std::runtime_error("Company Data not in Memory");
		}
	}

	void addNewStock(std::string& t)
	{
		syslog(LOG_INFO, "AllStocks::addNewStock");
		printf("%s\n", t);
		std::string historyLink = link(t, history);
		std::string analysisLink = link(t, analysis);

		Stock s(historyLink.c_str(), analysisLink.c_str());

		s.GetData();
		stocks.emplace(t, std::make_unique<Stock>(std::move(s)));

		syslog(LOG_INFO, "New stock %s was added successfully\n", t);
	}

	void SetTicker(std::string& t)
	{
		ticker = t;
	}

	std::string link(std::string& name, std::string& type)
	{
		return urlA + name + type + urlB + name;
	}
private:
	std::string urlA = "https://finance.yahoo.com/quote/";
	std::string ticker = "";
	std::string history = "/history"; //history
	std::string analysis = "/analysis";
	std::string urlB = "?p=";
	std::string srcURLhistory = ""; // urlA + ticker + analysis + urlB + ticker;
	std::string srcURLanalysis = "";
	std::map<std::string, D> stocks;

};


#endif // !data_h
