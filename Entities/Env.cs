using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HostSwitch.Entities
{
    /// <summary>
    /// Environment setup class represent object configured in env.config of computer configuration folder in repository folder.
    /// For example: 
    /// <code>
    /// <Env>
	///     <Title>SRV-IIS-and-DB local VM</Title>
	///     <EnvHost>192.168.253.134</EnvHost>
	///     <EnvHostPath>\\192.168.253.134\etc</EnvHostPath>
    /// </Env>
    /// </code>
    /// or 
    /// <code>
    /// <Env>
    ///     <Title>localhost</Title>
    ///     <EnvHost>localhost</EnvHost>
    ///     <EnvHostPath>c:\Windows\System32\drivers\etc</EnvHostPath>
    /// </Env>
    /// </code>
    /// </summary>
	[Serializable]
	public class Env
	{
        /// <summary>
        /// Title shows as tab text
        /// </summary>
        [XmlElement]
		public string Title { get; set; }

        /// <summary>
        /// EnvHost identifies computer name of IP address
        /// </summary>
		[XmlElement]
		public string EnvHost { get; set; }

        /// <summary>
        /// EnvHostPath specifies either local folder in case if it is local computer, or UNC of shared folder or remote computer
        /// </summary>
		[XmlElement]
		public string EnvHostPath { get; set; }
	}
}
