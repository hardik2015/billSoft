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
using System.Data.Sql;
using Microsoft.Win32;
//using Microsoft.SqlServer.Management.Smo;
using System.Data;
using System.ComponentModel;
using Microsoft.SqlServer.Management.Smo;
using System.IO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net.Http;
using DeviceId;
using ModernWpf.Controls;
using DeviceId.Formatters;
using DeviceId.Encoders;
using System.Threading;
using System.Diagnostics;
using System.Security.Principal;

namespace BillMakerDatabase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        List<string> ServerList = new List<string>();
        SqlConnection conn;
        String insertProductKey;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            ServerNameList = ServerList;
            
        }
        public List<string> ServerNameList { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GetDataSources()
        {
            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    foreach (var instanceName in instanceKey.GetValueNames())
                    {
                        ServerList.Add(instanceName);
                    }
                }
            }
        }

        private async void Install_Button_Click(object sender, RoutedEventArgs e)
        {
            conn.Open();
            string script = File.ReadAllText(@"DatabaseScript.sql");
            script += insertProductKey;
            if (conn.State == ConnectionState.Open)
            {
                try
                {
                    MainGrid.Visibility = Visibility.Hidden;
                    ProcessGoingOn.Visibility = Visibility.Visible;
                    string dbMakerString = "CREATE DATABASE BillMaker";
                    using (SqlCommand command = new SqlCommand(dbMakerString, conn))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    using (var tran = conn.BeginTransaction())
                    using (SqlCommand installCommand = new SqlCommand(string.Empty, conn, tran))
                    {
                        try
                        {
                            string sqlBatch = string.Empty;
                            foreach (string line in script.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (line.ToUpperInvariant().Trim() == "GO")
                                {
                                    installCommand.CommandText = sqlBatch;
                                    await installCommand.ExecuteNonQueryAsync();
                                    sqlBatch = string.Empty;
                                }
                                else
                                {
                                    sqlBatch += line + "\n";
                                }
                            }
                        }
                        catch
                        {
                            tran.Rollback();
                        }
                        tran.Commit();
                    }
                    ProcessGoingOn.Visibility = Visibility.Hidden;
                    MainGrid.Visibility = Visibility.Visible;
                }
                catch
                {
                    ProcessGoingOn.Visibility = Visibility.Hidden;
                    MainGrid.Visibility = Visibility.Visible;
                }
                finally
                {
                    conn.Close();
                }
                ConnectionTest.Content = "Installed";
                RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
                {
                    RegistryKey instanceKey = hklm.CreateSubKey(@"SOFTWARE\AlakhSoftware", true);
                    if (instanceKey != null)
                    {
                        instanceKey.SetValue("Server","(local)\\"+ServerListBox.SelectedItem as String);
                        instanceKey.SetValue("Database", "BilMaker");
                        instanceKey.Close();
                    }

                }
            }
            conn.Close();
        }

        private async void ConnectionBtn_Click(object sender, RoutedEventArgs e)
        {
            if(ServerListBox.SelectedIndex < 0)
            {
                MessageBoxDialog errorDialog = new MessageBoxDialog("Error!!!!", "Please select Server from dropdown");
                errorDialog.PrimaryButtonText = "Ok";
                errorDialog.IsSecondaryButtonEnabled = false;
                _ = await errorDialog.ShowAsync();
                return;
            }
            String SelectedServer = ServerListBox.SelectedItem as String;
            if(SelectedServer.ToUpper().Equals("MSSQLSERVER"))
			{
                SelectedServer = "";
			}
            String ConnectionString = "Data Source=(local)\\" + SelectedServer + ";Initial Catalog=master;Integrated Security=True;MultipleActiveResultSets=True;App=EntityFramework&quot;";
            conn = new SqlConnection(ConnectionString);
            if (conn == null)
            {
                ConnectionTest.Content = "Error";
            }
            else
            {
                ProcessGoingOn.Visibility = Visibility.Visible;
                MainGrid.Visibility = Visibility.Hidden;
                CancellationTokenSource source = new CancellationTokenSource();
                source.CancelAfter(5000);
                CancellationToken token = source.Token;
                try
                {
                    await conn.OpenAsync(token);
                    if (conn.State == ConnectionState.Open)
                    {
                        bool isDBExisit = false;
                        string dbCheckingString = "SELECT name FROM master.dbo.sysdatabases WHERE('[' + name + ']' = 'BillMaker' OR name = 'BillMaker')";
                        SqlCommand command = new SqlCommand(dbCheckingString, conn);
                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        bool isDbNotDeleted = false;
                        while (reader.Read())
                        {
                            isDBExisit = true;
                        }

                        if (isDBExisit)
                        {
                            MessageBoxDialog errorDialog = new MessageBoxDialog("Warning!!", "Database is already exists \n Do you want to delete it and Make new one and this will delete all your content");
                            ContentDialogResult answer = await errorDialog.ShowAsync();
                            if(answer.Equals(ContentDialogResult.Primary))
                            {
                                try
                                {
                                    string commandText = "DROP DATABASE BillMaker";
                                    command = new SqlCommand(commandText, conn);
                                    await command.ExecuteNonQueryAsync();
                                }
                                catch
                                {
                                    MessageBoxDialog Dialog = new MessageBoxDialog("Error!!!!", "Database deletion failed Try Again");
                                    Dialog.PrimaryButtonText = "Ok";
                                    Dialog.IsSecondaryButtonEnabled = false;
                                    Dialog.SecondaryButtonText = "Cancel";
                                    _ = await Dialog.ShowAsync();
                                    isDbNotDeleted = true;
                                }
                            }
                            conn.Close();
                        }
                        if (!isDbNotDeleted)
                        {
                            LoginGrid.Visibility = Visibility.Visible;
                            ConnectionTest.Content = "Connected";
                            ConnectionBtn.Visibility = Visibility.Hidden;
                        }
                        conn.Close();
                    }
                }
                catch(OperationCanceledException)
                {
                    MessageBoxDialog errorDialog = new MessageBoxDialog("Error!!!!", "Server connection timeout please try again");
                    errorDialog.PrimaryButtonText = "Ok";
                    errorDialog.IsSecondaryButtonEnabled = false;
                    errorDialog.SecondaryButtonText = "Cancel";
                    _ = await errorDialog.ShowAsync();
                }
                catch(Exception execption)
                {
                    Console.WriteLine("Error : " + execption.ToString());
                }
                finally
                {
                    ProcessGoingOn.Visibility = Visibility.Hidden;
                    MainGrid.Visibility = Visibility.Visible;
                }
            }
        }

		private async void Login_Button_Click(object sender, RoutedEventArgs e)
		{
            MessageBoxDialog dialog = new MessageBoxDialog("Alert!!!!", "Are You Sure To Install The Product on This PC?");
            ContentDialogResult result = await dialog.ShowAsync();
            if(result.Equals(ContentDialogResult.Secondary))
            {
                Close();
            }
            List<String> licenseResult;
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("Email", UserNameBox.Text);
            keyValuePairs.Add("Password", PasswordBox.Password);
            keyValuePairs.Add("Ip", new DeviceIdBuilder().UseFormatter(new StringDeviceIdFormatter(new PlainTextDeviceIdComponentEncoder())).AddMotherboardSerialNumber().ToString());
            var requestData = JsonConvert.SerializeObject(keyValuePairs);
            var data = new StringContent(requestData, Encoding.UTF8, "application/json");
            string url = "https://licancesmanger.000webhostapp.com/licenceUpdate.php";
            HttpClient client = new HttpClient();
            MainGrid.Visibility = Visibility.Hidden;
            ProcessGoingOn.Visibility = Visibility.Visible;
            var response = await client.PostAsync(url, data);
            string responseResult = response.Content.ReadAsStringAsync().Result;
            licenseResult = JsonConvert.DeserializeObject<List<String>>(responseResult);
            if(licenseResult[1].Equals("Fail"))
            {
                MessageBoxDialog errorDialog = new MessageBoxDialog("Error!!!!", licenseResult[3]) ;
                errorDialog.PrimaryButtonText = "Ok";
                errorDialog.IsSecondaryButtonEnabled = false;
                _ = await errorDialog.ShowAsync();
            }
            else if (licenseResult[1].Equals("AlreadyAdded") || licenseResult[1].Equals("Success"))
            {
                InstallBtn.Visibility = Visibility.Visible;
                insertProductKey += "SET IDENTITY_INSERT [dbo].[CompanySettings] ON \n";
                insertProductKey += "INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES(1, N'ProductKey', N'" + licenseResult[0] + "' ) \n ";
                insertProductKey += "INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES(2, N'RegisteredEmail', N'" + UserNameBox.Text + "' ) \n ";
                insertProductKey += "INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES(3, N'ExpiryDate', N'" + licenseResult[2] + "' ) \n ";
                insertProductKey += "SET IDENTITY_INSERT [dbo].[CompanySettings] OFF \n";
                insertProductKey += "GO \n";
                ProductKeyLbl.Content = "Product Key :  " + licenseResult[0];
                LoginGrid.Visibility = Visibility.Hidden;
            }

            ProcessGoingOn.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(new WindowsPrincipal(WindowsIdentity.GetCurrent()))
             .IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBoxDialog Dialog = new MessageBoxDialog("Error!!!!", "Run this app as Admin. \nRight click on BilMakerDatabase.exe File -> select \"Run as Administrator\"");
                Dialog.PrimaryButtonText = "Ok";
                Dialog.IsSecondaryButtonEnabled = false;
                Dialog.SecondaryButtonText = "Cancel";
                _ = await Dialog.ShowAsync();
                Close();
            }

            GetDataSources();
        }
    }
}
