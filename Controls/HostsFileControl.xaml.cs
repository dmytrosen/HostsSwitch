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
		public EnvSet EnvSet { get; set; }

		public HostsFileControl()
		{
			InitializeComponent();
			EnvsDropDown.SelectionChanged += (s, a) =>
			{
				HostsFileControl ctrl = ((HostsFileControl)((Grid)((ComboBox)s).Parent).Parent);
				AvailableHostFile aHost = ((ComboBoxItem)((ComboBox)s).SelectedItem).Content as AvailableHostFile;
				MainWindow mainWindow = (MainWindow)App.Current.MainWindow;

				ctrl.CurrentEnvContent.Text = File.ReadAllText(aHost.FullPath);
				aHost.Parent.SelectedHostFile = aHost;
				mainWindow.AddFootNote(aHost.Name + " config has been selected on " + aHost.Parent.Title, EMessagingLevel.Verbose);
				mainWindow.UpdateUi(ctrl);
			};
			BtnSwitch.Click += (s, a) =>
			{
				HostsFileControl ctrl = (HostsFileControl)((Grid)((Border)((Grid)((Button)s).Parent).Parent).Parent).Parent;
				MainWindow mainWindow = (MainWindow)App.Current.MainWindow;

				// copy selected file to destination
				File.Delete(ctrl.EnvSet.EnvHostPath + "\\hosts");
				File.Copy(ctrl.EnvSet.SelectedHostFile.FullPath, ctrl.EnvSet.EnvHostPath + "\\hosts");

				// refresh DNS on local (server)
				string result = ExecuteCommand(ctrl.EnvSet.EnvHost);

				if (!string.IsNullOrWhiteSpace(result))
				{
					mainWindow.AddFootNote(ctrl.EnvSet.SelectedHostFile.FullPath + " hostfiles set", EMessagingLevel.Result);
					if (!string.IsNullOrWhiteSpace(result))
					{
						mainWindow.AddFootNote(result.Trim(), EMessagingLevel.Result);
					}
				}
				else // show error on UI
				{
					mainWindow.AddFootNote("Error of DNS flush", EMessagingLevel.Error);
				}
				ctrl.EnvSet.CurrentHostFile = ctrl.EnvSet.SelectedHostFile;
				ctrl.CurrentEnvTitle.Text = ctrl.EnvSet.CurrentHostFile.Name;
                ctrl.CurrentEnvContent.Foreground = new SolidColorBrush(Colors.Black);
                mainWindow.UpdateUi(ctrl);
			};
			BtnReset.Click += (s, a) =>
			{
				HostsFileControl ctrl = (HostsFileControl)((Grid)((Border)((Grid)((Button)s).Parent).Parent).Parent).Parent;

				ctrl.EnvSet.SelectedHostFile = ctrl.EnvSet.CurrentHostFile;
				ctrl.EnvsDropDown.SelectedIndex = ctrl.EnvSet.CurrentHostFile.Index;
				ctrl.CurrentEnvContent.Text = File.ReadAllText(ctrl.EnvSet.SelectedHostFile.FullPath);
				((MainWindow)App.Current.MainWindow).UpdateUi(ctrl);
			};
        }

		public void UpdateUi(bool enableBtnSwitch, bool enableBtnReset)
		{
			BtnSwitch.IsEnabled = enableBtnSwitch;
			BtnReset.IsEnabled = enableBtnReset;
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
	}
}
