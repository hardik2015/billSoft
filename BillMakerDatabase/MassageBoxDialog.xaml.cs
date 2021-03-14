using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace BillMakerDatabase
{
	/// <summary>
	/// Interaction logic for MessageBoxDialog.xaml
	/// </summary>
	public partial class MessageBoxDialog : ContentDialog
	{
		public MessageBoxDialog(string title, string message)
		{
			messageText = message;
			titleText = title;
			InitializeComponent();
			this.DataContext = this;
		}

		public string messageText
		{
			get; private set;
		}

		public string titleText
		{
			get; private set;
		}

	}
}
