namespace Futures
{
	using System.Collections.Generic;

	using Newtonsoft.Json;

	public class FutureDepth
	{
		[JsonProperty(PropertyName = "asks")]
		public List<FutureDepthDetail> Asks { get; set; } = new List<FutureDepthDetail>();

		[JsonProperty(PropertyName = "bids")]
		public List<FutureDepthDetail> Bids { get; set; } = new List<FutureDepthDetail>();
	}

	public class FutureDepthDetail
	{
		public double Price { get; set; }

		public double Amount { get; set; }

		public double Cumulative { get; set; }
	}
}