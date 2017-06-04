using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HostSwitch.Entities
{
    /// <summary>
    /// Environment set class, extension of Environment setup class.
    /// Hosts all runtime properties of an environment; local or remote computer
    /// </summary>
	public class EnvSet : Env,  INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string configPath = null;
        public string originalHostFileContent = null;
        public string currentEnvContent = null;
        public AvailableHostFile currentHostFile = null;
        public AvailableHostFile selectedHostFile = null;
        public ObservableCollection<AvailableHostFile> availableHostFiles = null;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Flag indicates if environment is valid and able to change host files
        /// </summary>
		public bool IsValid { get; set; }

        /// <summary>
        /// Fully qualified path of configuration folder of local or remote computer in repository folder, 
        /// where env.config and alternative "hosts" files are located.
        /// </summary>
        public string ConfigPath
        {
            get
            {
                return configPath;
            }
            set
            {
                if (configPath != value)
                {
                    configPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Content of "hosts" file of the local or remote computer environment before environment load or at last commitment of environment change. 
        /// It is used to identify if uncommitted change is made. 
        /// </summary>
        public string OriginalHostFileContent
        {
            get
            {
                return originalHostFileContent;
            }
            set
            {
                if (originalHostFileContent != value)
                {
                    originalHostFileContent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Content of current environment
        /// </summary>
        public string CurrentEnvContent
        {
            get
            {
                return currentEnvContent;
            }
            set
            {
                if (currentEnvContent != value)
                {
                    currentEnvContent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Runtime object of current "host" file
        /// </summary>
        public AvailableHostFile CurrentHostFile
        {
            get
            {
                return currentHostFile;
            }
            set
            {
                if (currentHostFile != value)
                {
                    currentHostFile = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CurrentEnvironmentName");
                }
            }
        }

        /// <summary>
        /// Name of currently selected environment
        /// </summary>
        public string CurrentEnvironmentName { get { return CurrentHostFile?.Name ?? "UNAVAILABLE"; } }

        /// <summary>
        /// Runtime object of selected "host" file. May not be Current, and subject to rollback. 
        /// </summary>
        public AvailableHostFile SelectedHostFile
        {
            get
            {
                return selectedHostFile;
            }
            set
            {
                if (selectedHostFile != value)
                {
                    selectedHostFile = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// List of all available "hosts" file of local or remote computer environment
        /// </summary>
        public ObservableCollection<AvailableHostFile> AvailableHostFiles
        {
            get
            {
                return availableHostFiles;
            }
            set
            {
                if (availableHostFiles != value)
                {
                    availableHostFiles = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Messages of the local or remote computer environment, generated at runtime; normally - environment load time
        /// </summary>
        public List<string> Messages { get; set; } = new List<string>();

        /// <summary>
        /// Messages of the local or remote computer environment converted into single string
        /// </summary>
        public string AllMessages { get { return string.Join("\n", Messages); } }

        public (bool btnSwitchState, bool btnResetState) UpdateUi()
        {
			bool enableBtnSwitch = false;
			bool enableBtnReset = false;

			if (SelectedHostFile == null) // first load, no match; no change is made
			{
				enableBtnSwitch = false;
				enableBtnReset = false;
			}
			else if (CurrentHostFile == null) // first load, no match;; change is made
			{
				enableBtnSwitch = true;
				enableBtnReset = false;
			}
			else if (CurrentHostFile.Index == SelectedHostFile.Index) // selection same as current
			{
				enableBtnSwitch = false;
				enableBtnReset = false;
			}
			else // selection is dif from current
			{
				enableBtnSwitch = true;
				enableBtnReset = true;
			}
            return (enableBtnSwitch, enableBtnReset);
        }
    }
}

