﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Data;
using System.ComponentModel;
using System.IO;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net.Http;
using BillMaker.FingerPrint;
using ModernWpf.Controls;
using BillMaker.FingerPrint.Formatters;
using BillMaker.FingerPrint.Encoders;
using System.Threading;
using System.Diagnostics;
using System.Security.Principal;
using System.Globalization;
using BillMaker.LicenseArgs;
using Newtonsoft.Json.Linq;

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
            if (ConnectionTest.Content as String == "Installed")
                return;
            MainGrid.Visibility = Visibility.Hidden;
            ProcessGoingOn.Visibility = Visibility.Visible;

            conn.Open();
            string url = "https://licancesmanger.000webhostapp.com/MainDb/mainDB.txt";
            string script = "";
            try
            {
                using (var wc = new HttpClient())
                {
                    wc.Timeout = TimeSpan.FromSeconds(10);
                    script = await wc.GetStringAsync(url);
                }

                if (script.Equals(""))
                {
                    MessageBoxDialog errorDialog = new MessageBoxDialog("Error!!!!", "try again some problem with internet");
                    errorDialog.PrimaryButtonText = "Ok";
                    errorDialog.IsSecondaryButtonEnabled = false;
                    _ = await errorDialog.ShowAsync();
                    ProcessGoingOn.Visibility = Visibility.Hidden;
                    MainGrid.Visibility = Visibility.Visible;
                    conn.Close();
                    return;
                }
            }
            catch(Exception)
			{
                MessageBoxDialog errorDialog = new MessageBoxDialog("Error!!!!", "try again some problem with internet");
                errorDialog.PrimaryButtonText = "Ok";
                errorDialog.IsSecondaryButtonEnabled = false;
                _ = await errorDialog.ShowAsync();
                ProcessGoingOn.Visibility = Visibility.Hidden;
                MainGrid.Visibility = Visibility.Visible;
                conn.Close();
                return;
            }
            script += insertProductKey;
            if (conn.State == ConnectionState.Open)
            {
                try
                {
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
                            ConnectionTest.Content = "Not Installed";
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
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            String SKey = new FingerPrintBuilder().UseFormatter(new StringFingerPrintFormatter(new PlainTextFingerPrintComponentEncoder())).AddSystemUUID().AddProcessorId().AddSystemDriveSerialNumber().AddMotherboardSerialNumber().AddOSInstallationID().ToString();
            SKey = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SKey));
            String MKey = new FingerPrintBuilder().AddSystemUUID().AddProcessorId().AddSystemDriveSerialNumber().AddMotherboardSerialNumber().AddOSInstallationID().ToString();
            LicenseVerifyRequest licenseVerify = new LicenseVerifyRequest(UserNameBox.Text,PasswordBox.Password,MKey,SKey);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://friendcircles.xyz/");
            MainGrid.Visibility = Visibility.Hidden;
            ProcessGoingOn.Visibility = Visibility.Visible;
            HttpResponseMessage response = await client.PutAsJsonAsync<LicenseVerifyRequest>("api/Values",licenseVerify);
            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                LicenseVerifyResponse verifyResponse = await response.Content.ReadAsAsync<LicenseVerifyResponse>();
                InstallBtn.Visibility = Visibility.Visible;
                insertProductKey += "SET IDENTITY_INSERT [dbo].[CompanySettings] ON \n";
                insertProductKey += "INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES(1, N'ProductKey', N'" + verifyResponse.ProductKey + "' ) \n ";
                insertProductKey += "INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES(2, N'RegisteredEmail', N'" + UserNameBox.Text + "' ) \n ";
                insertProductKey += "INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES(3, N'ExpiryDate', N'" + verifyResponse.ExpiryKey + "' ) \n ";
                String xDate = verifyResponse.ProductKey + '.' + verifyResponse.ExpiryKey + '.' + DateTime.Now.Date.ToString("dd-MM-yyyy");
                xDate = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(xDate));
                insertProductKey += "INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES(4, N'xDate', N'" + xDate + "' ) \n ";
                insertProductKey += "INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES(5, N'mData', N'" + MKey + "' ) \n ";
                insertProductKey += "SET IDENTITY_INSERT [dbo].[CompanySettings] OFF \n";
                insertProductKey += "GO \n";
                ProductKeyLbl.Content = "Product Key :  " + verifyResponse.ProductKey;
                LoginGrid.Visibility = Visibility.Hidden;
                
            }
            else
            {
                String jSon = await response.Content.ReadAsStringAsync();
                var JsonArray = JObject.Parse(jSon);
                String ErrorString = Convert.ToString(JsonArray["Message"]);
                MessageBoxDialog errorDialog = new MessageBoxDialog("Error!!!!", ErrorString);
                errorDialog.PrimaryButtonText = "Ok";
                errorDialog.IsSecondaryButtonEnabled = false;
                _ = await errorDialog.ShowAsync();
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
