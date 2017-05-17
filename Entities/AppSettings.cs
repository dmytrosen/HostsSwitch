using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostSwitch.Entities
{
	public class AppSettings : ICloneable
	{
	    protected bool Equals(AppSettings other)
	    {
	        return MessagingLevel == other.MessagingLevel 
                && ClearLogsOnRefresh == other.ClearLogsOnRefresh;
	    }

	    public override bool Equals(object obj)
	    {
	        return !ReferenceEquals(null, obj)
                && ReferenceEquals(this, obj)
                && obj.GetType() == GetType()
                && Equals((AppSettings)obj);
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            return ((int)MessagingLevel * 397) ^ ClearLogsOnRefresh.GetHashCode();
	        }
	    }

	    public EMessagingLevel MessagingLevel { get; set; }

        public bool ClearLogsOnRefresh { get; set; }

        public static bool operator ==(AppSettings set1, AppSettings set2)
		{
			return set1?.IsEqual(set2) ?? false;
		}

		public static bool operator !=(AppSettings set1, AppSettings set2)
		{
			return !set1?.IsEqual(set2) ?? false;
		}

		private bool IsEqual(object set2)
		{
			return set2 != null
				&& set2 is AppSettings
				&& MessagingLevel == ((AppSettings)set2).MessagingLevel
                && ClearLogsOnRefresh == ((AppSettings)set2).ClearLogsOnRefresh;
		}

		public object Clone()
		{
			return new AppSettings
			{
				MessagingLevel = MessagingLevel,
                ClearLogsOnRefresh = ClearLogsOnRefresh
            };
		}
	}
}
