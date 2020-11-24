using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Text.Json;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;
using Jdenticon;

namespace VectorChat.Client_WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ClientConfig configInfo;
		private int maxLines = 5;
		private double messageTextBoxCellStartHeight;
		private User currentUser;
		private string currentToken;

		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Refreshes information about the textbox.
		/// Sets the area and size of the textbox based on its content.
		/// </summary>
		private void ReloadTextBoxHeight(TextBox textBox, RowDefinition gridCellHeight, double gridCellStartHeight, int maxLines)
		{
			if (textBox.LineCount < maxLines && IsLoaded)
			{
				gridCellHeight.Height = new GridLength(gridCellStartHeight + (textBox.LineCount - 1) * (textBox.FontSize + 4));
			}
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			//Loading a configuration file from a local directory if it exists or creating it if it does not exist
			if (File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
			{
				configInfo = JsonSerializer.Deserialize<ClientConfig>(File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json")));
			}
			else
			{
				configInfo = new ClientConfig(200, "http://localhost:8080", 540, 960);
				File.WriteAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json"), JsonSerializer.Serialize(configInfo));
			}

			if (configInfo.mainWindowHeight > mainWindow.MinHeight && configInfo.mainWindowHeight < mainWindow.MaxHeight)
				mainWindow.Height = configInfo.mainWindowHeight;
			if (configInfo.mainWindowWidth > mainWindow.MinWidth && configInfo.mainWindowWidth < mainWindow.MaxWidth)
				mainWindow.Width = configInfo.mainWindowWidth;

			//Getting start information
			messageTextBoxCellStartHeight = messageTextBoxCellHeight.Height.Value;
			OpenEnterWindow();
			MessagesRequesing();

			Grid currentUserIcon = DrawRoundedIdenticon(50, Color.FromRgb(135, 163, 191), Color.FromRgb(88, 117, 158), currentUser?.ToString());
			currentUserIcon.HorizontalAlignment = HorizontalAlignment.Right;

			userInfoGrid.Children.Add(currentUserIcon);
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ReloadTextBoxHeight(messageTextBox, messageTextBoxCellHeight, messageTextBoxCellStartHeight, maxLines);
		}

		private void messageTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ReloadTextBoxHeight(messageTextBox, messageTextBoxCellHeight, messageTextBoxCellStartHeight, maxLines);
		}


		private void Send(object sender, RoutedEventArgs e)
		{
			//Sending a post request storing the message to the server
			bool infoAvailable = false;
			foreach (char symbol in messageTextBox.Text)
			{
				if (symbol != ' ' && symbol != '\r' && symbol != '\n')
				{
					infoAvailable = true;
					break;
				}
			}
			if (!String.IsNullOrEmpty(messageTextBox.Text) && infoAvailable)
			{
				HttpWebRequest mesToServer = (HttpWebRequest)WebRequest.Create(configInfo.serverAddress + "/api/chat/messages");
				mesToServer.Method = "POST";
				mesToServer.ContentType = "application/json";
				Message mes = new Message()
				{
					Content = messageTextBox.Text,
					Timestamp = DateTime.Now,
					FromID = "0"
				};
				using (StreamWriter stream = new StreamWriter(mesToServer.GetRequestStream()))
				{
					stream.Write(JsonSerializer.Serialize(mes));
				}
				mesToServer.Proxy = null;
				using (var response = (HttpWebResponse)mesToServer.GetResponse())
				{
				}
				messageTextBox.Text = "";
			}
		}

		private void MessagesRequesing()
		{
			Message message1 = new Message()
			{
				Content = "Test message 1",
				FromID = "TestUser #1",
				Timestamp = DateTime.Now
			};
			Message message2 = new Message()
			{
				Content = "Test message 2",
				FromID = "TestUser #2",
				Timestamp = DateTime.Now
			};

			BuildMessageBubble(messagesList, message1);
			BuildMessageBubble(messagesList, message2);
		}

		/// <summary>
		///	Adds a message unit to the specified ListBox.
		/// </summary>
		/// <param name="_msg"></param>
		/// <param name="messagesArea"></param>
		private void BuildMessageBubble(ListBox messagesArea, Message _msg)
		{
			var mainGrid = new Grid();
			var messageGrid = new Grid();
			var vertStack = new StackPanel();
			var horizStack = new StackPanel();

			mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
			mainGrid.ColumnDefinitions.Add(new ColumnDefinition());

			vertStack.Orientation = Orientation.Vertical;
			horizStack.Orientation = Orientation.Horizontal;

			var nickname = new Label()
			{
				Content = _msg.FromID,
				FontSize = 14,
				Background = null,
				BorderBrush = null
			};
			vertStack.Children.Add(nickname);

			var rect = new Rectangle()
			{
				Fill = new SolidColorBrush(Color.FromRgb(171, 202, 239)),
				RadiusX = 10,
				RadiusY = 10,
				Focusable = false
			};
			if (_msg.FromID == currentUser?.ToString())
				rect.Fill = new SolidColorBrush(Color.FromRgb(88, 117, 158));
			messageGrid.Children.Add(rect);

			var tb = new TextBox()
			{
				Style = messageTextBox.Style,
				TextWrapping = TextWrapping.Wrap,
				Width = 240,
				Text = _msg.Content,
				FontSize = 16,
				Background = null,
				BorderBrush = null,
				Focusable = false
			};
			messageGrid.Children.Add(tb);
			horizStack.Children.Add(messageGrid);

			var time = new Label()
			{
				Content = _msg.Timestamp.ToShortTimeString(),
				FontSize = 14,
				VerticalAlignment = VerticalAlignment.Bottom,
				Margin = new Thickness(0, 0, 5, 0),
				Foreground = Brushes.DarkGray,
				Background = null,
				BorderBrush = null
			};

			var icon = new Grid();
			icon = DrawRoundedIdenticon(30, Colors.Transparent, Colors.Transparent, _msg.FromID);
			icon.VerticalAlignment = VerticalAlignment.Bottom;
			icon.HorizontalAlignment = HorizontalAlignment.Center;
			icon.Margin = new Thickness(0, 0, 5, 0);

			horizStack.Children.Add(time);
			vertStack.Children.Add(horizStack);

			Grid.SetColumn(icon, 0);
			mainGrid.Children.Add(icon);			

			Grid.SetColumn(vertStack, 1);
			mainGrid.Children.Add(vertStack);

			messagesArea.Items.Add(mainGrid);
			var renderedMessageGrid = (((messagesArea.Items[^1] as Grid).Children[1] as StackPanel).Children[1] as StackPanel).Children[0] as Grid; //So it should.
			foreach (var objects in renderedMessageGrid.Children)
			{
				if (objects.GetType().Equals(typeof(TextBox)))
				{
					TextBox renderedTb = objects as TextBox;
					renderedTb.UpdateLayout();
					renderedTb.Width = tb.ExtentWidth + 6;
					renderedTb.Height = tb.ExtentHeight + 2;
					renderedMessageGrid.Width = tb.Width + 14;
					renderedMessageGrid.Height = tb.Height + 10;
				}
			}
			(messagesArea.Items[messagesArea.Items.Count - 1] as Grid).Margin = new Thickness(15, 5, 100, 5);
		}

		private void OpenEnterWindow()
		{
			var enterWindow = new EnterWindow(configInfo);
			if (enterWindow.ShowDialog() == true)
			{
				currentUser = enterWindow.session.Item1;
				currentToken = enterWindow.session.Item2;
				nicknameLabel.Content = currentUser.nickname;
				idLabel.Content = "#" + currentUser.userID;
			}
			else
			{
				Application.Current.MainWindow.Close();
			}
		}

		private void messageTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(messageTextBox.Text))
				enterSign.Opacity = 0;
		}

		private void messageTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(messageTextBox.Text))
				enterSign.Opacity = 1;
		}

		private Grid DrawRoundedIdenticon(uint size, Color maskColor, Color ringColor, string keyword)
		{
			var grid = new Grid();
			var iconBrush = new ImageBrush(Bitmap2BitmapImage(Identicon.FromValue(keyword, (int)size).ToBitmap()));
			var maskBrush = new RadialGradientBrush();
			maskBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.99));
			maskBrush.GradientStops.Add(new GradientStop(maskColor, 0.99));

			var imageRect = new Rectangle()
			{
				Fill = iconBrush,
				Width = size,
				Height = size
			};

			var maskRect = new Rectangle()
			{
				Fill = maskBrush,
				Width = size + 1,
				Height = size + 1
			};

			var ring = new Ellipse()
			{
				Width = size + 2,
				Height = size + 2,
				Stroke = new SolidColorBrush(ringColor),
				StrokeThickness = 3
			};

			grid.Children.Add(imageRect);
			grid.Children.Add(maskRect);
			grid.Children.Add(ring);

			return grid;
		}

		private BitmapImage Bitmap2BitmapImage(System.Drawing.Bitmap bitmap)
		{
			using (MemoryStream memory = new MemoryStream())
			{
				bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
				memory.Position = 0;
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = memory;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();

				return bitmapImage;
			}
		}

	}
}
