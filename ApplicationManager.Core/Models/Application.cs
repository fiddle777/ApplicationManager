using ApplicationManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationManager.Core.Models
{
    public class Application : ApplicationBase,
        IComparable<Application>,
        IEquatable<Application>,
        IFormattable
    {
        public string? ContactInfo { get; set; }
        public int? MonthlyWage { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;
        public DateTime createdDate { get; init; } = DateTime.UtcNow;
        public DateTime? LastContactDate { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public DateTime? InterviewDate { get; set; }
        public List<ApplicationEvent> Events { get; } = new();

        public bool Equals(Application? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }
        public override bool Equals(object? obj) => Equals(obj as Application);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(Application? left, Application? right) => Equals(left, right);
        public static bool operator !=(Application? left, Application? right) => !Equals(left, right);

        public int CompareTo(Application? other)
        {
            int statusCompare = Status.CompareTo(other?.Status);
            if (statusCompare != 0) return statusCompare;

            int deadlineCompare = Nullable.Compare(DeadlineDate, other?.DeadlineDate);
            if (deadlineCompare != 0) return deadlineCompare;

            return string.Compare(CompanyName, other.CompanyName, StringComparison.CurrentCultureIgnoreCase);
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            format ??= "G";
            return format switch
            {
                "G" => $"{CompanyName} - {PositionName} ({Status})",

                "D" => $"[{Id}] {CompanyName} - {PositionName}\n" +
                       $"Status: {Status}\n" +
                       $"Deadline: {DeadlineDate?.ToShortDateString() ?? "N/A"}\n",
                _ => ToString()
            };
        }
        public override string ToString() => ToString("G", null);
        public void Deconstruct(out string companyName, out string positionName, out ApplicationStatus status)
        {
            companyName = CompanyName;
            positionName = PositionName;
            status = Status;
        }
    }
}
