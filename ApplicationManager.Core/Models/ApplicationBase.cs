using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationManager.Core.Models
{
    public abstract class ApplicationBase
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
    }
}
