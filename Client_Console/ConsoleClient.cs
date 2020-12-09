using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using Terminal.Gui;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;

namespace VectorChat.Client_Console
{
	class ConsoleClient
	{
		private static ClientConfig config;

		internal static (User user, System.Collections.Generic.List<Message> messages) Session;

		public static void Main()
		{
			LoadConfig();

			Application.Init();

			Application.Top.ColorScheme = Colors.TopLevel;

			#region
			//switch(MessageBox.Query("Login/Signup", "", "Log in", "Sign up"))
			//{
			//	// login
			//	case 0:
			//		SignupRequest signupRequest = new SignupRequest() { acc = new Account() };
			//		//Console.Write("Login: ");
			//		signupRequest.acc.login = "admin";
			//		//Console.Write("Password: ");
			//		signupRequest.acc.password = "admin";

			//		AuthResponse auth = LoginSignupResponse($"{config.serverAddress}/api/auth/login", signupRequest);
			//		switch (auth.code)
			//		{
			//			case ApiErrCodes.Success:
			//				Session.user = auth.usr;
			//				Session.messages = new List<Message>();
			//				break;
			//		}

			//		break;

			//	// signup
			//	case 1:
			//		break;

			//	default:
			//		return;
			//}

			//Window mainWindow = new Window("VectorChat Console Client")
			//{
			//	X = 0,
			//	Y = 0,
			//	Width = Dim.Fill(),
			//	Height = Dim.Fill()
			//};
			//mainWindow.ColorScheme = Colors.Dialog;

			//TextField messageField = new TextField()
			//{
			//	X = mainWindow.X + 1,
			//	Y = mainWindow.Y + 10,
			//	CanFocus = true,
			//	Width = Dim.Width(mainWindow),
			//	Height = Dim.Height(mainWindow)
			//};
			//messageField.Visible = true;
			//messageField.ReadOnly = false;
			////messageField.Bounds = new Rect(messageField.X, messageField.Y, messageField.Width, messageField.Height);

			//Application.Top.Add(mainWindow, messageField);
			#endregion

			// Attach menu bar
			MenuBar mainMenu = new MenuBar(new MenuBarItem[]
			{
				new MenuBarItem("_Action", new MenuItem[]
				{
					new MenuItem("_Login", NStack.ustring.Empty, () =>
					{
						AuthResponse r = ServerRequest<AuthResponse>(
							$"{config.serverAddress}/api/auth/login",
							new SignupRequest() { acc = new Account() { login = "admin", password = "admin"}}
						);
						if (r.code != ApiErrCodes.Success) return;
						Session = (r.usr, new List<Message>());
					}), // Add login  callback
					new MenuItem("_Signup", NStack.ustring.Empty, () => {}) // Add signup callback
				}),
				new MenuBarItem("_Quit", "Exit the app", Application.RequestStop)
			});
			Application.Top.Add(mainMenu);

			// Attach main window (parent for next windows)
			Window mainWindow = new Window("VectorChat Console Client")
			{
				X = 0,
				Y = Pos.Bottom(mainMenu),
				Width  = Dim.Fill(),
				Height = Dim.Fill(),
				ColorScheme = Colors.Dialog
			};
			Application.Top.Add(mainWindow);

			Window messagesWindow = new Window()
			{
				X = 0,
				Y = 0,
				Width = mainWindow.Width,
				Height = mainWindow.Height - 1,
				ColorScheme = Colors.Dialog
			};
			mainWindow.Add(messagesWindow);

			//mainWindow.Add(new Label()
			//{
			//	X = Pos.Left(messagesWindow),
			//	Y = Pos.Bottom(messagesWindow) + 1,
			//	Width = 1,
			//	Height = 1,
			//	Text = ">",
			//});

			TextField messageTextField = new TextField()
			{
				X = Pos.Left(messagesWindow) + 1,
				Y = Pos.Bottom(messagesWindow),
				Width = messagesWindow.Width - 10,
				Height = 1,
				ColorScheme = Colors.Dialog
			};
			mainWindow.Add(messageTextField);

			Button sendBtn = new Button()
			{
				X = Pos.Right(messagesWindow) - 10,
				Y = Pos.Bottom(messagesWindow),
				Width = 10,
				Height = 1,
				Text = "_Send",
				IsDefault = true
			};
			sendBtn.Clicked += () =>
			{
				AuthResponse r = ServerRequest<AuthResponse>(
					$"{config.serverAddress}/api/chat/messages",
					new Message()
					{
						content = messageTextField.Text.ToString(),
						fromID = Session.user.ToString(),
						groupID = 0,
						timestamp = DateTime.Now
					}
				);

				if (r.code != ApiErrCodes.Success) return;
				messageTextField.Text = NStack.ustring.Empty;
			};
			mainWindow.Add(sendBtn);

			Application.Run();

		}

		/// <summary></summary>
		/// <typeparam name="TResponse"></typeparam>
		/// <returns>Respone of type <typeparamref name="TResponse"/> deserialized from JSON</returns>
		private static TResponse ServerRequest<TResponse>(string url, object body, string method = "POST")
		{
			HttpWebRequest webRequest = WebRequest.CreateHttp(url);
			webRequest.Method = method;
			webRequest.ContentType = "application/json";
			using (StreamWriter writer = new StreamWriter(webRequest.GetRequestStream()))
			{
				writer.Write(JsonSerializer.Serialize(body));
			}
			HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
			using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
			{
				return JsonSerializer.Deserialize<TResponse>(reader.ReadToEnd());
			}
		}

		private static void LoadConfig()
		{
			if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
			{
				config = FileWorker.LoadFromFile<ClientConfig>(
					Path.Combine(Directory.GetCurrentDirectory(), "config.json")
				);
			}
			else
			{
				config = new ClientConfig()
				{
					serverAddress = "http://127.0.0.1:8080"
				};
				FileWorker.SaveToFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), config);
			}
		}
	}
}
