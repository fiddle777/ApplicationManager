using System;
using ApplicationManager.Core;
using ApplicationManager.Core.Enums;
using ApplicationManager.Core.Models;
using ApplicationManager.Core.Repos;
using ApplicationManager.Core.Services;

namespace ApplicationManager.App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var repo = new InMemoryApplicationRepo();
            var manager = new ApplicationService(repo);
            //SeedDummyData(manager);
            RunHelloMenu(manager);
        }
    private static void RunHelloMenu(ApplicationService manager)
        {
            Console.Clear();
            Console.WriteLine("Sveiki sugrįžę!");
            Console.WriteLine();

            var apps = manager.GetAllSorted().ToArray();
            if (!apps.Any())
            {
                Console.WriteLine("Šiuo metu neturite jokių aplikacijų.");
                Console.WriteLine();
            }

            Console.WriteLine("AKTYVŪS ĮSPĖJIMAI:");
            var anyAlerts = false;
            foreach (var app in apps)
            {
                var alert = manager.GetAlert(app);
                if (!string.IsNullOrWhiteSpace(alert))
                {
                    anyAlerts = true;
                    Console.WriteLine($"- [{app.Id}] {app.CompanyName} / {app.PositionName}");
                    Console.WriteLine($"  >> {alert}");
                }
            }
            if (!anyAlerts)
            {
                Console.WriteLine("Šiuo metu nėra jokių įspėjimų.");
            }
            Console.WriteLine();
            WaitForKey();
            RunMenu(manager);
        }
        private static void RunMenu(ApplicationService manager)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== APLIKACIJŲ VALDYTOJAS ===");
                Console.WriteLine("1. Rodyti visas aplikacijas");
                Console.WriteLine("2. Pridėti naują aplikaciją");
                Console.WriteLine("0. Išeiti");
                Console.Write("Pasirinkimas >> ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        ShowAllApplications(manager);
                        break;
                    case "2":
                        AddNewApplication(manager);
                        break;
                    case "0":
                        Console.WriteLine("YOU HAVE BEEN TERMINATED.");
                        return;
                    default:
                        Console.WriteLine("Neteisingas pasirinkimas. Bandykite dar kartą.");
                        WaitForKey();
                        break;
                }
            }
        }
        private static void ShowAllApplications(ApplicationService manager)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== VISOS APLIKACIJOS ===");
                var apps = manager.GetAllSorted().ToList();
                if (!apps.Any())
                {
                    Console.WriteLine("Nėra aplikacijų.");
                    WaitForKey();
                    return;
                }
                for (int i = 0; i < apps.Count; i++)
                {
                    var app = apps[i];
                    Console.Write($"{i + 1}. ");
                    Console.WriteLine(app.ToString("G", null));

                    var alert = manager.GetAlert(app);
                    if (!string.IsNullOrWhiteSpace(alert))
                    {
                        Console.WriteLine("   DĖMESIO!!! " + alert);
                    }

                    var lastEvents = manager.GetLastEvents(app, count: 3);
                    foreach (var ev in lastEvents)
                    {
                        Console.WriteLine($"      [{ev.Timestamp:g}]: {ev.Description}");
                    }

                    Console.WriteLine();
                }
                Console.WriteLine("Pasirinkite aplikaciją, kurią norite peržiūrėti / redaguoti (0 - atgal) >> ");
                var input = Console.ReadLine();
                if (input == "0")
                    return;

                if (int.TryParse(input, out int index) &&
                    index >= 1 && index <= apps.Count)
                {
                    var selectedApp = apps[index - 1];
                    ShowApplicationDetails(manager, selectedApp);
                }
                else
                {
                    Console.WriteLine("Neteisingas pasirinkimas. Bandykite dar kartą.");
                    WaitForKey();
                }
            }
        }
        private static void ShowApplicationDetails(ApplicationService manager, Core.Models.Application app)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== APLIKACIJOS INFORMACIJA ===");
                Console.WriteLine(app.ToString("D", null));
                Console.WriteLine();
                Console.WriteLine("Įvykių žurnalas:");
                foreach (var ev in app.Events.OrderBy(e => e.Timestamp))
                {
                    Console.WriteLine($" - [{ev.Timestamp:g}] ({ev.EventType}): {ev.Description}");
                }
                Console.WriteLine();
                Console.WriteLine("1. Redaguoti aplikacijos duomenis");
                Console.WriteLine("2. Pridėti naują įvykį");
                Console.WriteLine("0. Grįžti atgal");
                Console.Write("Pasirinkimas >> ");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        EditApplication(manager, app);
                        break;
                    case "2":
                        AddEventToApplication(manager, app);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Neteisingas pasirinkimas. Bandykite dar kartą.");
                        WaitForKey();
                        break;
                }
            }
        }
        private static void EditApplication(ApplicationService manager, Core.Models.Application app)
        {
            Console.Clear();
            Console.WriteLine("=== APLIKACIJOS REDAGAVIMAS ===");
            Console.WriteLine("Palikite tuščią, jei nenorite keisti reikšmės.");
            Console.WriteLine();
            Console.WriteLine($"Dabartinė įmonė: {app.CompanyName}");
            Console.Write("Nauja įmonė: ");
            var newCompany = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newCompany))
            {
                app.CompanyName = newCompany;
            }
            Console.WriteLine($"Dabartinė pozicija: {app.PositionName}");
            Console.Write("Nauja pozicija: ");
            var newPosition = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newPosition))
            {
                app.PositionName = newPosition;
            }
            Console.WriteLine($"Dabartinis mėnesinis atlyginimas: {app.MonthlyWage?.ToString() ?? "nenurodytas"}");
            Console.Write("Naujas atlyginimas: ");
            var wageInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(wageInput))
            {
                if (manager.TryParseMonthlyWage(wageInput, out int? wage))
                {
                    app.MonthlyWage = wage;
                }
                else
                {
                    Console.WriteLine("Neteisingas atlyginimo formatas, paliekama sena reikšmė.");
                }
            }
            Console.WriteLine($"Dabartinis statusas: {app.Status}");
            Console.WriteLine("Galimi statusai:");
            foreach (var status in Enum.GetValues<ApplicationStatus>())
            {
                Console.WriteLine($" - {status}");
            }
            Console.Write("Naujas statusas (palikite tuščią jei nekeičiate): ");
            var statusInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(statusInput) &&
                Enum.TryParse<ApplicationStatus>(statusInput, ignoreCase: true, out var newStatus))
            {
                app.Status = newStatus;
            }
            manager.UpdateApplication(app);
            Console.WriteLine("Aplikacija sėkmingai atnaujinta.");
            WaitForKey();
        }
        private static void AddEventToApplication(ApplicationService manager, Core.Models.Application app)
        {
            Console.Clear();
            Console.WriteLine("=== NAUJAS ĮVYKIS ===");
            Console.WriteLine("Galimi įvykių tipai:");
            foreach (var type in Enum.GetValues<ApplicationEventType>())
            {
                Console.WriteLine($" - {type}");
            }
            Console.Write("Įvykio tipas >> ");
            var typeInput = Console.ReadLine();
            if (!Enum.TryParse<ApplicationEventType>(typeInput, ignoreCase: true, out var eventType))
            {
                Console.WriteLine("Neteisingas įvykio tipas, atšaukiama.");
                WaitForKey();
                return;
            }
            Console.Write("Įvykio aprašymas: ");
            var description = Console.ReadLine() ?? string.Empty;
            var newEvent = new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow,
                EventType = eventType,
                Description = description
            };
            app.Events.Add(newEvent);
            manager.UpdateApplication(app);
            Console.WriteLine("Įvykis sėkmingai pridėtas.");
            WaitForKey();
        }
        private static void AddNewApplication(ApplicationService manager)
        {
            Console.Clear();
            Console.WriteLine("=== NAUJA APLIKACIJA ===");
            Console.Write("Įmonė: ");
            var company = Console.ReadLine() ?? string.Empty;

            Console.Write("Pozicija: ");
            var position = Console.ReadLine() ?? string.Empty;

            Console.Write("Mėnesinis atlyginimas: ");
            var wageInput = Console.ReadLine();
            manager.TryParseMonthlyWage(wageInput, out int? wage);

            var apps = manager.GetAllSorted();
            var nextId = apps.Any() ? apps.Max(a => a.Id) + 1 : 1;

            var app = new Core.Models.Application
            {
                Id = nextId,
                CompanyName = company,
                PositionName = position,
                Status = ApplicationStatus.Draft,
                CreatedDate = DateTime.UtcNow,
                MonthlyWage = wage
            };
            app.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow,
                EventType = ApplicationEventType.Created,
                Description = "Aplikacija sukurta."
            });
            manager.AddApplication(app);

            Console.WriteLine("Aplikacija pridėta sėkmingai!");
            WaitForKey();

            ShowApplicationDetails(manager, app);
        }
        private static void WaitForKey()
        {
            Console.WriteLine();
            Console.WriteLine("Paspauskite bet kurį klavišą tęsti...");
            Console.ReadKey(true);
        }
        private static void SeedDummyData(ApplicationService manager)
        {
            var app1 = new Core.Models.Application
            {
                Id = 1,
                CompanyName = "ACME Corp",
                PositionName = "Support Engineer",
                Status = ApplicationStatus.PendingReply,
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                LastContactDate = DateTime.UtcNow.AddDays(-7),
            };
            app1.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-10),
                EventType = ApplicationEventType.Created,
                Description = "Sukurta aplikacija."
            });

            var app2 = new Core.Models.Application
            {
                Id = 2,
                CompanyName = "MegaSoft",
                PositionName = "Intern Developer",
                Status = ApplicationStatus.InterviewScheduled,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                LastContactDate = DateTime.UtcNow.AddDays(-3),
                InterviewDate = DateTime.UtcNow.AddDays(2),
            };
            app2.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-5),
                EventType = ApplicationEventType.Created,
                Description = "Sukurta aplikacija."
            });
            app2.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-3),
                EventType = ApplicationEventType.Misc,
                Description = "Suplanuotas pokalbis."
            });

            // params-based add (keeps your 'params' requirement)
            manager.AddApplication(app1, app2);
        }
    }
}