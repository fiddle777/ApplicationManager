using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationManager.Core.Enums
{
    public enum ApplicationStatus
    {
        Draft,
        Applied,
        PendingReply,
        InterviewScheduled,
        Interviewed,
        Offer,
        Rejected,
        Ghosted
    }
}
