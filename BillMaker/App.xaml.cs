using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using log4net;
using Microsoft.Win32;
using BillMaker.DataLib;

namespace BillMaker
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(App));
		protected override void OnStartup(StartupEventArgs e)
		{
			log4net.Config.XmlConfigurator.Configure();
			log.Info("        =============  Started Logging  =============        ");
            SplashScreen splashScreen = new SplashScreen("Asset/Splash.png");
            splashScreen.Show(true);
            base.OnStartup(e);
            Configuration config =  ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringsSection section = config.ConnectionStrings;
            log.Info( CultureInfo.CurrentCulture);
            if (section.ConnectionStrings["BillMakerEntities"].ConnectionString.Contains(@"(local)\HEREGOESSERVERNAME"))
            {
                log.Error(" Server Name is Not added in the Configuration File");
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("        =============  End Logging  ============        ");
        }
    }

    public sealed class ParametrizedBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;

            if (value is bool)
                flag = (bool)value;
            else
            {
                if (value is bool?)
                {
                    bool? flag2 = (bool?)value;
                    flag = (flag2.HasValue && flag2.Value);
                }
            }

            //If false is passed as a converter parameter then reverse the value of input value
            if (parameter != null)
            {
                bool par = true;
                if ((bool.TryParse(parameter.ToString(), out par)) && (!par)) flag = !flag;
            }

            return flag ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
                return (Visibility)value == Visibility.Visible;

            return false;
        }

        public ParametrizedBooleanToVisibilityConverter()
        {
        }
    }
}
