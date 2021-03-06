﻿using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
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
		EnterWindow enterWindow;
		private readonly int maxLines = 6;
		private double messageTextBoxCellStartHeight;
		private User currentUser;
		private string currentToken;
		private Group currentGroup;
		private List<Group> groups = new List<Group>();
		private readonly Dictionary<uint, List<Message>> messageHistories = new Dictionary<uint, List<Message>>();
		private static readonly Random rng = new Random(DateTime.Now.Millisecond);

		enum sendingEdge
		{
			top,
			bottom
		}

		public MainWindow()
		{
			InitializeComponent();
		}

		internal static void DispatcherInvoker(FrameworkElement controlElement, Action action, DispatcherPriority priority = DispatcherPriority.Background) => controlElement.Dispatcher.Invoke(priority, action);

		/// <summary>
		/// Refreshes information about the textbox.
		/// Sets the area and size of the textbox based on its content.
		/// </summary>
		private void ReloadTextBoxHeight(TextBox textBox, RowDefinition gridCellHeight, double gridCellStartHeight, int maxLines)
		{
			if (textBox.LineCount <= maxLines && IsLoaded)
			{
				gridCellHeight.Height = new GridLength(gridCellStartHeight + (textBox.LineCount - 1) * (textBox.FontSize + 4));
			}
			if (textBox.LineCount > maxLines)
			{
				gridCellHeight.Height = new GridLength(gridCellStartHeight + (maxLines - 1) * (textBox.FontSize + 4));
			}
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			messageTextBoxCellStartHeight = messageTextBoxCellHeight.Height.Value;
			//Loading a configuration file from a local directory if it exists or creating it if it does not exist
			//if (File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
			try
			{
				configInfo = JsonSerializer.Deserialize<ClientConfig>(File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json")));
				if (configInfo.mainWindowHeight > mainWindow.MinHeight && configInfo.mainWindowHeight < mainWindow.MaxHeight)
					Height = configInfo.mainWindowHeight;
				if (configInfo.mainWindowWidth > mainWindow.MinWidth && configInfo.mainWindowWidth < mainWindow.MaxWidth)
					Width = configInfo.mainWindowWidth;
				if (configInfo.messageRequestTime < 50U)
					configInfo.messageRequestTime = 200U;
			}
			catch
			{
				configInfo = new ClientConfig()
				{
					messageRequestTime = 200U,
					serverAddress = "http://localhost:8080",
					mainWindowHeight = 540,
					mainWindowWidth = 960,
					enableFileAuth = false,
					login = string.Empty,
					password = string.Empty
				};
				FileWorker.SaveToFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json"), configInfo);
			}

			enterWindow = new EnterWindow(configInfo);
			OpenEnterWindow();

			messageHistories[0U] = new List<Message>();

			Grid currentUserIcon = DrawRoundedIdenticon(50U, Color.FromRgb(135, 163, 191), Color.FromRgb(88, 117, 158), currentUser?.ToString());
			currentUserIcon.HorizontalAlignment = HorizontalAlignment.Right;

			userInfoGrid.Children.Add(currentUserIcon);

			if (!string.IsNullOrEmpty(currentUser?.nickname))
			{
				MessagesRequesting();
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

		//Sending a post request storing the message to the server
		private async void Send(object sender, RoutedEventArgs e)
		{
			if ((connectLabel.Content as string) == "Online")
			{
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
						groupID = currentGroup.groupID
					};
					messageTextBox.Text = string.Empty;
					await Task.Run(() => ClientRequests.ServerRequest<AuthResponse>($"{configInfo.serverAddress}/api/chat/messages", mes));
				}
			}
		}

		private void MessagesRequest()
		{
			DateTime ts = new DateTime();
			const uint messagesCount = 20U;
			var recivedMessages = new List<Message>();
			if (messageHistories[currentGroup.groupID].Count == 0)
			{
				ts = DateTime.Now;
				recivedMessages = ClientRequests.ServerRequest<List<Message>>($"{configInfo.serverAddress}/api/chat/messages/" +
					$"{currentUser?.nickname}/{currentUser.userID}/{currentGroup.groupID}/" +
					$"{ts.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture)}/{messagesCount}/", null, "GET");
			}
			else
			{
				ts = messageHistories[currentGroup.groupID][^1].timestamp;
				recivedMessages = ClientRequests.ServerRequest<List<Message>>($"{configInfo.serverAddress}/api/chat/messages/" +
					$"{currentUser?.nickname}/{currentUser.userID}/{currentGroup.groupID}/" +
					$"{ts.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture)}", null, "GET");
			}
			if (recivedMessages.Count != 0)
			{
				RefreshUsersCount(currentGroup);
				bool onBottom = messagesScroll.VerticalOffset == messagesScroll.ScrollableHeight;
				messageHistories[currentGroup.groupID].AddRange(recivedMessages);
				foreach (var mes in recivedMessages)
				{
					DispatcherInvoker(messagesList, () => BuildMessageBubble(messagesList, mes, sendingEdge.bottom));
				}
				if (onBottom) DispatcherInvoker(messagesList, () => messagesScroll.ScrollToBottom());
			}
		}

		private async void MessagesRequesting()
		{
			int reconectionCount = 3;
			for (int i = 0; i < reconectionCount; i++)
			{
				connectLabel.Foreground = new SolidColorBrush(Color.FromRgb(88, 114, 158));
				connectLabel.Content = "Connecting...";
				connectLabel.FontWeight = FontWeights.Normal;
				connectLabel.Cursor = Cursors.Arrow;
				connectRect.Width = 0;
				await Task.Run(() =>
				{
					while (!false)
					{
						try
						{
							MessagesRequest();
							DispatcherInvoker(userInfoGrid, ()=>
							{
								if ((connectLabel.Content as string) != "Online")
								{
									connectLabel.Content = "Online";
									connectLabel.Foreground = new SolidColorBrush(Color.FromRgb(77, 77, 77));
									SendingButton.IsEnabled = true;
									i = 0;
								}
							});
							Task.Delay((int)configInfo.messageRequestTime);
						}
						catch
						{
							if (i < reconectionCount - 1) break;
							DispatcherInvoker(userInfoGrid, () =>
							{
								connectLabel.Content = "Connect";
								connectLabel.FontWeight = FontWeights.Medium;
								connectLabel.Cursor = Cursors.Hand;
								connectLabel.UpdateLayout();
								connectRect.Width = connectLabel.ActualWidth;
								connectLabel.Foreground = new SolidColorBrush(Color.FromRgb(32, 84, 220));
								SendingButton.IsEnabled = false;
							});
							break;
						}
					}
				});
			}
		}

		/// <summary>
		///	Adds a message unit to the specified StackPanel.
		/// </summary>
		/// <param name="_msg">The message object that will be processed and added to the StackPanel as a bubble</param>
		/// <param name="messagesArea">StackPanel in which the message bubble will be created</param>
		/// <param name="edge">Message is added to the beginning of the StackPanel or to the end</param>
		private void BuildMessageBubble(StackPanel messagesArea, Message _msg, sendingEdge edge)
		{
			Index end = edge == 0 ? 1 : ^1;
			var mainGrid = new Grid();
			var messageGrid = new Grid();
			var vertStack = new StackPanel();
			var horizStack = new StackPanel();

			mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
			mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
			mainGrid.HorizontalAlignment = HorizontalAlignment.Left;

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
				Focusable = false,
				Cursor = Cursors.Arrow
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
			if (_msg.fromID != MessagePhrases.LoginLogoutNotification)
			{
				icon = DrawRoundedIdenticon(30U, Colors.Transparent, Colors.Transparent, _msg.fromID);
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
				messagesArea.Children.Add(mainGrid);
			else
				messagesArea.Children.Insert(1, mainGrid);

			var renderedMessageGrid = (((messagesArea.Children[end] as Grid).Children[1] as StackPanel).Children[1] as StackPanel).Children[0] as Grid; //So it should.
			TextBox renderedTb = renderedMessageGrid.Children[1] as TextBox;
			renderedTb.UpdateLayout();
			renderedTb.Width = tb.ExtentWidth + 6;
			renderedTb.Height = tb.ExtentHeight + 2;
			renderedMessageGrid.Width = tb.Width + 14;
			renderedMessageGrid.Height = tb.Height + 10;
			(messagesArea.Children[end] as Grid).Margin = new Thickness(10, 0, 10, 5);
		}

		/// <summary>
		///	Opens the authentication window. Processes the data received from there
		/// </summary>
		private void OpenEnterWindow()
		{
			if (enterWindow.ShowDialog() == true)
			{
				if (configInfo != enterWindow.configInfo)
				{
					configInfo = enterWindow.configInfo;
					FileWorker.SaveToFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json"), configInfo);
				}
				currentUser = enterWindow.session.user;
				currentToken = enterWindow.session.token;
				groups = enterWindow.startGroups;
				currentGroup = groups[0];
				nicknameLabel.Content = currentUser.nickname;
				idLabel.Content = "#" + currentUser.userID;
				SetGroup(currentGroup);
			}
			else
			{
				Application.Current.MainWindow.Close();
			}
		}

		/// <summary>
		/// Displays information about the current group in the GUI
		/// </summary>
		private void SetGroup(Group currentGroup)
		{
			groupName.Text = $"{currentGroup.name} #{currentGroup.groupID}";
			Grid icon = new Grid();
			icon = DrawRoundedIdenticon(60U, Color.FromRgb(198, 210, 222), Color.FromRgb(88, 117, 158), $"{currentGroup?.name}{currentGroup?.groupID}");
			icon.HorizontalAlignment = HorizontalAlignment.Right;
			groupInfoGrid.Children.Add(icon);
			RefreshUsersCount(currentGroup);
		}

		/// <summary>
		///	Refreshes information about the number of members in the current group in the GUI
		/// </summary>
		private async void RefreshUsersCount(Group currentGroup)
		{
			await Task.Run(() => 
			{
				DispatcherInvoker(groupInfoGrid, () =>
				{
					try
					{
						int count = ClientRequests.ServerRequest<int>($"{configInfo.serverAddress}/api/chat/groups/" +
							$"{currentUser.nickname}/{currentUser.userID}/{currentGroup.groupID}/count", null, "GET");
						Members.Text = $"{count} group member" + (count > 1 ? "s" : string.Empty); 
						if (MembersErr.Foreground != Brushes.Transparent)
							MembersErr.Foreground = Brushes.Transparent;
					}
					catch
					{
						if (MembersErr.Foreground == Brushes.Transparent)
							MembersErr.Foreground = Members.Foreground;
					}
				});
			});
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

		private async void loadHIstoryGrid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (connectLabel.Content as string == "Online" && horizPlusRect.Width == 20)
			{
				DoubleAnimation widthAnimation = new DoubleAnimation()
				{
					From = horizPlusRect.Width,
					To = 0,
					Duration = TimeSpan.FromSeconds(0.2)
				};

				DoubleAnimation heightAnimation = new DoubleAnimation()
				{
					From = vertPlusRect.Height,
					To = 0,
					Duration = TimeSpan.FromSeconds(0.2)
				};

				horizPlusRect.BeginAnimation(WidthProperty, widthAnimation);
				vertPlusRect.BeginAnimation(HeightProperty, heightAnimation);

				if (messageHistories[currentGroup.groupID].Count > 0)
				{
					await Task.Run(() =>
					{
						var recivedMessages = new List<Message>();
						const uint messagesCount = 40U;
						DateTime ts = messageHistories[currentGroup.groupID][0].timestamp;

						recivedMessages = ClientRequests.ServerRequest<List<Message>>($"{configInfo.serverAddress}/api/chat/messages/" +
					$"{currentUser?.nickname}/{currentUser.userID}/{currentGroup.groupID}/" +
					$"{ts.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture)}/{messagesCount}", null, "GET");
						messageHistories[currentGroup.groupID].InsertRange(0, recivedMessages);
						recivedMessages.Reverse();
						DispatcherInvoker(messagesList, () =>
						{
							if (recivedMessages.Count > 0)
							{
								DoubleAnimation widthAnimation = new DoubleAnimation()
								{
									From = horizPlusRect.Width,
									To = 20,
									Duration = TimeSpan.FromSeconds(0.2)
								};

								DoubleAnimation heightAnimation = new DoubleAnimation()
								{
									From = vertPlusRect.Height,
									To = 20,
									Duration = TimeSpan.FromSeconds(0.2)
								};

								horizPlusRect.BeginAnimation(WidthProperty, widthAnimation);
								vertPlusRect.BeginAnimation(HeightProperty, heightAnimation);
								foreach (var mes in recivedMessages)
								{
									BuildMessageBubble(messagesList, mes, sendingEdge.top);
								}
							}
							else
							{
								DoubleAnimation opacityAnimation = new DoubleAnimation()
								{
									From = moreBtn.Opacity,
									To = 0,
									Duration = TimeSpan.FromSeconds(0.4)
								};

								ThicknessAnimation marginAnimation = new ThicknessAnimation()
								{
									From = moreBtn.Margin,
									To = new Thickness(150, 0, 0, 0),
									Duration = TimeSpan.FromSeconds(0.4)
								};

								moreBtn.BeginAnimation(OpacityProperty, opacityAnimation);
								moreBtn.BeginAnimation(MarginProperty, marginAnimation);
								moreBtn.Cursor = Cursors.Arrow;
								moreBtn.IsEnabled = false;
							}
						});
					});
				}
			}
		}

		private void connectLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if ((connectLabel.Content as string) == "Connect")
			{
				MessagesRequesting();
			}
		}

		private void messageTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (Keyboard.Modifiers == ModifierKeys.None)
				{
					e.Handled = true;
					Send(SendingButton, null);
				}
			}
		}

		private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!string.IsNullOrEmpty(currentUser?.nickname) && (connectLabel.Content as string) == "Online")
			{
				Message mes = new Message()
				{
					content = $"{ currentUser}{MessagePhrases.goodbyes[rng.Next(MessagePhrases.goodbyes.Length)]}",
					timestamp = DateTime.Now,
					fromID = MessagePhrases.LoginLogoutNotification,
					groupID = currentGroup.groupID
				};
				HttpWebRequest signupToServer = WebRequest.CreateHttp(configInfo.serverAddress + "/api/chat/messages");
				signupToServer.Method = "POST";
				signupToServer.ContentType = "application/json";
				using (StreamWriter stream = new StreamWriter(signupToServer.GetRequestStream()))
				{
					stream.Write(JsonSerializer.Serialize(mes));
				}
				var webResponse = (HttpWebResponse)signupToServer.GetResponse();
				using (StreamReader stream = new StreamReader(webResponse.GetResponseStream()))
				{
				}
			}
		}

		private void logout_Click(object sender, RoutedEventArgs e)
		{
			configInfo.enableFileAuth = false;
			FileWorker.SaveToFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.json"), configInfo);
			Application.Current.Shutdown();
			string path = Application.ResourceAssembly.Location;
			path = path.Remove(path.Length - 3, 3) + "exe";
			System.Diagnostics.Process.Start(path);
		}
	}
}
