namespace Futures
{
	using System.Collections.Generic;

	using Newtonsoft.Json;

	public class FuturePosition
	{
		[JsonProperty(PropertyName = "result")]
		public bool Result { get; set; }

		[JsonProperty(PropertyName = "holding")]
		public List<Holding> Holding { get; set; }

		[JsonProperty(PropertyName = "force_liqu_price")]
		public string ForceLiquPrice { get; set; }
	}

	public class Holding
	{
		[JsonProperty(PropertyName = "buy_amount")]
		public int BuyAmount { get; set; }

		[JsonProperty(PropertyName = "buy_available")]
		public int BuyAvailable { get; set; }

		[JsonProperty(PropertyName = "buy_price_avg")]
		public double BuyPriceAvg { get; set; }

		[JsonProperty(PropertyName = "buy_price_cost")]
		public double BuyPriceCost { get; set; }

		[JsonProperty(PropertyName = "buy_profit_real")]
		public double BuyProfitReal { get; set; }

		[JsonProperty(PropertyName = "contract_id")]
		public long ContractId { get; set; }

		[JsonProperty(PropertyName = "contract_type")]
		public string ContractType { get; set; }

		[JsonProperty(PropertyName = "create_date")]
		public long CreateDate { get; set; }

		[JsonProperty(PropertyName = "lever_rate")]
		public int LeverRate { get; set; }

		[JsonProperty(PropertyName = "sell_amount")]
		public int SellAmount { get; set; }

		[JsonProperty(PropertyName = "sell_available")]
		public int SellAvailable { get; set; }

		[JsonProperty(PropertyName = "sell_price_avg")]
		public int SellPriceAvg { get; set; }

		[JsonProperty(PropertyName = "sell_price_cost")]
		public int SellPriceCost { get; set; }

		[JsonProperty(PropertyName = "sell_profit_real")]
		public int SellProfitReal { get; set; }

		[JsonProperty(PropertyName = "symbol")]
		public string Symbol { get; set; }
	}
}