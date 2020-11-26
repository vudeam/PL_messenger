using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;

namespace VectorChat.Client_WPF
{
	/// <summary>
	/// Логика взаимодействия для EnterWindow.xaml
	/// </summary>
	public partial class EnterWindow : Window
	{
		ClientConfig configInfo;

		internal (User, string) session { get; private set; }

		public EnterWindow(ClientConfig _config)
		{
			configInfo = _config;
			InitializeComponent();
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty((sender as TextBox).Text))
			{
				(((sender as TextBox).Parent as Grid).Children[1] as TextBlock).Opacity = 0;
			}
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty((sender as TextBox).Text))
			{
				(((sender as TextBox).Parent as Grid).Children[1] as TextBlock).Opacity = 1;
			}
		}

		private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty((sender as PasswordBox).Password))
			{
				(((sender as PasswordBox).Parent as Grid).Children[1] as TextBlock).Opacity = 0;
			}
		}

		private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty((sender as PasswordBox).Password))
			{
				(((sender as PasswordBox).Parent as Grid).Children[1] as TextBlock).Opacity = 1;
			}
		}

		private void EnterButton_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (string.IsNullOrEmpty(passwordPasswordBox.Password))
			{
				ErrorLabel.Text = "The password field is empty";
				ErrorLabel.Opacity = 1;
				return;
			}
			if ((authButtonGrid.Children[1] as TextBlock).Text.Equals("Log in"))
			{
				AuthResponse response = AuthRequest(AuthRequestType.login);
				switch (response.code)
				{
					case ApiErrCodes.Success:
						session = (response.usr, response.token);
						this.DialogResult = true;
						return;
					case ApiErrCodes.LoginNotFound:
						ErrorLabel.Text = response.defaultMessage;
						ErrorLabel.Opacity = 1;
						return;
					case ApiErrCodes.PasswordIncorrect:
						ErrorLabel.Text = response.defaultMessage;
						ErrorLabel.Opacity = 1;
						return;
					case ApiErrCodes.Unknown:
						ErrorLabel.Text = "Damn what you've done!";
						ErrorLabel.Opacity = 1;
						return;
					default:
						break;
				}
			}
			else if ((authButtonGrid.Children[1] as TextBlock).Text.Equals("Sign up"))
			{
				if (!passwordPasswordBox.Password.Equals(passwordAgainPasswordBox.Password))
				{
					ErrorLabel.Text = "Passwords don't match";
					ErrorLabel.Opacity = 1;
					return;
				}
				AuthResponse response = AuthRequest(AuthRequestType.signup);
				switch (response.code)
				{
					case ApiErrCodes.Success:
						session = (response.usr, response.token);
						this.DialogResult = true;
						return;
					case ApiErrCodes.LoginTaken:
						ErrorLabel.Text = "Entered login is already taken";
						ErrorLabel.Opacity = 1;
						return;
					case ApiErrCodes.Unknown:
						ErrorLabel.Text = "Damn what you've done!";
						ErrorLabel.Opacity = 1;
						return;
					default:
						break;
				}
			}
		}

		private void SwapButton_Click(object sender, MouseButtonEventArgs e)
		{
			ErrorLabel.Opacity = 0;
			if (modeSwapLabel.Text.Equals("Sign up"))
			{
				repPassGrid.IsEnabled = true;
				nicknameGrid.IsEnabled = true;
				repPassGrid.Opacity = 1;
				nicknameGrid.Opacity = 1;
				(authButtonGrid.Children[1] as TextBlock).Text = "Sign up";
				modeSwapLabel.Text = "Log in";
			}
			else if (modeSwapLabel.Text.Equals("Log in"))
			{
				repPassGrid.IsEnabled = false;
				nicknameGrid.IsEnabled = false;
				repPassGrid.Opacity = 0;
				nicknameGrid.Opacity = 0;
				(authButtonGrid.Children[1] as TextBlock).Text = "Log in";
				modeSwapLabel.Text = "Sign up";
			}
		}
		private AuthResponse AuthRequest(AuthRequestType type)
		{
			string _type = string.Empty;
			string _nickname = string.Empty;
			switch (type)
			{
				case AuthRequestType.login:
					_type = "login";
					_nickname = null;
					break;
				case AuthRequestType.signup:
					_type = "signup";
					_nickname = nicknameTextBox.Text;
					break;
				default:
					break;
			}
			HttpWebRequest signupToServer = (HttpWebRequest)WebRequest.Create(configInfo.serverAddress + "/api/auth/" + type);
			signupToServer.Method = "POST";
			signupToServer.ContentType = "application/json";
			var newAccount = new SignupRequest()
			{
				acc = new Account()
				{
					login = loginTextBox.Text,
					password = passwordPasswordBox.Password
				},
				nickname = _nickname
			};
			using (StreamWriter stream = new StreamWriter(signupToServer.GetRequestStream()))
			{
				stream.Write(JsonSerializer.Serialize(newAccount));
			}
			var webResponse = (HttpWebResponse)signupToServer.GetResponse();
			AuthResponse response;
			using (StreamReader stream = new StreamReader(webResponse.GetResponseStream()))
			{
				response = JsonSerializer.Deserialize<AuthResponse>(stream.ReadToEnd());
			}
			return response;
		}
		
		enum AuthRequestType
		{
			login,
			signup
		};

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key.Equals(Key.Enter))
			{
				EnterButton_MouseUp(authButtonGrid.Children[1], null);
			}
		}
	}
}
