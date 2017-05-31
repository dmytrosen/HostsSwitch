using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostSwitch.Entities
{
    /// <summary>
    /// Host file object
    /// </summary>
	public class AvailableHostFile
	{
        /// <summary>
        /// Index in DDL list, firts one must start with 1 in alternative file name, following by ".". Examples:
        /// <code>
        /// 0.Local_only_no_mapping
        /// 1.Dev_env
        /// 2.Test_env
        /// </code>
        /// where number in each file name is the index. 
        /// </summary>
		public int Index { get; set; }

        /// <summary>
        /// Name of alternative "hosts" file
        /// </summary>
		public string Name { get; set; }

        /// <summary>
        /// Full path to Name of alternative "hosts" file
        /// </summary>
		public string FullPath { get; set; }

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
