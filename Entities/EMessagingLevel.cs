using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostSwitch.Entities
{
    /// <summary>
    /// Log level 
    /// </summary>
	public enum EMessagingLevel
	{
        /// <summary>
        /// Log all messages
        /// </summary>
		Verbose = 0, 
        /// <summary>
        /// Log only environment's "hosts" file change or settings update, as well as log all errors
        /// </summary>
		Result, 
        /// <summary>
        /// Log only errors
        /// </summary>
		Error
	}
}
