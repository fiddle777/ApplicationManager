using System;
using ApplicationManager.Core;
using ApplicationManager.Core.Enums;
using ApplicationManager.Core.Models;
using ApplicationManager.Core.Repos;
using ApplicationManager.Core.Services;
using ApplicationManager.Core.Exceptions;
using ApplicationManager.Core.Extensions;
namespace ApplicationManager.App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var dataPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName, "applications.json");
                var repo = new JsonFileApplicationRepo(dataPath);
                var manager = new ApplicationService(repo);

                // REIKALAVIMAS: Naudojate įvykius savo projekte (1 t.)
                manager.ApplicationChanged += (sender, e) =>
                {
                    Console.WriteLine($"[EVENT] {e.Action}: id={e.Application.Id}");
                };

                //SeedDummyData(manager);
                RunHelloMenu(manager);
            }
            catch (ApplicationRepositoryException ex)
            {
                // REIKALAVIMAS: Yra blokai 'try' 'catch' vietose, kur gali įvykti klaida (1 t.)
                Console.WriteLine("Nepavyko paleisti programos (duomenų klaida). Detalės:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException is not null)
                {
                    Console.WriteLine("Vidinė klaida: " + ex.InnerException.Message);
                }
                Console.WriteLine();
                Console.WriteLine("Paspauskite bet kurį klavišą išeiti...");
                Console.ReadKey(true);
            }
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
                    Console.WriteLine($"- [{app.Id}] {app.CompanyName} / {app.PositionName} >> {alert}");
                }
            }
            if (!anyAlerts)
            {
                Console.WriteLine("Šiuo metu nėra jokių įspėjimų.");
            }
            // REIKALAVIMAS: Sukurtas savas bendrasis (generic) plėtimo metodas (1 t.)
            // Panaudojame WithFlag<T> (extension) parodyti, kad generic extension veikia.
            var urgentCount = apps.WithFlag(a => (a.Flags & ApplicationFlags.Urgent) == ApplicationFlags.Urgent).Count();
            Console.WriteLine();
            Console.WriteLine($"Skubios (Urgent) aplikacijos: {urgentCount}");

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

                // REIKALAVIMAS: Teisingai atlikote implementaciją IEnumerable<T> / IEnumerator<T>
                // Naudojame savo ApplicationCollection su custom enumeratorium.
                var collection = manager.GetAllAsCollection();
                var apps = collection.YieldAll().ToList();

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

                    var lastEvents = manager.GetLastEvents(app, count: 2);
                    foreach (var ev in lastEvents)
                    {
                        // REIKALAVIMAS: Sukurtas praplėtimo dekonstruktorius (1 t.)
                        // Deconstruct extension iš ApplicationEventExtensions.
                        var (time, type, desc) = ev;
                        Console.WriteLine($"     Paskutinis įvykis: [{time:g}] ({type}): {desc}");
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

            // REIKALAVIMAS: Teisingai įgyvendintas ICloneable (1 t.)
            // Susikuriame atsarginę kopiją prieš redagavimą.
            var backup = (Core.Models.Application)app.Clone();

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
            var i = 0;
            foreach (var status in Enum.GetValues<ApplicationStatus>())
            {
                Console.WriteLine($" {i}. {status}");
                i++;
            }
            Console.Write("Naujas statusas (palikite tuščią jei nekeičiate): ");
            var statusInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(statusInput) &&
                Enum.TryParse<ApplicationStatus>(statusInput, ignoreCase: true, out var newStatus))
            {
                app.Status = newStatus;
            }

            try
            {
                manager.UpdateApplication(app);
                Console.WriteLine("Aplikacija sėkmingai atnaujinta.");
            }
            catch (Exception)
            {
                app.CompanyName = backup.CompanyName;
                app.PositionName = backup.PositionName;
                app.ContactInfo = backup.ContactInfo;
                app.MonthlyWage = backup.MonthlyWage;
                app.Status = backup.Status;
                app.LastContactDate = backup.LastContactDate;
                app.DeadlineDate = backup.DeadlineDate;
                app.InterviewDate = backup.InterviewDate;
                app.Flags = backup.Flags;
                app.Events = backup.Events;

                Console.WriteLine("Įvyko klaida atnaujinant. Pakeitimai atšaukti.");
            }

            WaitForKey();
        }
        private static void AddEventToApplication(ApplicationService manager, Core.Models.Application app)
        {
            Console.Clear();
            Console.WriteLine("=== NAUJAS ĮVYKIS ===");
            Console.WriteLine("Galimi įvykių tipai:");
            var i = 0;
            foreach (var type in Enum.GetValues<ApplicationEventType>())
            {
                Console.WriteLine($"{i}. {type}");
                i++;
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
                Status = ApplicationStatus.Juodraštis,
                CreatedDate = DateTime.UtcNow,
                MonthlyWage = wage
            };
            app.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow,
                EventType = ApplicationEventType.Sukurta,
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
                Status = ApplicationStatus.LaukiamaAtsakymo,
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                LastContactDate = DateTime.UtcNow.AddDays(-7),
            };
            app1.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-10),
                EventType = ApplicationEventType.Sukurta,
                Description = "Sukurta aplikacija."
            });

            var app2 = new Core.Models.Application
            {
                Id = 2,
                CompanyName = "MegaSoft",
                PositionName = "Intern Developer",
                Status = ApplicationStatus.PokalbisSuplanuotas,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                LastContactDate = DateTime.UtcNow.AddDays(-3),
                InterviewDate = DateTime.UtcNow.AddDays(2),
            };
            app2.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-5),
                EventType = ApplicationEventType.Sukurta,
                Description = "Sukurta aplikacija."
            });
            app2.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-3),
                EventType = ApplicationEventType.Kita,
                Description = "Suplanuotas pokalbis."
            });

            manager.AddApplication(app1, app2);
        }
    }
}