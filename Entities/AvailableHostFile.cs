using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HostSwitch.Entities
{
    /// <summary>
    /// Host file object
    /// </summary>
	public class AvailableHostFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int index = -1;
        private string fullPath = null;
        public string name = null;

        /// <summary>
        /// Index in DDL list, firts one must start with 1 in alternative file name, following by ".". Examples:
        /// <code>
        /// 0.Local_only_no_mapping
        /// 1.Dev_env
        /// 2.Test_env
        /// </code>
        /// where number in each file name is the index. 
        /// </summary>
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                if (index != value)
                {
                    index = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of alternative "hosts" file
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Full path to Name of alternative "hosts" file
        /// </summary>
        public string FullPath
        {
            get
            {
                return fullPath;
            }
            set
            {
                if (fullPath != value)
                {
                    fullPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Environment (local or remote computer), which the "host" belogs to
        /// </summary>
		public EnvSet Parent { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
