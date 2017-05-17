using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bhhc.Com.Apps.Tools.HostSwitch.Entities
{
	[Serializable]
	public class Env
	{
		[XmlElement]
		public string Title { get; set; }

		[XmlElement]
		public string EnvHost { get; set; }

		[XmlElement]
		public string EnvHostPath { get; set; }
	}
}
