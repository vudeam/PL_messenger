using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Jdenticon;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;
using VectorChat.Utilities.ClientRequests;

namespace VectorChat.Client_WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ClientConfig configInfo;
		private readonly int maxLines = 5;
		private double messageTextBoxCellStartHeight;
		private User currentUser;
		private string currentToken;
		private uint currentGroupID;
		private readonly Dictionary<uint, List<Message>> messageHistories = new Dictionary<uint, List<Message>>();

		enum sendingEdge
		{
			top,
			bottom
		}

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

		private async void OnLoad(object sender, RoutedEventArgs e)
		{
			//Loading a configuration file from a local directory if it exists or creating it if it does not exist
			if (File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
			{
				configInfo = JsonSerializer.Deserialize<ClientConfig>(File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json")));
				if (configInfo.mainWindowHeight > mainWindow.MinHeight && configInfo.mainWindowHeight < mainWindow.MaxHeight)
					mainWindow.Height = configInfo.mainWindowHeight;
				if (configInfo.mainWindowWidth > mainWindow.MinWidth && configInfo.mainWindowWidth < mainWindow.MaxWidth)
					mainWindow.Width = configInfo.mainWindowWidth;
			}
			else
			{
				configInfo = new ClientConfig(200, "http://localhost:8080", 540, 960);
				FileWorker.SaveToFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json"), configInfo);
			}

			messageTextBoxCellStartHeight = messageTextBoxCellHeight.Height.Value;
			OpenEnterWindow();

			messageHistories[0U] = new List<Message>();

			Grid currentUserIcon = DrawRoundedIdenticon(50, Color.FromRgb(135, 163, 191), Color.FromRgb(88, 117, 158), currentUser?.ToString());
			currentUserIcon.HorizontalAlignment = HorizontalAlignment.Right;

			userInfoGrid.Children.Add(currentUserIcon);

			if (!string.IsNullOrEmpty(currentUser?.nickname))
			{
				await Task.Run(() =>
				{
					while (!false)
					{
						MessagesRequest();
						Task.Delay((int)configInfo.messageRequestTime);
					}
				}
				);
			}
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
				int end = messageTextBox.Text.Length - 1;
				for (int i = 1; i < end; i++)
				{
					if (messageTextBox.Text[i] == ' ' && messageTextBox.Text[i - 1] == ' ' && messageTextBox.Text[i + 1] == ' ')
					{
						messageTextBox.Text = messageTextBox.Text.Remove(i - 1, 1);
						i -= 1;
						end -= 1;
					}
					if (i > 1 && i < end - 1)
						if (messageTextBox.Text[i] == '\r' && messageTextBox.Text[i - 2] == '\r' && messageTextBox.Text[i + 2] == '\r')
						{
							messageTextBox.Text = messageTextBox.Text.Remove(i - 1, 2);
							i -= 1;
							end -= 2;
						}
				}
				Message mes = new Message()
				{
					content = messageTextBox.Text,
					timestamp = DateTime.Now,
					fromID = currentUser.ToString(),
					groupID = currentGroupID
				};
				ClientRequests.PostRequest(configInfo.serverAddress, mes);
				messageTextBox.Text = string.Empty;
			}
		}

		private void MessagesRequest()
		{
			DateTime ts = new DateTime();
			var recivedMessages = new List<Message>();
			if (messageHistories[currentGroupID].Count == 0)
			{
				ts = DateTime.Now;
				recivedMessages = ClientRequests.GetRequest(configInfo.serverAddress, currentUser?.nickname, currentUser.userID, currentGroupID, ts, 20);
			}
			else
			{
				ts = messageHistories[currentGroupID][^1].timestamp;
				recivedMessages = ClientRequests.GetRequest(configInfo.serverAddress, currentUser?.nickname, currentUser.userID, currentGroupID, ts);
			}
			if (recivedMessages.Count != 0)
			{
				bool onBottom = messagesScroll.VerticalOffset == messagesScroll.ScrollableHeight;
				messageHistories[currentGroupID].AddRange(recivedMessages);
				foreach (var mes in recivedMessages)
				{
					messagesList.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => BuildMessageBubble(messagesList, mes, sendingEdge.bottom)));
				}

				if (onBottom) messagesList.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => messagesScroll.ScrollToBottom()));
			}
		}

		private void MessagesRequesting()
		{
			while (!false)
			{
				MessagesRequest();
				Thread.Sleep((int)configInfo.messageRequestTime);
			}
		}

		/// <summary>
		///	Adds a message unit to the specified ListBox.
		/// </summary>
		/// <param name="_msg"></param>
		/// <param name="messagesArea"></param>
		private void BuildMessageBubble(ListBox messagesArea, Message _msg, sendingEdge edge)
		{
			Index end = edge == 0 ? 1 : ^1;
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
				Content = _msg.fromID,
				FontSize = 14,
				Background = null,
				BorderBrush = null,
				Foreground = Brushes.DarkSlateGray,
				Padding = new Thickness(0, 2, 0, 2)
			};

			var rect = new Rectangle()
			{
				Fill = new SolidColorBrush(Color.FromRgb(223, 234, 239)),
				RadiusX = 10,
				RadiusY = 10,
				Focusable = false
			};

			var tb = new TextBox()
			{
				Style = messageTextBox.Style,
				TextWrapping = TextWrapping.Wrap,
				Width = 350,
				Text = _msg.content,
				FontSize = 16,
				Background = null,
				BorderBrush = null,
				Focusable = false
			};

			var time = new Label()
			{
				Content = _msg.timestamp.ToShortTimeString(),
				FontSize = 14,
				VerticalAlignment = VerticalAlignment.Bottom,
				Margin = new Thickness(5),
				Foreground = Brushes.DarkGray,
				Background = null,
				BorderBrush = null
			};

			var icon = new Grid();
			if (_msg.fromID != Message.LoginNotification)
			{
				icon = DrawRoundedIdenticon(30, Colors.Transparent, Colors.Transparent, _msg.fromID);
				vertStack.Children.Add(nickname);
			}
			else
			{
				Grid arrowIconGrid = new Grid();
				Image arrowIcon = new Image();
				arrowIcon.Source = new BitmapImage(new Uri(@"Assets/little-arrow.png", UriKind.Relative));
				arrowIconGrid.Children.Add(arrowIcon);
				icon = arrowIconGrid;
				rect.Fill = new SolidColorBrush(Colors.Transparent);
				nickname.Foreground = Brushes.Transparent;
				tb.Foreground = Brushes.Gray;
				vertStack.Children.Add(new UIElement());
			}
			icon.VerticalAlignment = VerticalAlignment.Bottom;
			icon.HorizontalAlignment = HorizontalAlignment.Center;
			icon.Margin = new Thickness(0, 0, 5, 4);
			icon.Height = 30;
			icon.Width = 30;

			if (_msg.fromID == currentUser?.ToString())
			{
				rect.Fill = new SolidColorBrush(Color.FromRgb(188, 217, 232));
			}

			messageGrid.Children.Add(rect);
			messageGrid.Children.Add(tb);
			horizStack.Children.Add(messageGrid);
			horizStack.Children.Add(time);
			vertStack.Children.Add(horizStack);
			Grid.SetColumn(icon, 0);
			mainGrid.Children.Add(icon);
			Grid.SetColumn(vertStack, 1);
			mainGrid.Children.Add(vertStack);
			if (end.ToString().Equals("^1"))
				messagesArea.Items.Add(mainGrid);
			else
				messagesArea.Items.Insert(1, mainGrid);

			var renderedMessageGrid = (((messagesArea.Items[end] as Grid).Children[1] as StackPanel).Children[1] as StackPanel).Children[0] as Grid; //So it should.
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
			(messagesArea.Items[end] as Grid).Margin = new Thickness(10, 0, 10, 0);
		}

		private void OpenEnterWindow()
		{
			var enterWindow = new EnterWindow(configInfo);
			if (enterWindow.ShowDialog() == true)
			{
				currentUser = enterWindow.session.user;
				currentToken = enterWindow.session.token;
				currentGroupID = enterWindow.session.user.groupsIDs[0];
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

		private async void Grid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			await Task.Run(() =>
			{
				if (messageHistories[currentGroupID].Count > 0)
				{
					DateTime ts = new DateTime();
					double ofset;
					var recivedMessages = new List<Message>();
					ts = messageHistories[currentGroupID][0].timestamp;
					ofset = messagesScroll.VerticalOffset;
					recivedMessages = ClientRequests.GetRequest(configInfo.serverAddress, currentUser?.nickname, currentUser.userID, currentGroupID, ts, 40);
					messageHistories[currentGroupID].InsertRange(0, recivedMessages);
					recivedMessages.Reverse();
					messagesList.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
					{
						foreach (var mes in recivedMessages)
						{
							BuildMessageBubble(messagesList, mes, sendingEdge.top);
						}
						messagesScroll.ScrollToVerticalOffset(ofset);
					}));
				}
			});
		}
	}
}
