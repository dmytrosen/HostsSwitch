using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bhhc.Com.Apps.Tools.HostSwitch.Entities
{
	public class EnvSet : Env
	{
		public bool IsValid { get; set; }

		public string ConfigPath { get; set; }

        public List<string> Messages { get; set; } = new List<string>();

        public string AllMessages { get { return string.Join("\n", Messages); } }

        public string OriginalHostFileContent { get; set; }

        public AvailableHostFile CurrentHostFile { get; set; }

		public AvailableHostFile SelectedHostFile { get; set; }

		public List<AvailableHostFile> AvailableHostFiles { get; set; }
	}
}
