using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;
using VectorChat.Utilities.ClientRequests;

namespace VectorChat.Client_WPF
{
	/// <summary>
	/// Логика взаимодействия для EnterWindow.xaml
	/// </summary>
	public partial class EnterWindow : Window
	{
		ClientConfig configInfo;

		public enum AuthRequestType
		{
			login,
			signup
		};

		internal (User user, string token) session { get; private set; }
		internal List<Group> startGroups = new List<Group>();

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
					case ApiErrCodes.NoConnection:
						ErrorLabel.Text = "No connection to server";
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
						AuthRequest(AuthRequestType.login);
						this.DialogResult = true;
						return;
					case ApiErrCodes.LoginTaken:
						ErrorLabel.Text = "Entered login is already taken";
						ErrorLabel.Opacity = 1;
						return;
					case ApiErrCodes.NoConnection:
						ErrorLabel.Text = "No connection to server";
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
			string _nickname;
			if (type == AuthRequestType.login)
				_nickname = null;
			else
				_nickname = nicknameTextBox.Text;

			var newAccount = new SignupRequest()
			{
				nickname = _nickname,
				acc = new Account()
				{
					login = loginTextBox.Text,
					password = passwordPasswordBox.Password
				}
			};
			AuthResponse response;
			try
			{
				response = ClientRequests.ServerRequest<AuthResponse>($"{configInfo.serverAddress}/api/auth/{type}", newAccount);
				if (response.code == ApiErrCodes.Success)
				{
					startGroups = ClientRequests.ServerRequest<List<Group>>($"{configInfo.serverAddress}/api/chat/groups/" +
						$"{response.usr?.nickname}/{response.usr?.userID}", null, "GET");
				}
			}
			catch
			{
				response = new AuthResponse()
				{
					code = ApiErrCodes.NoConnection
				};
			}
			return response;

		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key.Equals(Key.Enter))
			{
				EnterButton_MouseUp(authButtonGrid.Children[1], null);
			}
		}
	}
}
