using ApplicationManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationManager.Core.Enums;

namespace ApplicationManager.Core.Models
{
    public class Application : ApplicationBase,
        IComparable<Application>,
        IEquatable<Application>,
        IFormattable,
        ICloneable
    {
        public string? ContactInfo { get; set; }
        public int? MonthlyWage { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Juodraštis;
        public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
        public DateTime? LastContactDate { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public DateTime? InterviewDate { get; set; }
        public List<ApplicationEvent> Events { get; set; } = new();
        public ApplicationFlags Flags { get; set; } = ApplicationFlags.None;

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
            if (other is null) return 1;

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
                       $"Statusas: {Status}\n" +
                       $"Kontaktinė informacija: {(string.IsNullOrEmpty(ContactInfo) ? "Nenurodyta" : ContactInfo)}\n" +
                       $"Mėnesinis atlyginimas: {(MonthlyWage.HasValue ? $"{MonthlyWage}" : "NĖRA")}\n" +
                       $"Terminas: {DeadlineDate?.ToShortDateString() ?? "NĖRA"}\n" +
                       $"Paskutinis kontaktas: {LastContactDate?.ToShortDateString() ?? "NĖRA"}\n" +
                       $"Planuojamas pokalbis: {InterviewDate?.ToShortDateString() ?? "NĖRA"}\n",
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

        // REIKALAVIMAS: Teisingai įgyvendintas ICloneable (1 t.)
        // Atliekama "deep copy" – kopijuojami ir Events elementai.
        public object Clone()
        {
            var clone = (Application)MemberwiseClone();
            clone.Events = Events.Select(e => (ApplicationEvent)e.Clone()).ToList();
            return clone;
        }
    }
}
