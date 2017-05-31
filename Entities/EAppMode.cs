using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostSwitch.Entities
{
    /// <summary>
    /// Application state; used for navigation and active form identification
    /// </summary>
	internal enum EAppMode
	{
        /// <summary>
        /// App load
        /// </summary>
		Initializing = -1, 
        /// <summary>
        /// Main screen
        /// </summary>
		Config = 0,
        /// <summary>
        /// Settings screen
        /// </summary>
		Settings,
        /// <summary>
        /// Help screen
        /// </summary>
		Help 
	}
}
