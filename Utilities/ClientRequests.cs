using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using VectorChat.Utilities.Credentials;

namespace VectorChat.Utilities.ClientRequests
{
	public static class ClientRequests
	{
		/// <summary></summary>
        /// <typeparam name="TResponse">Type which response will be deserialized to</typeparam>
        /// <returns>Respone of type <typeparamref name="TResponse"/> deserialized from JSON</returns>
        public static TResponse ServerRequest<TResponse>(string url, object body = null, string method = "POST")
		{
			HttpWebRequest webRequest = WebRequest.CreateHttp(url);
			webRequest.Method = method;
			webRequest.ContentType = "application/json";
			if (body != null) using (StreamWriter writer = new StreamWriter(webRequest.GetRequestStream()))
				{
					writer.Write(JsonSerializer.Serialize(body));
				}
			HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
			using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
			{
				return JsonSerializer.Deserialize<TResponse>(reader.ReadToEnd());
			}
		}
	}
}
