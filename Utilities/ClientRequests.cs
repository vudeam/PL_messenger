using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using VectorChat.Utilities.Credentials;

namespace VectorChat.Utilities.ClientRequests
{
	public static class ClientRequests
	{
		/// <summary>
		/// Sends a POST request for authorization or registration depending on the passed enum value
		/// </summary>
		/// <param name="serverAddress"></param>
		/// <param name="type"></param>
		/// <param name="body"></param>
		/// <returns></returns>
		public static AuthResponse PostRequest(string serverAddress, AuthRequestType type, SignupRequest body)
		{
			HttpWebRequest signupToServer = WebRequest.CreateHttp(serverAddress + "/api/auth/" + type.ToString());
			signupToServer.Method = "POST";
			signupToServer.ContentType = "application/json";
			using (StreamWriter stream = new StreamWriter(signupToServer.GetRequestStream()))
			{
				stream.Write(JsonSerializer.Serialize(body));
			}
			var webResponse = (HttpWebResponse)signupToServer.GetResponse();
			AuthResponse response;
			using (StreamReader stream = new StreamReader(webResponse.GetResponseStream()))
			{
				response = JsonSerializer.Deserialize<AuthResponse>(stream.ReadToEnd());
			}
			return response;
		}

		/// <summary>
		/// Sends a POST request with a message to the server
		/// </summary>
		/// <param name="serverAddress"></param>
		/// <param name="body"></param>
		public static void PostRequest(string serverAddress, Message body)
		{
			HttpWebRequest signupToServer = WebRequest.CreateHttp(serverAddress + "/api/chat/messages");
			signupToServer.Method = "POST";
			signupToServer.ContentType = "application/json";
			using (StreamWriter stream = new StreamWriter(signupToServer.GetRequestStream()))
			{
				stream.Write(JsonSerializer.Serialize(body));
			}
			var webResponse = (HttpWebResponse)signupToServer.GetResponse();
			using (StreamReader stream = new StreamReader(webResponse.GetResponseStream()))
			{
			}
		}

		/// <summary>
		/// Sends a request to get the latest messages from the passed timestamp to now
		/// </summary>
		/// <param name="serverAddress"></param>
		/// <param name="currentUser"></param>
		/// <param name="currentID"></param>
		/// <param name="groupID"></param>
		/// <param name="timestamp"></param>
		/// <returns></returns>
		public static List<Message> GetRequest(string serverAddress, string currentUser, uint currentID, uint groupID, DateTime timestamp)
		{
			string path = serverAddress + $"/api/chat/messages/" +
				$"{currentUser}/" +
				$"{currentID}/" +
				$"{groupID}/" +
				$"{timestamp.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture)}";
			return MessageListRequest(path);
		}

		/// <summary>
		/// Sends a request to receive the passed number of messages from the passed timestamp to the past
		/// </summary>
		/// <param name="serverAddress">address of server</param>
		/// <param name="currentUser"></param>
		/// <param name="currentID"></param>
		/// <param name="groupID"></param>
		/// <param name="timestamp"></param>
		/// <param name="messagesCount">Maximum number of messages requested</param>
		/// <returns></returns>
		public static List<Message> GetRequest(string serverAddress, string currentUser, uint currentID, uint groupID, DateTime timestamp, uint messagesCount)
		{
			string path = serverAddress + $"/api/chat/messages/" +
				$"{currentUser}/" +
				$"{currentID}/" +
				$"{groupID}/" +
				$"{timestamp.ToUniversalTime().ToString("O",System.Globalization.CultureInfo.InvariantCulture)}/{messagesCount}";
			return MessageListRequest(path);
		}

		private static List<Message> MessageListRequest(string webPath)
		{
			var recievedMessagesList = new List<Message>();

			HttpWebRequest mesFromServer = WebRequest.CreateHttp(webPath);
			mesFromServer.Method = "GET";

			mesFromServer.Proxy = null;
			var webResponse = (HttpWebResponse)mesFromServer.GetResponse();
			using (StreamReader stream = new StreamReader(webResponse.GetResponseStream()))
			{
				recievedMessagesList = JsonSerializer.Deserialize<List<Message>>(stream.ReadToEnd());
			}
			return recievedMessagesList;
		}
	}

	public enum AuthRequestType
	{
		login,
		signup
	};
}
