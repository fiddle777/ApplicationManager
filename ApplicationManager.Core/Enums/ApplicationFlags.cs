using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ApplicationManager.Core.Enums
{
    public enum ApplicationFlags
    {
        None = 0,
        Remote = 1 << 0, // 1
        RequiresRelocation = 1 << 1, // 2
        Urgent = 1 << 2  // 4
    }
}
