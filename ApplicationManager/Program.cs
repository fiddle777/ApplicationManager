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
            var repo = new InMemoryAllocationRepo();
            var manager = new ApplicationService(repo);
            SeedDummyData(manager);
            RunMenu(manager);
        }
        private static void RunMenu(ApplicationService manager)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== APLIKACIJU VALDIKLIS ===");
                Console.WriteLine("1. Rodyti visas aplikacijas");
                Console.WriteLine("2. Rodyti \"stale\" aplikacijas");
                Console.WriteLine("3. Pridėti naują aplikaciją");
                Console.WriteLine("0. Išeiti");
                Console.Write("Pasirinkimas: ");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        ShowAllApplications(manager);
                        break;
                    case "2":
                        ShowStaleApplications(manager);
                        break;
                    case "3":
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
            Console.Clear();
            Console.WriteLine("=== VISOS APLIKACIJOS ===");
            var applications = manager.GetAllSorted();
            if (!applications.Any())
            {
                Console.WriteLine("Nėra aplikacijų.");
                WaitForKey();
                return;
            }
            int i = 1;
            foreach (var app in applications)
            {
                Console.Write(i + ". ");
                Console.WriteLine(app.ToString("G", null));
                var alert = manager.GetAlert(app);
                if(!string.IsNullOrWhiteSpace(alert))
                {
                    Console.WriteLine("DĖMESIO!!! " + alert);
                }
                var lastEvents = manager.GetLastEvents(app, count: 3);
                foreach(var ev in lastEvents)
                {
                    Console.WriteLine($"    [{ev.Timestamp:g}]: {ev.Description}");
                }
                Console.WriteLine();
                i++;
            }

            Console.WriteLine("Pirmas objektas (DescribeObject demo):");
            var first = applications.FirstOrDefault();
            Console.WriteLine(manager.DescribeObject(first));
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("pasirinkite aplikaciją (0 - atgal)");
                var appList = applications?.ToList() ?? new List<Application>();
                var choice = Console.ReadLine();
                while (true)
                {
                    if (int.TryParse(choice, out int index) &&
                        index >= 1 && index <= appList.Count)
                    {
                        var selectedApp = appList[index - 1];
                        Console.Clear();
                        Console.WriteLine("=== APLIKACIJOS DETALĖS ===");
                        Console.WriteLine(selectedApp.ToString("D", null));
                        Console.WriteLine("Įvykiai:");
                        foreach (var ev in selectedApp.Events.OrderBy(e => e.Timestamp))
                        {
                            Console.WriteLine($" - [{ev.Timestamp:g}]: {ev.Description}");
                        }
                        WaitForKey();
                        return;
                    }
                    else if (choice == "0")
                    {
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Neteisingas pasirinkimas. Bandykite dar kartą.");
                        choice = Console.ReadLine();
                    }
                }
            }
        }
        private static void ShowStaleApplications(ApplicationService manager)
        {
            Console.Clear();
            Console.WriteLine("=== STALE APLIKACIJOS ===");
            var stale = manager.GetStaleApplications().ToArray();

            if (!stale.Any())
            {
                Console.WriteLine("Nėra stale aplikacijų.");
                WaitForKey();
                return;
            }
            foreach(var app in stale)
            {
                Console.WriteLine(app.ToString("G", null));
                var alert = manager.GetAlert(app);
                if (!string.IsNullOrWhiteSpace(alert))
                {
                    Console.WriteLine("  !!! " + alert);
                }
            }
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
            var WageInput = Console.ReadLine();
            manager.TryParseMonthlyWage(WageInput, out int? wage);
            var apps = manager.GetAllSorted();
            var nextId = apps.Any() ? apps.Max(a => a.Id) + 1 : 1;

            var app = new Application
            {
                Id = nextId,
                CompanyName = company,
                PositionName = position,
                Status = ApplicationStatus.Draft,
                createdDate = DateTime.UtcNow,
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
        }
        private static void WaitForKey()
        {
            Console.WriteLine();
            Console.WriteLine("Paspauskite bet kurį klavišą tęsti...");
            Console.ReadKey();
        }



        private static void SeedDummyData(ApplicationService manager)
        {
            var app1 = new Application
            {
                Id = 1,
                CompanyName = "ACME Corp",
                PositionName = "Support Engineer",
                Status = ApplicationStatus.PendingReply,
                createdDate = DateTime.UtcNow.AddDays(-10),
                LastContactDate = DateTime.UtcNow.AddDays(-7),
            };
            app1.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-10),
                Description = "Sukurta aplikacija."
            });

            var app2 = new Application
            {
                Id = 2,
                CompanyName = "MegaSoft",
                PositionName = "Intern Developer",
                Status = ApplicationStatus.InterviewScheduled,
                createdDate = DateTime.UtcNow.AddDays(-5),
                LastContactDate = DateTime.UtcNow.AddDays(-3),
                InterviewDate = DateTime.UtcNow.AddDays(2),
            };
            app2.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-5),
                Description = "Sukurta aplikacija."
            });
            app2.Events.Add(new ApplicationEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-3),
                Description = "Suplanuotas pokalbis."
            });

            manager.AddApplication(app1, app2); // params + named method
        }
    }
}