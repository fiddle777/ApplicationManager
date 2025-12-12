using System;
using ApplicationManager.Core.Models;
using ApplicationManager.Core.Enums;

namespace ApplicationManager.Core.Extensions
{
    public static class ApplicationEventDeconstructExtensions
    {
        // REIKALAVIMAS: Sukurtas praplėtimo dekonstruktorius (1 t.)
        public static void Deconstruct(this ApplicationEvent ev,
            out DateTime timestamp,
            out ApplicationEventType eventType,
            out string description)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));

            timestamp = ev.Timestamp;
            eventType = ev.EventType;
            description = ev.Description;
        }
    }
}
