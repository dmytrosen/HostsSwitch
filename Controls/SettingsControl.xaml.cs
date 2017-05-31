using HostSwitch.Entities;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Serialization;

namespace HostSwitch.Controls
{
	/// <summary>
    /// Settings screen. 
	/// Interaction logic for SettingsControl.xaml
	/// </summary>
	public partial class SettingsControl : UserControl
	{
		private const string CONFIG_FILE_NAME = "AppSettings.config";

		internal AppSettings ConfigAppSettings { get; private set; }

		private AppSettings currentAppSettings = null;

		public SettingsControl()
		{
			InitializeComponent();
			UpdateUi();
		}

		internal void LoadConfig()
		{
			string configName = ((MainWindow)App.Current.MainWindow).REPO_PATH + "\\" + CONFIG_FILE_NAME;
			MainWindow mainWindow = (MainWindow)App.Current.MainWindow;

            if (currentAppSettings?.ClearLogsOnRefresh == true)
            {
                mainWindow.Result.Items.Clear();
            }
            try
			{
				// read settings config
				using (FileStream fs = new FileStream(configName, FileMode.Open))
				{
					currentAppSettings = (AppSettings)(new XmlSerializer(typeof(AppSettings))).Deserialize(fs);
					ConfigAppSettings = (AppSettings)currentAppSettings.Clone();
					mainWindow.AddFootNote("app config: " + configName + " has been loaded", EMessagingLevel.Verbose);
                    MessageSelector.SelectedIndex = (int)currentAppSettings.MessagingLevel;
                    ClearLogsCheckBox.IsChecked = currentAppSettings.ClearLogsOnRefresh;
                }
            }
			catch 
			{
				currentAppSettings = new AppSettings { MessagingLevel = EMessagingLevel.Verbose };
				ConfigAppSettings = (AppSettings)currentAppSettings.Clone();
				mainWindow.AddFootNote("faild to load app config: " + configName, EMessagingLevel.Error);
				SaveConfig();
			}
			UpdateUi();
		}

		internal bool HasChanges => ConfigAppSettings != currentAppSettings;

	    private void UpdateUi()
		{
			MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

			if (mainWindow.IsLoaded)
			{
				mainWindow.SaveSettingsBtn.IsEnabled = HasChanges;
			}
		}

		private void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			MainWindow mainWindow = (MainWindow)App.Current.MainWindow;

			if (mainWindow.IsLoaded)
			{
                currentAppSettings.MessagingLevel = (EMessagingLevel)((ComboBox)sender).SelectedIndex;
				mainWindow.AddFootNote("new messaging Level has been selected: " + currentAppSettings.MessagingLevel.ToString(), EMessagingLevel.Result);
				UpdateUi();
			}
		}

        private void OnClearLogsCheckBoxClick(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)App.Current.MainWindow;
            CheckBox cleaLogsOnRefreshControl = (CheckBox)sender;

            if (mainWindow.IsLoaded)
            {
                currentAppSettings.ClearLogsOnRefresh = cleaLogsOnRefreshControl.IsChecked ?? false;
                mainWindow.AddFootNote("Clear Logs On Refresh is set to : " + currentAppSettings.ClearLogsOnRefresh.ToString(), EMessagingLevel.Result);
                UpdateUi();
            }
        }

        private void OnClearLogsButtonClick(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)App.Current.MainWindow;

            if (mainWindow.IsLoaded)
            {
                mainWindow.Result.Items.Clear();
            }
        }

        internal bool SaveConfig()
		{
			string configName = ((MainWindow)App.Current.MainWindow).REPO_PATH + "\\" + CONFIG_FILE_NAME;
			MainWindow mainWindow = (MainWindow)App.Current.MainWindow;
			bool isSuccess = false;

			try
			{
				// save settings config
				if (File.Exists(configName))
				{
					File.Delete(configName);
				}
				using (FileStream fs = new FileStream(configName, FileMode.Create))
				{
					(new XmlSerializer(typeof(AppSettings))).Serialize(fs, currentAppSettings);
					ConfigAppSettings = (AppSettings)currentAppSettings.Clone();
				}
				isSuccess = true;
			}
			catch 
			{
				mainWindow.AddFootNote("faild to save app config: " + configName, EMessagingLevel.Error);
				isSuccess = false;
			}
			UpdateUi();
			return isSuccess;
		}
    }
}
