using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostSwitch.Entities
{
    /// <summary>
    /// Environment set class, extension of Environment setup class.
    /// Hosts all runtime properties of an environment; local or remote computer
    /// </summary>
	public class EnvSet : Env
	{
        /// <summary>
        /// Flag indicates if environment is valid and able to change host files
        /// </summary>
		public bool IsValid { get; set; }

        /// <summary>
        /// Fully qualified path of configuration folder of local or remote computer in repository folder, 
        /// where env.config and alternative "hosts" files are located.
        /// </summary>
		public string ConfigPath { get; set; }

        /// <summary>
        /// Messages of the local or remote computer environment, generated at runtime; normally - environment load time
        /// </summary>
        public List<string> Messages { get; set; } = new List<string>();

        /// <summary>
        /// Messages of the local or remote computer environment converted into single string
        /// </summary>
        public string AllMessages { get { return string.Join("\n", Messages); } }

        /// <summary>
        /// Content of "hosts" file of the local or remote computer environment before environment load or at last commitment of environment change. 
        /// It is used to identify if uncommitted change is made. 
        /// </summary>
        public string OriginalHostFileContent { get; set; }

        /// <summary>
        /// Runtime object of current "host" file
        /// </summary>
        public AvailableHostFile CurrentHostFile { get; set; }

        /// <summary>
        /// Runtime object of selected "host" file. May not be Current, and subject to rollback. 
        /// </summary>
		public AvailableHostFile SelectedHostFile { get; set; }

        /// <summary>
        /// List of all available "hosts" file of local or remote computer environment
        /// </summary>
		public List<AvailableHostFile> AvailableHostFiles { get; set; }
	}
}
