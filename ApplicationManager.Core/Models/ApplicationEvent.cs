using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationManager.Core.Enums;

namespace ApplicationManager.Core.Models
{
    public class ApplicationEvent : ICloneable
    {
        public DateTime Timestamp { get; set; }
        public ApplicationEventType EventType { get; set; }
        public string Description { get; set; } = string.Empty;

        // REIKALAVIMAS: Teisingai įgyvendintas ICloneable (1 t.)
        public object Clone()
        {
            return new ApplicationEvent
            {
                Timestamp = Timestamp,
                EventType = EventType,
                Description = Description
            };
        }
    }
}
        