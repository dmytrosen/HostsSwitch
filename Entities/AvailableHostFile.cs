using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bhhc.Com.Apps.Tools.HostSwitch.Entities
{
	public class AvailableHostFile
	{
		public int Index { get; set; }

		public string Name { get; set; }

		public string FullPath { get; set; }

		public EnvSet Parent { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}
