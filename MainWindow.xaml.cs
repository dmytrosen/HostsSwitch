using HostSwitch.Controls;
using HostSwitch.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		internal readonly string REPO_PATH = $"{Environment.CurrentDirectory}\\repo";

        private bool IsLeftPanelExpanded = false;

        public static readonly DependencyProperty AppModeProperty = DependencyProperty.Register("AppMode", typeof(EAppMode),
            typeof(MainWindow), new FrameworkPropertyMetadata(EAppMode.Config));

        public EAppMode AppMode
        {
            get { return (EAppMode)GetValue(AppModeProperty); }
            set { SetValue(AppModeProperty, value); }
        }

        internal new bool IsLoaded { get; private set; }
        public EnvSets Environments { get; private set; } = new EnvSets();
        public ObservableCollection<ListViewItem> Results { get; private set; } = new ObservableCollection<ListViewItem>();

		public MainWindow()
		{
			InitializeComponent();
            DataContext = this;
            SettingControl.LoadConfig();
            AddFootNote("application load started", EMessagingLevel.Verbose);
            ParseConfigs();
            AddFootNote("application load completed", EMessagingLevel.Verbose);
            UpdateUiMode();
        }

        internal void AddFootNote(string note, EMessagingLevel requestedMessagingLevel)
		{
            if (requestedMessagingLevel < SettingControl.ConfigAppSettings.MessagingLevel) // do not show messages 
			{
				return;
			}

            string toAdd = $"{DateTime.Now}: {note.Replace("\n", " ").Replace("\r", string.Empty)}";

            Results.Add(new ListViewItem()
            {
                Content = new TextBlock
                {
                    Text = toAdd,
                    ToolTip = toAdd,
                    Foreground = new SolidColorBrush(requestedMessagingLevel == EMessagingLevel.Error ? Colors.Pink : Colors.White)
                }
            });
            if (Results?.Count > 0
                && Result?.Items?.Count > 0)
            {
                Result.ScrollIntoView(Result.Items[Result.Items.Count - 1]);
                Result.SelectedIndex = Result.Items.Count - 1;
            }
		}

        private void ParseConfigs()
		{
            MainLoadingPanel.Visibility = Visibility.Visible;
            MainButtonsPanel.Visibility = Visibility.Collapsed;
            Environments.Clear();

            string[] envsPaths = Directory.GetDirectories(REPO_PATH);

            if (!(envsPaths != null
                && envsPaths.Count() > 0))
            {
                AddFootNote("configs repository is either ivalid or empty", EMessagingLevel.Error);
                return;
            }
            LoadingProgressBar.Maximum = envsPaths.Count();
            LoadingProgressBar.Value = 0;
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
                        EnvSet existedItem = Environments.FirstOrDefault(i => i.IsValid
                            && (string.Compare(i.EnvHost.Trim(), envSet.EnvHost.Trim(), true) == 0
                            || string.Compare(i.EnvHostPath.Trim(), envSet.EnvHostPath.Trim(), true) == 0
                            || string.Compare(i.Title.Trim(), envSet.Title.Trim(), true) == 0));

                        if (existedItem != null)
                        {
                            AddFootNote($"ignoring duplicate config {envSet.Title} of already loaded environment [Title: {existedItem.Title}, EnvHost: {existedItem.EnvHost}, EnvHostPath: {existedItem.EnvHostPath}]", EMessagingLevel.Result);
                            return;
                        }
                        if (envSet.IsValid)
                        {
                            AddFootNote($"{envSet.Title} environment loaded", EMessagingLevel.Verbose);
                        }
                        else
                        {
                            envSet.CurrentEnvContent = envSet.AllMessages;
                            envSet.AvailableHostFiles = new ObservableCollection<AvailableHostFile>();
                            AddFootNote($"{envSet.Title} environment is not loaded properly", EMessagingLevel.Error);
                        }
                        LoadingProgressBar.Value = e.ProgressPercentage;
                        LoadingProgressBar.UpdateLayout();
                        Environments.Add(envSet);
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
                    IsLoaded = true;
                };
                bw.DoWork += (s, e) =>
                {
                    BackgroundWorker worker = s as BackgroundWorker;
                    IEnumerable<EnvSet> Environments = Directory.GetDirectories(REPO_PATH).Select(i => new EnvSet
                    {
                        ConfigPath = i,
                        Title = i.Replace($"{REPO_PATH}\\", string.Empty),
                        AvailableHostFiles = new ObservableCollection<AvailableHostFile>()
                    }).ToArray();
                    int counter = 0;

                    foreach (EnvSet envSet in Environments)
                    {
                        worker.ReportProgress(counter, $"loading config {envSet.ConfigPath}");
                        // vadidate envconfig
                        try
                        {
                            envSet.IsValid = true;
                            using (FileStream fs = new FileStream($"{envSet.ConfigPath}\\{CONFIG_FILE_NAME}", FileMode.Open))
                            {
                                if (fs == null)
                                {
                                    string message = $"missing, locked or corrupded config {envSet.ConfigPath}\\{CONFIG_FILE_NAME}";

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
                                        string message = $"EnvPath property of {envSet.Title} is undefined";

                                        envSet.Messages.Add(message);
                                        worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                                        envSet.IsValid = false;
                                    }
                                    if (string.IsNullOrWhiteSpace(envSet.EnvHost))
                                    {
                                        string message = $"EnvHost property of {envSet.Title} is undefined";

                                        envSet.Messages.Add(message);
                                        worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                                        envSet.IsValid = false;
                                    }
                                    if (string.IsNullOrWhiteSpace(envSet.EnvHostPath))
                                    {
                                        string message = $"EnvHostPath property of {envSet.Title} is undefined";

                                        envSet.Messages.Add(message);
                                        worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                                        envSet.IsValid = false;
                                    }
                                }
                            }
                        }
                        catch 
                        {
                            string message = $"fail to load config {envSet.ConfigPath}\\{CONFIG_FILE_NAME}";

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
                            string message = $"hosts file path {envSet.EnvHostPath} does not exist or unreachable";

                            envSet.IsValid = false;
                            envSet.Messages.Add(message);
                            worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                            worker.ReportProgress(++counter, envSet);
                            continue;
                        }

                        // validate choices
                        ObservableCollection<AvailableHostFile> availHostFiles = new ObservableCollection<AvailableHostFile>(Directory.GetFiles(envSet.ConfigPath)
                            .Where(i => !i.Contains(CONFIG_FILE_NAME))
                            .Select(i => new AvailableHostFile
                            {
                                Parent = envSet,
                                Index = int.Parse(i.Replace($"{envSet.ConfigPath}\\", string.Empty).Substring(0, 1)),
                                Name = i.Replace($"{envSet.ConfigPath}\\", string.Empty).Substring(2).Replace("_", " "),
                                FullPath = i
                            }).ToArray());

                        if (availHostFiles.Count() == 0)
                        {
                            string message = $"config path {envSet.ConfigPath} has no alternative host file(s)";

                            envSet.IsValid = false;
                            envSet.Messages.Add(message);
                            worker.ReportProgress(counter, new KeyValuePair<EMessagingLevel, string>(EMessagingLevel.Error, message));
                        }
                        else
                        {
                            // parse available hostfiles
                            envSet.OriginalHostFileContent = (File.ReadAllText($"{envSet.EnvHostPath}\\hosts") ?? string.Empty).Trim();
                            foreach (AvailableHostFile availHostFile in availHostFiles)
                            {
                                try
                                {
                                    // read available host file and compare to env host file
                                    string availHostFileContent = (File.ReadAllText(availHostFile.FullPath) ?? string.Empty).Trim();

                                    if (availHostFileContent != null
                                        && string.Compare(availHostFileContent.Trim(), envSet.OriginalHostFileContent, true) == 0) // match
                                    {
                                        envSet.CurrentHostFile = availHostFile;
                                        envSet.SelectedHostFile = envSet.CurrentHostFile;
                                    }
                                    envSet.CurrentEnvContent = availHostFileContent;
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
            e.Handled = true;
            IsLoaded = false;
            MainButtonsPanel.Visibility = Visibility.Collapsed;
            AddFootNote("application repository refresh started", EMessagingLevel.Verbose);
			SettingControl.LoadConfig();
            ParseConfigs();
			AddFootNote("application repository refresh completed", EMessagingLevel.Verbose);
			IsLoaded = true;
		}

		private void OnBtnCloseClick(object sender, RoutedEventArgs e)
		{
            e.Handled = true;
            Close();
		}

		private void OnEnvsTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            e.Handled = true;
            if (IsLoaded)
			{
				AddFootNote($"{((EnvSet)((TabControl)sender).SelectedItem).Title} environment has been selected", EMessagingLevel.Verbose);
			}
		}

		private void OnButtonHelpClick(object sender, RoutedEventArgs e)
		{
            e.Handled = true;
            AppMode = EAppMode.Help;
			UpdateUiMode();
		}

		private void OnButtonSetupClick(object sender, RoutedEventArgs e)
		{
            e.Handled = true;
            AppMode = EAppMode.Settings;
			SettingControl.LoadConfig();
			UpdateUiMode();
		}

		private void OnSaveSettingsBtnClick(object sender, RoutedEventArgs e)
		{
            e.Handled = true;
            if (SettingControl.SaveConfig())
			{
				AddFootNote("app config saved", EMessagingLevel.Verbose);
				AppMode = EAppMode.Config;
			}
			else
			{
				AddFootNote("failure to save app config", EMessagingLevel.Error);
			}
			UpdateUiMode();
		}

		private void OnCancelBtnClick(object sender, RoutedEventArgs e)
		{
            e.Handled = true;
            if (SettingControl.HasChanges)
			{
				AddFootNote("all settings changes have been canceled", EMessagingLevel.Result);
			}
			AppMode = EAppMode.Config;
			UpdateUiMode();
		}

		private void UpdateUiMode()
		{
			if (AppMode == EAppMode.Settings)
			{
				CloseBtn.Visibility = Visibility.Collapsed;
				RefreshBtn.Visibility = Visibility.Collapsed;
				SaveSettingsBtn.Visibility = Visibility.Visible;
				DiscardSettingsBtn.Visibility = Visibility.Visible;
                MainBtnsColumn2.Width = new GridLength(75);
                MainBtnsColumn1.Width = new GridLength(75);
            }
            else if (AppMode == EAppMode.Help)
			{
				CloseBtn.Visibility = Visibility.Collapsed;
				RefreshBtn.Visibility = Visibility.Collapsed;
				SaveSettingsBtn.Visibility = Visibility.Collapsed;
				DiscardSettingsBtn.Visibility = Visibility.Collapsed;
                MainBtnsColumn2.Width = new GridLength(0);
                MainBtnsColumn1.Width = new GridLength(0);
            }
            else
			{
				CloseBtn.Visibility = Visibility.Visible;
				RefreshBtn.Visibility = Visibility.Visible;
				SaveSettingsBtn.Visibility = Visibility.Collapsed;
				DiscardSettingsBtn.Visibility = Visibility.Collapsed;
                MainBtnsColumn2.Width = new GridLength(0);
                MainBtnsColumn1.Width = new GridLength(75);
            }
        }

        private void OnExpandBtnClick(object sender, RoutedEventArgs e)
        {
            IsLeftPanelExpanded = !IsLeftPanelExpanded;
            RootGrid.ColumnDefinitions[0].Width = new GridLength(IsLeftPanelExpanded ? 125 : 40);
        }
    }
}
