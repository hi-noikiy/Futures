namespace Futures
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Security.Cryptography;

	public class Md5Util
	{
		private static readonly char[] HexDigits = {'0', '1', '2', '3', '4', '5',
			                                           '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		public static string BuildMysignV1(Dictionary<string, string> sArray, string secretKey)
		{
			var prestr = CreateLinkString(sArray);
			prestr = prestr + "&secret_key=" + secretKey;
			var mySign = GetMd5String(prestr);

			return mySign;
		}

		private static string CreateLinkString(Dictionary<string, string> paras)
		{
			var keys = new List<string>(paras.Keys);

			var paraSort = from objDic in paras
			               orderby objDic.Key
			               select objDic;
			var prestr = "";
			var i = 0;
			foreach (var kvp in paraSort)
			{
				if (i == keys.Count - 1)
				{
					prestr = prestr + kvp.Key + "=" + kvp.Value;
				}
				else
				{
					prestr = prestr + kvp.Key + "=" + kvp.Value + "&";
				}
				i++;
				if (i == keys.Count)
				{
					break;
				}
			}
			return prestr;
		}

		public static string GetMd5String(string str)
		{
			if (str == null || str.Trim().Length == 0)
			{
				return "";
			}

			var bytes = Encoding.Default.GetBytes(str);
			var md = new MD5CryptoServiceProvider();
			var sb = new StringBuilder();

			bytes = md.ComputeHash(bytes);

			foreach (var t in bytes)
			{
				sb.Append(HexDigits[(t & 0xf0) >> 4] + ""
				          + HexDigits[t & 0xf]);
			}

			return sb.ToString();
		}
	}
}