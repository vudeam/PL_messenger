using NStack;
using Terminal.Gui;

namespace VectorChat.Client_Console
{
	internal class MainWindow : Window
	{
		public MainWindow(ustring title = null) : base(title) { }

		public MainWindow(Rect frame, ustring title = null) : base(frame, title) { }

		public MainWindow(ustring title = null, int padding = 0) : base(title, padding) { }

		public MainWindow(Rect frame, ustring title = null, int padding = 0) : base(frame, title, padding) { }

		public MainWindow() : base() { }
	}
}
