using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationManager.Core.Enums;

namespace ApplicationManager.Core.Models
{
    public class ApplicationEvent
    {
        public DateTime Timestamp { get; set; }
        public ApplicationEventType EventType { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
        