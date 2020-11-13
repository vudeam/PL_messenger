using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		private Config configInfo;
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
				configInfo = JsonSerializer.Deserialize<Config>(File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json")));
			}
			else
			{
				configInfo = new Config(200, "http://localhost:5005");
				File.WriteAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json"), JsonSerializer.Serialize(configInfo));
			}
			
			//Getting start information
			messageTextBoxCellStartHeight = messageTextBoxCellHeight.Height.Value;
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

		private void messagesRequesing()
		{
			HttpWebRequest mesFromServer = (HttpWebRequest)WebRequest.Create(configInfo.serverAddress + "/api/messages");
			mesFromServer.Method = "GET";
			mesFromServer.ContentType = "Application/json";
		}

        private void messageTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
			if (messageTextBox.Text == "")
				enterSign.Opacity = 0;
        }

        private void messageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
			if (messageTextBox.Text == "")
				enterSign.Opacity = 1;
        }
    }
}
