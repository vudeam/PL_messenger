using System;
using System.IO;
using System.Collections.Generic;
using Terminal.Gui;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;
using VectorChat.Utilities.ClientRequests;

namespace VectorChat.Client_Console
{
	class ConsoleClient
	{
		private static ClientConfig config;
		internal static (User user, List<Message> messages) Session;
		private static MenuBar mainMenu;
		private static Window mainWindow;
		private static Window messagesWindow;
		//private static TextField messageTextField;
		private static TextView msgTextView;
		private static Button sendButton;

		public static void Main()
		{
			Application.Init();

			Application.Top.ColorScheme = Colors.TopLevel;

			// Attach menu bar
			mainMenu = new MenuBar(new MenuBarItem[]
			{
				new MenuBarItem("_Action", new MenuItem[]
				{
					new MenuItem("_Login", NStack.ustring.Empty, () => {}),  // Add login callback
					new MenuItem("_Signup", NStack.ustring.Empty, () => {}), // Add signup callback
					new MenuItem("Load _up", NStack.ustring.Empty, () =>
					{
						List<Message> received = ClientRequests.ServerRequest<List<Message>>(
							$"{config.serverAddress}/api/chat/messages/" +
							$"{Session.user.nickname}/{Session.user.userID}/{0}/" +
							$"{DateTime.Now.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture)}/{21}",
							null,
							"GET"
						);
						Session.messages.InsertRange(0, received);
						DrawMessages(Session.messages, messagesWindow);
					}),
					new MenuItem("Load _down", NStack.ustring.Empty, () =>
					{
						List<Message> received = ClientRequests.ServerRequest<List<Message>>(
							$"{config.serverAddress}/api/chat/messages/" +
							$"{Session.user.nickname}/{Session.user.userID}/{0}/" +
							$"{DateTime.Now.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture)}",
							null,
							"GET"
						);
						Session.messages.AddRange(received);
						DrawMessages(Session.messages, messagesWindow);
					})
				}),
				new MenuBarItem("_Quit", "Exit the app", Application.RequestStop)
			});
			Application.Top.Add(mainMenu);

			// Attach main window (parent for next windows)
			mainWindow = new Window("VectorChat Console Client")
			{
				X = 0,
				Y = Pos.Bottom(mainMenu),
				Width  = Dim.Fill(),
				Height = Dim.Fill(),
				ColorScheme = Colors.Dialog
			};
			Application.Top.Add(mainWindow);

			messagesWindow = new Window()
			{
				X = 0,
				Y = 0,
				Width = mainWindow.Width,
				Height = mainWindow.Height - 4, // !!!!!!!!!!!!!!!!!!!!!!!! change to -1
				ColorScheme = Colors.Dialog
			};
			mainWindow.Add(messagesWindow);

			//messageTextField = new TextField()
			//{
			//	X = Pos.Left(messagesWindow) + 1,
			//	Y = Pos.Bottom(messagesWindow),
			//	Width = messagesWindow.Width - 10,
			//	Height = 1,
			//	ReadOnly = true,
			//	ColorScheme = Colors.Dialog
			//};
			//mainWindow.Add(messageTextField);

			msgTextView = new TextView()
			{
				X = Pos.Left(messagesWindow) + 1,
				Y = Pos.Bottom(messagesWindow),
				Width = messagesWindow.Width - 10,
				Height = 4,
				ReadOnly = true,
				ColorScheme = Colors.Dialog,
				
			};
			msgTextView.TextChanged += () =>
			{
				if (msgTextView.Text.Length > 10)
				{
					msgTextView.Text = NStack.ustring.Empty;
					msgTextView.ScrollTo(-1);
				}
			};
			mainWindow.Add(msgTextView);

			sendButton = new Button()
			{
				X = Pos.Right(messagesWindow) - 10,
				Y = Pos.Bottom(messagesWindow),
				Width = 10,
				Height = 1,
				Text = "_Send",
				IsDefault = true
			};
			sendButton.Clicked += () =>
			{
				// if (NStack.ustring.IsNullOrEmpty(messageTextField.Text)) return;
				if (NStack.ustring.IsNullOrEmpty(msgTextView.Text)) return;
				AuthResponse r = ClientRequests.ServerRequest<AuthResponse>(
					$"{config.serverAddress}/api/chat/messages",
					new Message()
					{
						//content = messageTextField.Text.ToString(),
						content = msgTextView.Text.ToString(),
						fromID = Session.user.ToString(),
						groupID = 0,
						timestamp = DateTime.Now
					}
				);

				if (r.code != ApiErrCodes.Success) return;
				// messageTextField.Text = NStack.ustring.Empty;
				msgTextView.Text = NStack.ustring.Empty;
			};
			mainWindow.Add(sendButton);

			LoadConfig();

			Application.Run();

		}

		private static void DrawMessages(List<Message> messages, Window container)
		{
			container.RemoveAll();
			int offset = 0;
			foreach (Message msg in messages)
			{
				View mesView = new View()
				{
					X = 0,
					Y = Pos.Top(messagesWindow) + offset,
					Width = messagesWindow.Width,
					Height = 1,
					Text = msg.ToString()
				};
				container.Add(mesView);
				Application.Refresh();
				offset += 1;
			}
		}

		private static void LoadConfig()
		{
			if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
			{
				config = FileWorker.LoadFromFile<ClientConfig>(
					Path.Combine(Directory.GetCurrentDirectory(), "config.json")
				);
				if (config.enableFileAuth)
				{
					AuthResponse r = new AuthResponse() { code = ApiErrCodes.Unknown };
					try
					{
						r = ClientRequests.ServerRequest<AuthResponse>(
							$"{config.serverAddress}/api/auth/login",
							new SignupRequest() { acc = new Account() { login = config.login, password = config.password} }
						);
					}
					catch
					{
						Session = (new User(), new List<Message>());
					}
					if (r.code == ApiErrCodes.Success)
					{
						Session = (r.usr, new List<Message>());
						msgTextView.ReadOnly = false;
					}
				}
			}
			else
			{
				config = new ClientConfig()
				{
					serverAddress = "http://127.0.0.1:8080",
					enableFileAuth = false,
					login = "",
					password = ""
				};
				FileWorker.SaveToFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), config);
			}
		}
	}
}
