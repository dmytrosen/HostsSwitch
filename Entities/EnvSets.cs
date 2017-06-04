using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostSwitch.Entities
{
    public class EnvSets : ObservableCollection<EnvSet>
    {
        public override string ToString()
        {
            return $"{(Items != null ? string.Join(", ", Items) : string.Empty)}";
        }
    }
}
