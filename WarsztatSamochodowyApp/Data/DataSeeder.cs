using Bogus;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Data;

public static class DataSeeder
{
    // WAŻNE: Zmień 'YourAppDbContext' na rzeczywistą nazwę Twojego kontekstu bazy danych!
    public static void Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

        if (context == null)
        {
            Console.WriteLine("Nie można uzyskać kontekstu bazy danych.");
            return;
        }

        // Upewnij się, że baza danych istnieje
        context.Database.EnsureCreated();

        // Sprawdzamy, czy dane już istnieją, aby uniknąć duplikatów przy każdym uruchomieniu
        if (context.Clients.Any())
        {
            Console.WriteLine("Baza danych została już wypełniona danymi.");
            return;
        }

        Console.WriteLine("Rozpoczynanie seedowania bazy danych...");

        // Ustawienie polskiej lokalizacji dla generowanych danych
        Randomizer.Seed = new Random(8675309);
        var faker = new Faker("pl");

        // === 1. Typy części (dane statyczne) ===
        var partTypes = new List<PartType>
        {
            new() { Name = "Części silnikowe" },
            new() { Name = "Układ hamulcowy" },
            new() { Name = "Filtry" },
            new() { Name = "Opony i felgi" },
            new() { Name = "Elementy karoserii" }
        };
        context.PartTypes.AddRange(partTypes);
        context.SaveChanges(); // Zapisujemy, aby uzyskać ID dla kolejnych kroków

        // === 2. Klienci ===
        var clientFaker = new Faker<Client>("pl")
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber("###-###-###"));

        var clients = clientFaker.Generate(5);
        context.Clients.AddRange(clients);
        context.SaveChanges();

        // === 3. Części ===
        var partFaker = new Faker<Part>("pl")
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Finance.Amount(20, 800))
            .RuleFor(p => p.PartTypeId, f => f.PickRandom(partTypes).Id);

        var parts = partFaker.Generate(15); // Generujemy więcej części, by było z czego wybierać
        context.Parts.AddRange(parts);
        context.SaveChanges();

        // === 4. Pojazdy ===
        var imageUrls = new[]
        {
            "5d73a31c-74d2-43a6-babb-59731c9048c2.jpg",
            "37aeebd6-5f3d-40e1-b3a7-d556ea1a1d00.jpg",
            "708edb74-4d39-4647-94be-99b3553fb82f.jpg",
            "33656570-7ba3-41a9-99f2-fe6dd8875d48.gif"
        };

        var vehicleFaker = new Faker<Vehicle>("pl")
            .RuleFor(v => v.Vin, f => f.Random.String2(17, "ABCDEFGHJKLMNPRSTUVWXYZ0123456789"))
            .RuleFor(v => v.RegistrationNumber, f => f.Random.Replace("?? #####").ToUpper())
            .RuleFor(v => v.ClientId, f => f.PickRandom(clients).Id)
            .RuleFor(v => v.ImageUrl,
                f => "/images/" + f.PickRandom(imageUrls)); // Zakładam, że pliki są w wwwroot/images/vehicles/

        var vehicles = vehicleFaker.Generate(5);
        context.Vehicles.AddRange(vehicles);
        context.SaveChanges();

        // === 5. Usługi (zadania serwisowe) ===
        var serviceTaskFaker = new Faker<ServiceTask>("pl")
            .RuleFor(st => st.Name,
                f => f.PickRandom("Wymiana oleju i filtra", "Przegląd okresowy", "Naprawa układu hamulcowego",
                    "Wymiana opon", "Diagnostyka komputerowa"))
            .RuleFor(st => st.Description, f => f.Lorem.Sentence(10))
            .RuleFor(st => st.Price,
                f => Math.Round(f.Finance.Amount(100, 1500) / 50) * 50); // Cena zaokrąglona do 50 PLN

        var serviceTasks = serviceTaskFaker.Generate(5);
        context.ServiceTasks.AddRange(serviceTasks);
        context.SaveChanges();

        // === 6. Zlecenia serwisowe ===
        var serviceOrderFaker = new Faker<ServiceOrder>("pl")
            .RuleFor(so => so.OrderDate, f => f.Date.Past(2))
            .RuleFor(so => so.Status, f => f.PickRandom<ServiceOrderStatus>())
            .RuleFor(so => so.VehicleId, f => f.PickRandom(vehicles).Id)
            .RuleFor(so => so.FinishedDate,
                (f, so) => so.Status == ServiceOrderStatus.Zakonczone || so.Status == ServiceOrderStatus.Anulowane
                    ? f.Date.Between(so.OrderDate, DateTime.Now)
                    : null);

        var serviceOrders = serviceOrderFaker.Generate(5);

        // Powiązanie zleceń z zadaniami (relacja wiele-do-wielu)
        foreach (var order in serviceOrders)
        {
            var tasksForOrder = faker.PickRandom(serviceTasks, faker.Random.Int(1, 2)).ToList();
            order.ServiceTasks = tasksForOrder;
        }

        context.ServiceOrders.AddRange(serviceOrders);
        context.SaveChanges();

        // === 7. Użyte części (relacja do zadań) ===
        var usedPartsList = new List<UsedPart>();
        // Bierzemy tylko zadania, które zostały faktycznie przypisane do jakiegoś zlecenia
        var tasksInUse = serviceOrders.SelectMany(so => so.ServiceTasks).Distinct();

        foreach (var task in tasksInUse)
            if (faker.Random.Bool(0.8f)) // 80% szans, że do zadania zostaną użyte jakieś części
            {
                var partsForTask = faker.PickRandom(parts, faker.Random.Int(1, 3));
                foreach (var part in partsForTask)
                    usedPartsList.Add(new UsedPart
                    {
                        ServiceTaskId = task.Id,
                        PartId = part.Id,
                        Quantity = faker.Random.Int(1, 2)
                    });
            }

        context.UsedParts.AddRange(usedPartsList);
        context.SaveChanges();


        Console.WriteLine("Seedowanie bazy danych zakończone pomyślnie.");
    }
}