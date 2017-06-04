using HostSwitch;
using HostSwitch.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
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

namespace HostSwitch.Controls
{
    /// <summary>
    /// Host file control is Tab on main screen. 
    /// Interaction logic for HostsFileControl.xaml
    /// </summary>
    public partial class HostsFileControl : UserControl
    {
        private const string REMOTE_PROCESS_START_FILE_NAME = "C:\\Windows\\System32\\cmd.exe";
        private const string REMOTE_PROCESS_START_ARGUMENTS = "\"psexec {0} C:\\Windows\\System32\\cmd.exe /c ipconfig.exe /flushdns\"";

        MainWindow mainWindow = (MainWindow)App.Current.MainWindow;

        public static readonly DependencyProperty EnvironmentProperty = DependencyProperty.Register("Environment", typeof(EnvSet),
            typeof(HostsFileControl), new FrameworkPropertyMetadata(null));

        public EnvSet Environment
        {
            get { return (EnvSet)GetValue(EnvironmentProperty); }
            set { SetValue(EnvironmentProperty, value); }
        }

        public HostsFileControl()
        {
            InitializeComponent();
            UpdateUi();
        }

        private void UpdateUi()
        {
            (bool btnSwitchState, bool btnResetState) = Environment?.UpdateUi() ?? (false, false);
            BtnSwitch.IsEnabled = btnSwitchState;
            BtnReset.IsEnabled = btnResetState;
        }

        private string ExecuteCommand(string serverName)
        {
            string result = null;

            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = REMOTE_PROCESS_START_FILE_NAME,
                    Arguments = string.Format(REMOTE_PROCESS_START_ARGUMENTS, serverName)
                };
                process.Start();
                process.WaitForExit();
                if (process.ExitCode == 0 && null != process && process.HasExited)
                {
                    result = process.StandardOutput.ReadToEnd();
                }
                else
                {
                    result = "Error: ";
                }
                return result;
            }
        }

        private void OnEnvsDropDownSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) // if any update, set new environment from env DDL
            {
                AvailableHostFile host = e.AddedItems[0] as AvailableHostFile;

                host.Parent.CurrentEnvContent = File.ReadAllText(host.FullPath);
                host.Parent.SelectedHostFile = host;
                mainWindow.AddFootNote(host.Name + " config has been selected on " + host.Parent.Title, EMessagingLevel.Verbose);
                UpdateUi();
            }
            e.Handled = true;
        }

        private void OnBtnResetClick(object sender, RoutedEventArgs e)
        {
            Environment.SelectedHostFile = Environment.CurrentHostFile;
            UpdateUi();
        }

        private void OnBtnSwitchClick(object sender, RoutedEventArgs e)
        {
            // copy selected file to destination
            try
            {
                File.Delete(Environment.EnvHostPath + "\\hosts");
            }
            catch (Exception ex)
            {
                mainWindow.AddFootNote($"Fatal Error of hostfile replacement. Unable to delete file {Environment.EnvHostPath}\\hosts. Reason: {ex.Message}", EMessagingLevel.Error);
            }
            try
            {
                File.Copy(Environment.SelectedHostFile.FullPath, Environment.EnvHostPath + "\\hosts");
            }
            catch (Exception ex)
            {
                mainWindow.AddFootNote($"Fatal Error of hostfile replacement. Unable to copy new file {Environment.EnvHostPath}\\hosts. Reason: {ex.Message}", EMessagingLevel.Error);
            }
            // Commit change of environment
            Environment.CurrentHostFile = Environment.SelectedHostFile;

            // refresh DNS on local or remote server
            string result = ExecuteCommand(Environment.EnvHost);

            if (!string.IsNullOrWhiteSpace(result))
            {
                mainWindow.AddFootNote(Environment.SelectedHostFile.FullPath + " hostfiles set", EMessagingLevel.Result);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    mainWindow.AddFootNote(result.Trim(), EMessagingLevel.Result);
                }
            }
            else // show error on UI
            {
                mainWindow.AddFootNote("Error of DNS flush", EMessagingLevel.Error);
            }
            UpdateUi();
        }
    }
}
