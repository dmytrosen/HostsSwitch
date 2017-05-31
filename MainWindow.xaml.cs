using HostSwitch.Controls;
using HostSwitch.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace HostSwitch
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string CONFIG_FILE_NAME = "env.config";
		internal readonly string REPO_PATH = Environment.CurrentDirectory + "\\repo";
		internal new bool IsLoaded { get; private set; }
		private EAppMode appMode = EAppMode.Config;

		internal IEnumerable<EnvSet> EnvSets { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			SettingControl.LoadConfig();
			AddFootNote("application load started", EMessagingLevel.Verbose);
			ParseConfigs();
			AddFootNote("application load completed", EMessagingLevel.Verbose);
			IsLoaded = true;
		}

		internal void UpdateUi(HostsFileControl ctrl)
		{
			if (!(ctrl != null
				&& ctrl.EnvSet != null))
			{
				return;
			}

			bool enableBtnSwitch = false;
			bool enableBtnReset = false;

			if (ctrl.EnvSet.SelectedHostFile == null) // first load, no match; no change is made
			{
				enableBtnSwitch = false;
				enableBtnReset = false;
			}
			else if (ctrl.EnvSet.CurrentHostFile == null) // first load, no match;; change is made
			{
				enableBtnSwitch = true;
				enableBtnReset = false;
			}
			else if (ctrl.EnvSet.CurrentHostFile.Index == ctrl.EnvSet.SelectedHostFile.Index) // selection same as current
			{
				enableBtnSwitch = false;
				enableBtnReset = false;
			}
			else // selection is dif from current
			{
				enableBtnSwitch = true;
				enableBtnReset = true;
			}
			ctrl.UpdateUi(enableBtnSwitch, enableBtnReset);
		}

		internal void AddFootNote(string note, EMessagingLevel requestedMessagingLevel)
		{
            if (requestedMessagingLevel < SettingControl.ConfigAppSettings.MessagingLevel) // do not show messages 
			{
				return;
			}

            string toAdd = DateTime.Now.ToString() + ": " + note.Replace("\n", " ").Replace("\r", string.Empty);

			Result.Items.Add(new TextBlock
            {
                Text = toAdd,
                ToolTip = toAdd,
                Foreground = new SolidColorBrush(requestedMessagingLevel == EMessagingLevel.Error ? Colors.Red : Colors.Black)
            });
			Result.ScrollIntoView(Result.Items[Result.Items.Count - 1]);
		}

        private void ParseConfigs()
		{
            MainLoadingPanel.Visibility = Visibility.Visible;
            MainButtonsPanel.Visibility = Visibility.Collapsed;
            EnvsTabControl.Items.Clear();
            EnvsTabControl.UpdateLayout();

            string[] envsPaths = Directory.GetDirectories(REPO_PATH);

            if (!(envsPaths != null
                && envsPaths.Count() > 0))
            {
                AddFootNote("configs repository is either ivalid or empty", EMessagingLevel.Error);
                return;
            }
            LoadingProgressBar.Maximum = envsPaths.Count();
            LoadingProgressBar.Value = 0;
            EnvSets = new List<EnvSet>();
            // run IO part in separate thread 
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.WorkerSupportsCancellation = false;
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += (s, e) =>
                {
                    if (e.UserState is EnvSet) // env load complete
                    {
                        EnvSet envSet = e.UserState as EnvSet;
                        // check for dups
                        EnvSet existedItem = EnvSets.FirstOrDefault(i => i.IsValid
                            && (string.Compare(i.EnvHost.Trim(), envSet.EnvHost.Trim(), true) == 0
                            || string.Compare(i.EnvHostPath.Trim(), envSet.EnvHostPath.Trim(), true) == 0
                            || string.Compare(i.Title.Trim(), envSet.Title.Trim(), true) == 0));

                        if (existedItem != null)
                        {
                            AddFootNote("ignoring duplicate config " + envSet.Title 
                                + " of already loaded environment [Title: " + existedItem.Title 
                                + ", EnvHost: " + existedItem.EnvHost + ", EnvHostPath: " 
                                + existedItem.EnvHostPath + ", ]", EMessagingLevel.Result);
                            return;
                        }
                        
                        HostsFileControl hostsFileControl = new HostsFileControl { EnvSet = envSet };
                        TabItem envTab = new TabItem();

                        envTab.Header = new TextBlock
                        {
                            Text = envSet.Title,
                            Foreground = new SolidColorBrush(envSet.IsValid ? Colors.Black : Colors.Red)
                        };
                        envTab.Content = hostsFileControl;
                        // add host files to DDL
                        hostsFileControl.CurrentEnvPath.Text = envSet.EnvHostPath;
                        hostsFileControl.ServerName.Text = envSet.EnvHost;
                        hostsFileControl.ConfigPath.Text = envSet.ConfigPath;
                        hostsFileControl.EnvsDropDown.Items.Clear();
                        if (envSet.IsValid)
                        {
                            foreach (AvailableHostFile hostFile in envSet.AvailableHostFiles) // set hosts files selector
                            {
                                hostsFileControl.EnvsDropDown.Items.Add(new ComboBoxItem
                                {
                                    Content = hostFile,
                                    Name = hostFile.Name.Replace(" ", "_")
                                });
                            }
                            if (envSet.CurrentHostFile != null) // set current index if any
                            {
                                hostsFileControl.EnvsDropDown.SelectedIndex = envSet.CurrentHostFile.Index;
                                hostsFileControl.CurrentEnvContent.Text = File.ReadAllText(envSet.CurrentHostFile.FullPath);
                                hostsFileControl.CurrentEnvContent.Foreground = new SolidColorBrush(Colors.Black);
                                hostsFileControl.CurrentEnvTitle.Text = envSet.CurrentHostFile.Name;
                            }
                            else
                            {
                                hostsFileControl.CurrentEnvContent.Text = envSet.OriginalHostFileContent;
                                hostsFileControl.CurrentEnvContent.Foreground = new SolidColorBrush(Colors.Blue);
                                hostsFileControl.CurrentEnvTitle.Text = "(DOES NOT MATCH ANY FILE IN CONFIGURATION LIST)";
                            }
                            hostsFileControl.EnvsDropDown.IsEnabled = true;
                        }
                        else
                        {
                            hostsFileControl.CurrentEnvContent.Text = envSet.AllMessages;
                            hostsFileControl.CurrentEnvContent.Foreground = new SolidColorBrush(Colors.Red);
                            hostsFileControl.EnvsDropDown.IsEnabled = false;
                            hostsFileControl.CurrentEnvTitle.Text = "(UNABLE TO ANALYZE)";
                        }
                        UpdateUi(hostsFileControl);
                        EnvsTabControl.Items.Add(envTab);
                        AddFootNote(envSet.Title + " environment loaded", EMessagingLevel.Verbose);
                        LoadingProgressBar.Value = e.ProgressPercentage;
                        LoadingProgressBar.UpdateLayout();
                        ((List<EnvSet>)EnvSets).Add(envSet);
                        if (EnvsTabControl.SelectedIndex < 0)
                        {
                            EnvsTabControl.SelectedIndex = 0;
                        }
                    }
                    else if (e.UserState is KeyValuePair<EMessagingLevel, string>) // error message
                    {
                        KeyValuePair<EMessagingLevel, string> log = (KeyValuePair<EMessagingLevel, string>)e.UserState;

                        AddFootNote(log.Value, log.Key);
                        LoadingText.Text = log.Value;
                        LoadingProgressBar.Value = e.ProgressPercentage;
                    }
                    else if (e.UserState is string) // independent message
                    {
                        LoadingText.Text = e.UserState as string;
                        LoadingText.ToolTip = e.UserState as string;
                    }
                };
                bw.RunWorkerCompleted += (s, e) =>
                {
                    MainLoadingPanel.Visibility = Visibility.Collapsed;
                    MainButtonsPanel.Visibility = Visibility.Visible;
                };
                bw.DoWork += (s, e) =>
                {
                    BackgroundWorker worker = s as BackgroundWorker;
                    IEnumerable<EnvSet> envSets = Directory.GetDirectories(REPO_PATH).Select(i => new EnvSet
                    {
                        ConfigPath = i,
                        Title = i.Replace(REPO_PATH + "\\", string.Empty),
                        AvailableHostFiles = new List<AvailableHostFile>()
                    }).ToArray();
                    int counter = 0;

                    foreach (EnvSet envSet in envSets)
                    {
                        worker.ReportProgress(counter, "loading config " + envSet.ConfigPath);
                        // vadidate envconfig
                        try
                        {
                            envSet.IsValid = true;
                            using (FileStream fs = new FileStream(envSet.ConfigPath + "\\" + CONFIG_FILE_NAME, FileMode.Open))
                            {
                                if (fs == null)
                                {
                                    string message = "missing, locked or corrupded config " + envSet.ConfigPath + "\\" + CONFIG_FILE_NAME;

                                    envSet.Messages.Add(message);
                                    worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                                    envSet.Title = envSet.Title.Replace("_", " ");
                                    envSet.IsValid = false;
                                }
                                else
                                {
                                    Env env = (Env)(new XmlSerializer(typeof(Env))).Deserialize(fs);

                                    envSet.EnvHostPath = env.EnvHostPath;
                                    envSet.EnvHost = env.EnvHost;
                                    if (!string.IsNullOrWhiteSpace(env.Title))
                                    {
                                        envSet.Title = env.Title;
                                    }
                                    // check if env host properties are valid
                                    if (string.IsNullOrWhiteSpace(envSet.ConfigPath))
                                    {
                                        string message = "EnvPath property of " + envSet.Title + " is undefined";

                                        envSet.Messages.Add(message);
                                        worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                                        envSet.IsValid = false;
                                    }
                                    if (string.IsNullOrWhiteSpace(envSet.EnvHost))
                                    {
                                        string message = "EnvHost property of " + envSet.Title + " is undefined";

                                        envSet.Messages.Add(message);
                                        worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                                        envSet.IsValid = false;
                                    }
                                    if (string.IsNullOrWhiteSpace(envSet.EnvHostPath))
                                    {
                                        string message = "EnvHostPath property of " + envSet.Title + " is undefined";

                                        envSet.Messages.Add(message);
                                        worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                                        envSet.IsValid = false;
                                    }
                                }
                            }
                        }
                        catch 
                        {
                            string message = "fail to load config " + envSet.ConfigPath + "\\" + CONFIG_FILE_NAME;

                            envSet.Messages.Add(message);
                            worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                            envSet.Title = envSet.Title.Replace("_", " ");
                            envSet.IsValid = false;
                        }
                        if (!envSet.IsValid) // no sense to validate server because config failes
                        {
                            worker.ReportProgress(++counter, envSet);
                            continue;
                        }

                        // validate server
                        if (!Directory.Exists(envSet.EnvHostPath)) // missconfigured or unreachable server
                        {
                            string message = "hosts file path " + envSet.EnvHostPath + " does not exist or unreachable";

                            envSet.IsValid = false;
                            envSet.Messages.Add(message);
                            worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                            worker.ReportProgress(++counter, envSet);
                            continue;
                        }

                        // validate choices
                        IEnumerable<AvailableHostFile> availHostFiles = Directory.GetFiles(envSet.ConfigPath)
                            .Where(i => !i.Contains(CONFIG_FILE_NAME))
                            .Select(i => new AvailableHostFile
                            {
                                Parent = envSet,
                                Index = int.Parse(i.Replace(envSet.ConfigPath + "\\", string.Empty).Substring(0, 1)),
                                Name = i.Replace(envSet.ConfigPath + "\\", string.Empty).Substring(2).Replace("_", " "),
                                FullPath = i
                            }).ToArray();

                        if (availHostFiles.Count() == 0)
                        {
                            string message = "config path " + envSet.ConfigPath + " has no alternative host file(s)";

                            envSet.IsValid = false;
                            envSet.Messages.Add(message);
                            worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                        }
                        else
                        {
                            // parse available hostfiles
                            envSet.OriginalHostFileContent = (File.ReadAllText(envSet.EnvHostPath + "\\hosts") ?? string.Empty).Trim();
                            foreach (AvailableHostFile availHostFile in availHostFiles)
                            {
                                try
                                {
                                    // read available host file and compare to env host file
                                    string availHostFileContent = File.ReadAllText(availHostFile.FullPath);

                                    if (availHostFileContent != null
                                        && string.Compare(availHostFileContent.Trim(), envSet.OriginalHostFileContent, true) == 0) // match
                                    {
                                        envSet.CurrentHostFile = availHostFile;
                                    }
                                    envSet.AvailableHostFiles.Add(availHostFile);
                                }
                                catch (Exception ex)
                                {
                                    worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, "cannot read alternative host file " + availHostFile.FullPath + ". " + ex.Message));
                                    continue;
                                }
                            }
                        }
                        worker.ReportProgress(++counter, envSet);
                    }
                };
                if (bw.IsBusy != true)
                {
                    bw.RunWorkerAsync();
                }
            }
        }

		private void OnBtnRefreshClick(object sender, RoutedEventArgs e)
		{
			IsLoaded = false;
			AddFootNote("application repository refresh started", EMessagingLevel.Verbose);
			SettingControl.LoadConfig();
            ParseConfigs();
			AddFootNote("application repository refresh completed", EMessagingLevel.Verbose);
			IsLoaded = true;
		}

		private void OnBtnCloseClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OnEnvsTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				AddFootNote(((TabItem)((TabControl)sender).SelectedItem).Header + " environment has been selected", EMessagingLevel.Verbose);
			}
		}

		private void OnButtonHelpClick(object sender, RoutedEventArgs e)
		{
			appMode = EAppMode.Help;
			UpdateUiMode();
		}

		private void OnButtonSetupClick(object sender, RoutedEventArgs e)
		{
			appMode = EAppMode.Settings;
			SettingControl.LoadConfig();
			UpdateUiMode();
		}

		private void OnSaveSettingsBtnClick(object sender, RoutedEventArgs e)
		{
			if (SettingControl.SaveConfig())
			{
				AddFootNote("app config saved", EMessagingLevel.Verbose);
				appMode = EAppMode.Config;
			}
			else
			{
				AddFootNote("failure to save app config", EMessagingLevel.Error);
			}
			UpdateUiMode();
		}

		private void OnCancelSettingsBtnClick(object sender, RoutedEventArgs e)
		{
			if (SettingControl.HasChanges)
			{
				AddFootNote("all settings changes have been canceled", EMessagingLevel.Result);
			}
			appMode = EAppMode.Config;
			UpdateUiMode();
		}

		private void OnCancelHelpBtnClick(object sender, RoutedEventArgs e)
		{
			appMode = EAppMode.Config;
			UpdateUiMode();
		}

		private void UpdateUiMode()
		{
			if (appMode == EAppMode.Settings)
			{
				EnvsTabControl.Visibility = Visibility.Collapsed;
				HelpControl.Visibility = Visibility.Collapsed;
				SettingControl.Visibility = Visibility.Visible;
				CloseBtn.Visibility = Visibility.Collapsed;
				RefreshBtn.Visibility = Visibility.Collapsed;
				SaveSettingsBtn.Visibility = Visibility.Visible;
				CancelSettingsBtn.Visibility = Visibility.Visible;
				CancelHelpBtn.Visibility = Visibility.Collapsed;
                MainBtnsColumn1.Width = new GridLength(0);
                MainBtnsColumn2.Width = new GridLength(75);
            }
            else if (appMode == EAppMode.Help)
			{
				EnvsTabControl.Visibility = Visibility.Collapsed;
				HelpControl.Visibility = Visibility.Visible;
				SettingControl.Visibility = Visibility.Collapsed;
				CloseBtn.Visibility = Visibility.Collapsed;
				RefreshBtn.Visibility = Visibility.Collapsed;
				SaveSettingsBtn.Visibility = Visibility.Collapsed;
				CancelSettingsBtn.Visibility = Visibility.Collapsed;
				CancelHelpBtn.Visibility = Visibility.Visible;
                MainBtnsColumn1.Width = new GridLength(0);
                MainBtnsColumn2.Width = new GridLength(0);
            }
            else
			{
				EnvsTabControl.Visibility = Visibility.Visible;
				HelpControl.Visibility = Visibility.Collapsed;
				SettingControl.Visibility = Visibility.Collapsed;
				CloseBtn.Visibility = Visibility.Visible;
				RefreshBtn.Visibility = Visibility.Visible;
				SaveSettingsBtn.Visibility = Visibility.Collapsed;
				CancelSettingsBtn.Visibility = Visibility.Collapsed;
				CancelHelpBtn.Visibility = Visibility.Collapsed;
                MainBtnsColumn1.Width = new GridLength(35);
                MainBtnsColumn2.Width = new GridLength(75);
            }
        }
	}
}
