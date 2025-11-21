using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationManager.Core.Models
{
    // REIKALAVIMAS: Naudojate abstrakčią klasę (0.5 t.)
    // ApplicationBase yra abstrakti bazinė klasė su bendrais laukais (Id,
    // CompanyName, PositionName), iš kurios paveldi Application.
    public abstract class ApplicationBase
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
    }
}
