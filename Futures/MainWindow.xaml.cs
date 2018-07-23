namespace Futures
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Threading.Tasks;
	using System.Windows;
	using System.ComponentModel;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	internal partial class MainWindow
	{
		private readonly BackgroundWorker worker;

		public MainWindow()
		{
			InitializeComponent();

			worker = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = false };
			worker.DoWork += WorkerDoWork;
		}

		public async void WorkerDoWork(object sender, DoWorkEventArgs e)
		{
			while (true)
			{
				if (worker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}

				#region CurrencyInfo

				var marketDepth = Convert.ToInt32(ConfigurationManager.AppSettings["MarketDepth"]);

				var btcTwResult = await FutureDepthBtcAsync("btc_usd", "this_week", marketDepth);
				var btcNwResult = await FutureDepthBtcAsync("btc_usd", "next_week", marketDepth);
				var btcQResult = await FutureDepthBtcAsync("btc_usd", "quarter", marketDepth);

				var ltcTwResult = await FutureDepthAsync("ltc_usd", "this_week", marketDepth);
				var ltcNwResult = await FutureDepthAsync("ltc_usd", "next_week", marketDepth);
				var ltcQResult = await FutureDepthAsync("ltc_usd", "quarter", marketDepth);

				var ethTwResult = await FutureDepthAsync("eth_usd", "this_week", marketDepth);
				var ethNwResult = await FutureDepthAsync("eth_usd", "next_week", marketDepth);
				var ethQResult = await FutureDepthAsync("eth_usd", "quarter", marketDepth);

				var etcTwResult = await FutureDepthAsync("etc_usd", "this_week", marketDepth);
				var etcNwResult = await FutureDepthAsync("etc_usd", "next_week", marketDepth);
				var etcQResult = await FutureDepthAsync("etc_usd", "quarter", marketDepth);

				var bchTwResult = await FutureDepthAsync("bch_usd", "this_week", marketDepth);
				var bchNwResult = await FutureDepthAsync("bch_usd", "next_week", marketDepth);
				var bchQResult = await FutureDepthAsync("bch_usd", "quarter", marketDepth);

				var btgTwResult = await FutureDepthAsync("btg_usd", "this_week", marketDepth);
				var btgNwResult = await FutureDepthAsync("btg_usd", "next_week", marketDepth);
				var btgQResult = await FutureDepthAsync("btg_usd", "quarter", marketDepth);

				var xrpTwResult = await FutureDepthAsync("xrp_usd", "this_week", marketDepth);
				var xrpNwResult = await FutureDepthAsync("xrp_usd", "next_week", marketDepth);
				var xrpQResult = await FutureDepthAsync("xrp_usd", "quarter", marketDepth);

				var eosTwResult = await FutureDepthAsync("eos_usd", "this_week", marketDepth);
				var eosNwResult = await FutureDepthAsync("eos_usd", "next_week", marketDepth);
				var eosQResult = await FutureDepthAsync("eos_usd", "quarter", marketDepth);

				#endregion

				UpdateLabels(btcTwResult, btcNwResult, btcQResult
							,ltcTwResult, ltcNwResult, ltcQResult
							,ethTwResult, ethNwResult, ethQResult
							,etcTwResult, etcNwResult, etcQResult
							,bchTwResult, bchNwResult, bchQResult
							,btgTwResult, btgNwResult, btgQResult
							,xrpTwResult, xrpNwResult, xrpQResult
							,eosTwResult, eosNwResult, eosQResult);
			}		 
		}

		public void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				MessageBox.Show("Stopped.");
			}
			else
			{
				MessageBox.Show("Stopped:" + e.Result);
			}
		}

		private void StartClick(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(Cumulative.Text))
			{
				MessageBox.Show("Enter cumulative value.");
				return;
			}

			Start.IsEnabled = false;
			Stop.IsEnabled = true;
			Cumulative.IsEnabled = false;

			worker.RunWorkerAsync();
		}

		private void StopClick(object sender, RoutedEventArgs e)
		{
			worker.CancelAsync();

			Start.IsEnabled = true;
			Stop.IsEnabled = false;
			Cumulative.IsEnabled = true;
		}

		public async Task<FutureDepth> FutureDepthAsync(string symbol, string contractType, int size)
		{
			var paras = new Dictionary<string, string>
							{
								{ "symbol", symbol },
								{ "contract_type", contractType },
								{ "size", size.ToString() }
							};

			var url = ConfigurationManager.AppSettings["OkexUrl"] + ConfigurationManager.AppSettings["FutureDepthUrl"];
			CreateUrl(ref url, paras);

			var request = CreateGetRequest(url);
			var response = await GetResponseAsync(request);
			var data = await ReadResponseAsync(response);

			var futureDepth = new FutureDepth();
			var result = JObject.Parse(data);

			foreach (var itemList in result)
			{
				var reverseAsks = itemList.Value.Reverse();
				double cumulative = 0;

				// ReSharper disable once ConvertIfStatementToSwitchStatement
				if (itemList.Key == "asks")
				{
					foreach (var item in reverseAsks)
					{
						var amount = Math.Round(item.Last.Value<double>() * 10 / item.First.Value<double>(), 5);
						var ask = new FutureDepthDetail
						{
							Price = item.First.Value<double>(),
							Amount = amount,
							Cumulative = Math.Round(cumulative + amount, 5)
						};

						cumulative = ask.Cumulative;
						futureDepth.Asks.Add(ask);
					}
				}
				else if (itemList.Key == "bids")
				{
					foreach (var item in itemList.Value)
					{
						var amount = Math.Round(item.Last.Value<double>() * 10 / item.First.Value<double>(), 5);
						var bid = new FutureDepthDetail
						{
							Price = item.First.Value<double>(),
							Amount = amount,
							Cumulative = Math.Round(cumulative + amount, 5)
						};

						cumulative = bid.Cumulative;
						futureDepth.Bids.Add(bid);
					}
				}
			}

			return futureDepth;
		}

		public async Task<FutureDepth> FutureDepthBtcAsync(string symbol, string contractType, int size)
		{
			var paras = new Dictionary<string, string>
				            {
					            { "symbol", symbol },
					            { "contract_type", contractType },
					            { "size", size.ToString() }
				            };

			var url = ConfigurationManager.AppSettings["OkexUrl"] + ConfigurationManager.AppSettings["FutureDepthUrl"];
			CreateUrl(ref url, paras);

			var request = CreateGetRequest(url);
			var response = await GetResponseAsync(request);
			var data = await ReadResponseAsync(response);

			var futureDepth = new FutureDepth();
			var result = JObject.Parse(data);

			foreach (var itemList in result)
			{
				var reverseAsks = itemList.Value.Reverse();
				double cumulative = 0;

				// ReSharper disable once ConvertIfStatementToSwitchStatement
				if (itemList.Key == "asks")
				{
					foreach (var item in reverseAsks)
					{
						var amount = Math.Round(item.Last.Value<double>() * 100 / item.First.Value<double>(), 5);
						var ask = new FutureDepthDetail
							          {
								          Price = item.First.Value<double>(),
								          Amount = amount,
								          Cumulative = Math.Round(cumulative + amount, 5)
							          };

						cumulative = ask.Cumulative;
						futureDepth.Asks.Add(ask);
					}
				}
				else if (itemList.Key == "bids")
				{
					foreach (var item in itemList.Value)
					{
						var amount = Math.Round(item.Last.Value<double>() * 100 / item.First.Value<double>(), 5);
						var bid = new FutureDepthDetail
							          {
								          Price = item.First.Value<double>(),
								          Amount = amount,
								          Cumulative = Math.Round(cumulative + amount, 5)
							          };

						cumulative = bid.Cumulative;
						futureDepth.Bids.Add(bid);
					}
				}
			}

			return futureDepth;
		}

		public async Task<FuturePosition> FuturePositionAsync(string symbol, string contractType)
		{
			var paras = new Dictionary<string, string>
							{
								{ "symbol", symbol },
								{ "contract_type", contractType },
								{ "api_key", ConfigurationManager.AppSettings[ "apiPublicKey" ] }
							};
			AddSign(ref paras);

			var url = ConfigurationManager.AppSettings["OkexUrl"] + ConfigurationManager.AppSettings["FuturePositionUrl"];
			CreateUrl(ref url, paras);

			var request = CreatePostRequest(url);
			var response = await GetResponseAsync(request);
			var data = await ReadResponseAsync(response);

			return JsonConvert.DeserializeObject<FuturePosition>(data);
		}

		private static async Task<string> ReadResponseAsync(WebResponse response)
		{
			using (var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
			{
				return await reader.ReadToEndAsync();
			}
		}

		private static async Task<WebResponse> GetResponseAsync(WebRequest request)
		{
			var response = (HttpWebResponse)await request.GetResponseAsync();

			if (response.StatusCode != HttpStatusCode.OK)
			{
				MessageBox.Show($"{(int)response.StatusCode} ({response.ResponseUri})");
			}

			return response;
		}

		private static WebRequest CreatePostRequest(string url)
		{
			if (string.IsNullOrEmpty(url))
				throw new ArgumentException(@"Value cannot be null or empty.", nameof(url));

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = "application/json";
			request.Method = "POST";

			return request;
		}

		private static WebRequest CreateGetRequest(string url)
		{
			if (string.IsNullOrEmpty(url))
				throw new ArgumentException(@"Value cannot be null or empty.", nameof(url));

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = "application/json";
			request.Method = "GET";

			return request;
		}

		private static void AddSign(ref Dictionary<string, string> paras)
		{
			var sign = Md5Util.BuildMysignV1(paras, ConfigurationManager.AppSettings["apiPrivateKey"]);
			paras.Add("sign", sign);
		}

		private static void CreateUrl(ref string url, Dictionary<string, string> paras)
		{
			url = paras.Keys.Aggregate(url, (current, key) => current + (key == paras.Keys.First()
																? $"?{key}={paras[key]}"
																: $"&{key}={paras[key]}"));
		}

		private void UpdateLabels(FutureDepth btcTwResult, FutureDepth btcNwResult, FutureDepth btcQResult,
						  FutureDepth ltcTwResult, FutureDepth ltcNwResult, FutureDepth ltcQResult,
						  FutureDepth ethTwResult, FutureDepth ethNwResult, FutureDepth ethQResult,
						  FutureDepth etcTwResult, FutureDepth etcNwResult, FutureDepth etcQResult,
						  FutureDepth bchTwResult, FutureDepth bchNwResult, FutureDepth bchQResult,
						  FutureDepth btgTwResult, FutureDepth btgNwResult, FutureDepth btgQResult,
						  FutureDepth xrpTwResult, FutureDepth xrpNwResult, FutureDepth xrpQResult,
						  FutureDepth eosTwResult, FutureDepth eosNwResult, FutureDepth eosQResult)
		{
			Dispatcher.Invoke(() =>
			{
				double.TryParse(Cumulative.Text, out var cumul);

				// TODO: get AskBids in parallel thread
				#region AskBids

				var btcTwAskPrice = btcTwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / btcTwResult.Asks.First().Price);
				var btcNwAskPrice = btcNwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / btcNwResult.Asks.First().Price);
				var btcQAskPrice = btcQResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / btcQResult.Asks.First().Price);
				var btcTwBidsPrice = btcTwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / btcTwResult.Bids.First().Price);
				var btcNwBidsPrice = btcNwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / btcNwResult.Bids.First().Price);
				var btcQBidsPrice = btcQResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / btcQResult.Bids.First().Price);

				var ltcTwAskPrice = ltcTwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / ltcTwResult.Asks.First().Price);
				var ltcNwAskPrice = ltcNwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / ltcNwResult.Asks.First().Price);
				var ltcQAskPrice = ltcQResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / ltcQResult.Asks.First().Price);
				var ltcTwBidsPrice = ltcTwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / ltcTwResult.Bids.First().Price);
				var ltcNwBidsPrice = ltcNwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / ltcNwResult.Bids.First().Price);
				var ltcQBidsPrice = ltcQResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / ltcQResult.Bids.First().Price);

				var ethTwAskPrice = ethTwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / ethTwResult.Asks.First().Price);
				var ethNwAskPrice = ethNwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / ethNwResult.Asks.First().Price);
				var ethQAskPrice = ethQResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / ethQResult.Asks.First().Price);
				var ethTwBidsPrice = ethTwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / ethTwResult.Bids.First().Price);
				var ethNwBidsPrice = ethNwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / ethNwResult.Bids.First().Price);
				var ethQBidsPrice = ethQResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / ethQResult.Bids.First().Price);

				var etcTwAskPrice = etcTwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / etcTwResult.Asks.First().Price);
				var etcNwAskPrice = etcNwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / etcNwResult.Asks.First().Price);
				var etcQAskPrice = etcQResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / etcQResult.Asks.First().Price);
				var etcNwBidsPrice = etcNwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / etcNwResult.Bids.First().Price);
				var etcTwBidsPrice = etcTwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / etcTwResult.Bids.First().Price);
				var etcQBidsPrice = etcQResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / etcQResult.Bids.First().Price);

				var bchTwAskPrice = bchTwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / bchTwResult.Asks.First().Price);
				var bchNwAskPrice = bchNwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / bchNwResult.Asks.First().Price);
				var bchQAskPrice = bchQResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / bchQResult.Asks.First().Price);
				var bchTwBidsPrice = bchTwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / bchTwResult.Bids.First().Price);
				var bchNwBidsPrice = bchNwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / bchNwResult.Bids.First().Price);
				var bchQBidsPrice = bchQResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / bchQResult.Bids.First().Price);

				var btgTwAskPrice = btgTwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / btgTwResult.Asks.First().Price);
				var btgNwAskPrice = btgNwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / btgNwResult.Asks.First().Price);
				var btgQAskPrice = btgQResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / btgQResult.Asks.First().Price);
				var btgNwBidsPrice = btgNwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / btgNwResult.Bids.First().Price);
				var btgTwBidsPrice = btgTwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / btgTwResult.Bids.First().Price);
				var btgQBidsPrice = btgQResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / btgQResult.Bids.First().Price);

				var xrpTwAskPrice = xrpTwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / xrpTwResult.Asks.First().Price);
				var xrpNwAskPrice = xrpNwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / xrpNwResult.Asks.First().Price);
				var xrpQAskPrice = xrpQResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / xrpQResult.Asks.First().Price);
				var xrpTwBidsPrice = xrpTwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / xrpTwResult.Bids.First().Price);
				var xrpNwBidsPrice = xrpNwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / xrpNwResult.Bids.First().Price);
				var xrpQBidsPrice = xrpQResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / xrpQResult.Bids.First().Price);

				var eosTwAskPrice = eosTwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / eosTwResult.Asks.First().Price);
				var eosNwAskPrice = eosNwResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / eosNwResult.Asks.First().Price);
				var eosQAskPrice = eosQResult.Asks.FirstOrDefault(x => x.Cumulative >= cumul / eosQResult.Asks.First().Price);
				var eosNwBidsPrice = eosNwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / eosNwResult.Bids.First().Price);
				var eosTwBidsPrice = eosTwResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / eosTwResult.Bids.First().Price);
				var eosQBidsPrice = eosQResult.Bids.FirstOrDefault(x => x.Cumulative >= cumul / eosQResult.Bids.First().Price);

				#endregion

				#region Set UI Content

				BtcTw.Content = btcTwAskPrice != null && btcTwBidsPrice != null
									? Math.Round((btcTwAskPrice.Price - btcTwBidsPrice.Price) * 100 / btcTwAskPrice.Price, 5) + "%" : "0";

				BtcNw.Content = btcNwAskPrice != null && btcNwBidsPrice != null
									? Math.Round((btcNwAskPrice.Price - btcNwBidsPrice.Price) * 100 / btcNwAskPrice.Price, 5) + "%" : "0";

				BtcQ.Content = btcQAskPrice != null && btcQBidsPrice != null
									? Math.Round((btcQAskPrice.Price - btcQBidsPrice.Price) * 100 / btcQAskPrice.Price, 5) + "%" : "0";

				LtcTw.Content = ltcTwAskPrice != null && ltcTwBidsPrice != null
									? Math.Round((ltcTwAskPrice.Price - ltcTwBidsPrice.Price) * 100 / ltcTwAskPrice.Price, 5) + "%" : "0";

				LtcNw.Content = ltcNwAskPrice != null && ltcNwBidsPrice != null
									? Math.Round((ltcNwAskPrice.Price - ltcNwBidsPrice.Price) * 100 / ltcNwAskPrice.Price, 5) + "%" : "0";

				LtcQ.Content = ltcQAskPrice != null && ltcQBidsPrice != null
									? Math.Round((ltcQAskPrice.Price - ltcQBidsPrice.Price) * 100 / ltcQAskPrice.Price, 5) + "%" : "0";

				EthTw.Content = ethTwAskPrice != null && ethTwBidsPrice != null
									? Math.Round((ethTwAskPrice.Price - ethTwBidsPrice.Price) * 100 / ethTwAskPrice.Price, 5) + "%" : "0";

				EthNw.Content = ethNwAskPrice != null && ethNwBidsPrice != null
									? Math.Round((ethNwAskPrice.Price - ethNwBidsPrice.Price) * 100 / ethNwAskPrice.Price, 5) + "%" : "0";

				EthQ.Content = ethQAskPrice != null && ethQBidsPrice != null
								   ? Math.Round((ethQAskPrice.Price - ethQBidsPrice.Price) * 100 / ethQAskPrice.Price, 5) + "%" : "0";

				EtcTw.Content = etcTwAskPrice != null && etcTwBidsPrice != null
									? Math.Round((etcTwAskPrice.Price - etcTwBidsPrice.Price) * 100 / etcTwAskPrice.Price, 5) + "%" : "0";

				EtcNw.Content = etcNwAskPrice != null && etcNwBidsPrice != null
									? Math.Round((etcNwAskPrice.Price - etcNwBidsPrice.Price) * 100 / etcNwAskPrice.Price, 5) + "%" : "0";

				EtcQ.Content = etcQAskPrice != null && etcQBidsPrice != null
								   ? Math.Round((etcQAskPrice.Price - etcQBidsPrice.Price) * 100 / etcQAskPrice.Price, 5) + "%" : "0";

				BchTw.Content = bchTwAskPrice != null && bchTwBidsPrice != null
									? Math.Round((bchTwAskPrice.Price - bchTwBidsPrice.Price) * 100 / bchTwAskPrice.Price, 5) + "%" : "0";

				BchNw.Content = bchNwAskPrice != null && bchNwBidsPrice != null
									? Math.Round((bchNwAskPrice.Price - bchNwBidsPrice.Price) * 100 / bchNwAskPrice.Price, 5) + "%" : "0";

				BchQ.Content = bchQAskPrice != null && bchQBidsPrice != null
								   ? Math.Round((bchQAskPrice.Price - bchQBidsPrice.Price) * 100 / bchQAskPrice.Price, 5) + "%" : "0";

				BtgTw.Content = btgTwAskPrice != null && btgTwBidsPrice != null
									? Math.Round((btgTwAskPrice.Price - btgTwBidsPrice.Price) * 100 / btgTwAskPrice.Price, 5) + "%" : "0";

				BtgNw.Content = btgNwAskPrice != null && btgNwBidsPrice != null
									? Math.Round((btgNwAskPrice.Price - btgNwBidsPrice.Price) * 100 / btgNwAskPrice.Price, 5) + "%" : "0";

				BtgQ.Content = btgQAskPrice != null && btgQBidsPrice != null
								   ? Math.Round((btgQAskPrice.Price - btgQBidsPrice.Price) * 100 / btgQAskPrice.Price, 5) + "%" : "0";

				XrpTw.Content = xrpTwAskPrice != null && xrpTwBidsPrice != null
									? Math.Round((xrpTwAskPrice.Price - xrpTwBidsPrice.Price) * 100 / xrpTwAskPrice.Price, 5) + "%" : "0";

				XrpNw.Content = xrpNwAskPrice != null && xrpNwBidsPrice != null
									? Math.Round((xrpNwAskPrice.Price - xrpNwBidsPrice.Price) * 100 / xrpNwAskPrice.Price, 5) + "%" : "0";

				XrpQ.Content = xrpQAskPrice != null && xrpQBidsPrice != null
								   ? Math.Round((xrpQAskPrice.Price - xrpQBidsPrice.Price) * 100 / xrpQAskPrice.Price, 5) + "%" : "0";

				EosTw.Content = eosTwAskPrice != null && eosTwBidsPrice != null
									? Math.Round((eosTwAskPrice.Price - eosTwBidsPrice.Price) * 100 / eosTwAskPrice.Price, 5) + "%" : "0";

				EosNw.Content = eosNwAskPrice != null && eosNwBidsPrice != null
									? Math.Round((eosNwAskPrice.Price - eosNwBidsPrice.Price) * 100 / eosNwAskPrice.Price, 5) + "%" : "0";

				EosQ.Content = eosQAskPrice != null && eosQBidsPrice != null
								   ? Math.Round((eosQAskPrice.Price - eosQBidsPrice.Price) * 100 / eosQAskPrice.Price, 5) + "%" : "0";

				#endregion
			});
		}
	}
}