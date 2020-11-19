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
				configInfo = new ClientConfig(200, "http://localhost:5005");
				File.WriteAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json"), JsonSerializer.Serialize(configInfo));
			}
			
			//Getting start information
			messageTextBoxCellStartHeight = messageTextBoxCellHeight.Height.Value;
			MessagesRequesing(); 
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
				HttpWebRequest mesToServer = (HttpWebRequest)WebRequest.Create(configInfo.serverAddress + "/api/messages");
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
				Fill = new SolidColorBrush(Color.FromRgb(180, 255, 200)),
				RadiusX = 10,
				RadiusY = 10,
				Focusable = false
			};
			messageGrid.Children.Add(rect);

			var tb = new TextBox()
			{
				TextWrapping = TextWrapping.Wrap,
				Width = 240	,
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
			horizStack.Children.Add(time);
			vertStack.Children.Add(horizStack);
			mainGrid.Children.Add(vertStack);

			messagesArea.Items.Add(mainGrid);
			var renderedMessageGrid = (((messagesArea.Items[^1] as Grid).Children[0] as StackPanel).Children[1] as StackPanel).Children[0] as Grid; //So it should.
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
			(messagesArea.Items[messagesArea.Items.Count - 1] as Grid).Margin = new Thickness(15, 10, 100, 5);
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
	}
}
