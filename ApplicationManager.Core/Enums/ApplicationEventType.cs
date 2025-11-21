using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationManager.Core.Enums
{
    public enum ApplicationEventType
    {
        Created,
        ApplicationSubmitted,
        ReplyReceived,
        InterviewScheduled,
        InterviewCompleted,
        OfferReceived,
        RejectionReceived,
        Misc
    }
}
