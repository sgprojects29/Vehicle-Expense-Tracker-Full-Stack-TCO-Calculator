using Microsoft.AspNetCore.Identity;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync(bool forceReset = false)
    {
        try
        {
          // Force reset if requested
          if (forceReset)
          {
              _logger.LogWarning("Force reset requested. Clearing existing data...");
              
              // Delete all data in correct order
              _context.FuelEntries.RemoveRange(_context.FuelEntries);
              _context.Expenses.RemoveRange(_context.Expenses);
              _context.Receipts.RemoveRange(_context.Receipts);
              _context.Vehicles.RemoveRange(_context.Vehicles);
              
              // Delete all users
              var existingUsers = _context.Users.ToList();
              foreach (var user in existingUsers)
              {
                  await _userManager.DeleteAsync(user);
              }
              
              await _context.SaveChangesAsync();
              _logger.LogInformation("Database cleared successfully.");
          }
          else if (_context.Users.Any())
          {
              _logger.LogInformation("Database already seeded. Skipping seed.");
              return;
          }

          _logger.LogInformation("Starting database seeding...");

          var users = await CreateTestUsersAsync();
          var vehicles = await CreateTestVehiclesAsync(users);

          await CreateTestExpensesAsync(vehicles);
          await CreateTestFuelEntriesAsync(vehicles);
          await _context.SaveChangesAsync();

          _logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task<List<User>> CreateTestUsersAsync()
    {
        _logger.LogInformation("Creating test users...");

        var users = new List<User>();
        var testPassword = "Test123"; // Simple password for demo purposes

        // User 1: Demo account
        var user1 = new User
        {
            UserName = "demo@vehicletracker.com",
            Email = "demo@vehicletracker.com",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-12)
        };
        var result1 = await _userManager.CreateAsync(user1, testPassword);
        if (result1.Succeeded)
        {
            users.Add(user1);
            _logger.LogInformation("Created user: {Email}", user1.Email);
        }

        // User 2: Test account
        var user2 = new User
        {
            UserName = "test@vehicletracker.com",
            Email = "test@vehicletracker.com",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-8)
        };
        var result2 = await _userManager.CreateAsync(user2, testPassword);
        if (result2.Succeeded)
        {
            users.Add(user2);
            _logger.LogInformation("Created user: {Email}", user2.Email);
        }

        // User 3: Admin account
        var user3 = new User
        {
            UserName = "admin@vehicletracker.com",
            Email = "admin@vehicletracker.com",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-18)
        };
        var result3 = await _userManager.CreateAsync(user3, testPassword);
        if (result3.Succeeded)
        {
            users.Add(user3);
            _logger.LogInformation("Created user: {Email}", user3.Email);
        }

        return users;
    }

    private async Task<List<Vehicle>> CreateTestVehiclesAsync(List<User> users)
    {
        _logger.LogInformation("Creating test vehicles...");

        var vehicles = new List<Vehicle>();
        var baseDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // User 1 vehicles
        if (users.Count > 0)
        {
            vehicles.Add(new Vehicle
            {
                Make = "Honda",
                Model = "Civic",
                Year = 2019,
                PurchasePrice = 18500m,
                OwnershipStart = baseDate.AddMonths(-24),
                OwnershipEnd = null, // Still owned
                VehicleType = VehicleType.Gasoline,
                UserId = users[0].Id
            });

            vehicles.Add(new Vehicle
            {
                Make = "Tesla",
                Model = "Model 3",
                Year = 2022,
                PurchasePrice = 52000m,
                OwnershipStart = baseDate.AddMonths(-18),
                OwnershipEnd = null, // Still owned
                VehicleType = VehicleType.Electric,
                UserId = users[0].Id
            });

            vehicles.Add(new Vehicle
            {
                Make = "Toyota",
                Model = "Prius Prime",
                Year = 2024,
                PurchasePrice = 38500m,
                OwnershipStart = baseDate.AddMonths(-6),
                OwnershipEnd = null, // Still owned
                VehicleType = VehicleType.PlugInHybrid,
                UserId = users[0].Id
            });
        }

        // User 2 vehicles (test user - single practical car)
        if (users.Count > 1)
        {
            vehicles.Add(new Vehicle
            {
                Make = "Mazda",
                Model = "CX-5",
                Year = 2021,
                PurchasePrice = 28000m,
                OwnershipStart = baseDate.AddMonths(-10),
                OwnershipEnd = null, // Still owned
                VehicleType = VehicleType.Gasoline,
                UserId = users[1].Id
            });

            vehicles.Add(new Vehicle
            {
                Make = "Chevrolet",
                Model = "Malibu",
                Year = 2018,
                PurchasePrice = 15000m,
                OwnershipStart = baseDate.AddMonths(-30),
                OwnershipEnd = baseDate.AddMonths(-10), // Sold
                VehicleType = VehicleType.Gasoline,
                UserId = users[1].Id
            });
        }

        // User 3 vehicles (admin user - truck and diesel)
        if (users.Count > 2)
        {
            vehicles.Add(new Vehicle
            {
                Make = "Ford",
                Model = "F-150",
                Year = 2020,
                PurchasePrice = 42000m,
                OwnershipStart = baseDate.AddMonths(-20),
                OwnershipEnd = null, // Still owned
                VehicleType = VehicleType.Gasoline,
                UserId = users[2].Id
            });

            vehicles.Add(new Vehicle
            {
                Make = "Volkswagen",
                Model = "Jetta TDI",
                Year = 2023,
                PurchasePrice = 26500m,
                OwnershipStart = baseDate.AddMonths(-8),
                OwnershipEnd = null, // Still owned
                VehicleType = VehicleType.Diesel,
                UserId = users[2].Id
            });
        }

        await _context.Vehicles.AddRangeAsync(vehicles);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created {Count} vehicles", vehicles.Count);
        return vehicles;
    }

    private async Task CreateTestExpensesAsync(List<Vehicle> vehicles)
    {
        _logger.LogInformation("Creating test expenses...");

        var expenses = new List<Expense>();
        var baseDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var random = new Random(42); // Seed for consistent results

        foreach (var vehicle in vehicles)
        {
            var ownershipMonths = (baseDate.DayNumber - vehicle.OwnershipStart.DayNumber) / 30;

            // Insurance (quarterly - every 3 months)
            var insuranceMonths = new[] { -12, -9, -6, -3 };
            foreach (var monthOffset in insuranceMonths.Where(m => m >= -ownershipMonths))
            {
                var insuranceCost = vehicle.VehicleType switch
                {
                    VehicleType.Electric => random.Next(120, 160),
                    VehicleType.PlugInHybrid => random.Next(140, 180),
                    VehicleType.Gasoline => random.Next(150, 200),
                    VehicleType.Diesel => random.Next(160, 210),
                    VehicleType.Hybrid => random.Next(135, 175),
                    _ => 150
                };

                expenses.Add(new Expense
                {
                    Category = ExpenseCategory.Insurance,
                    Amount = insuranceCost,
                    Date = baseDate.AddMonths(monthOffset),
                    Notes = "Quarterly insurance premium",
                    VehicleId = vehicle.Id
                });
            }

            // Registration (annual)
            if (ownershipMonths >= 12)
            {
                expenses.Add(new Expense
                {
                    Category = ExpenseCategory.Registration,
                    Amount = random.Next(80, 150),
                    Date = baseDate.AddMonths(-12),
                    Notes = "Annual vehicle registration",
                    VehicleId = vehicle.Id
                });
            }
            if (ownershipMonths >= 1)
            {
                expenses.Add(new Expense
                {
                    Category = ExpenseCategory.Registration,
                    Amount = random.Next(80, 150),
                    Date = baseDate.AddMonths(-1),
                    Notes = "Annual vehicle registration renewal",
                    VehicleId = vehicle.Id
                });
            }

            // Maintenance (every 4 months)
            var maintenanceCount = Math.Min(ownershipMonths / 4, 6);
            for (int i = 0; i < maintenanceCount; i++)
            {
                var monthOffset = -(i * 4 + random.Next(1, 3));
                if (monthOffset >= -ownershipMonths)
                {
                    var maintenanceTypes = new[]
                    {
                        ("Oil change", random.Next(40, 80)),
                        ("Tire rotation", random.Next(30, 60)),
                        ("Brake inspection", random.Next(50, 100)),
                        ("Air filter replacement", random.Next(25, 50)),
                        ("Cabin filter replacement", random.Next(20, 40))
                    };

                    var maintenance = maintenanceTypes[random.Next(maintenanceTypes.Length)];
                    expenses.Add(new Expense
                    {
                        Category = ExpenseCategory.Maintenance,
                        Amount = maintenance.Item2,
                        Date = baseDate.AddMonths(monthOffset),
                        Notes = maintenance.Item1,
                        VehicleId = vehicle.Id
                    });
                }
            }

            // Repairs (random, more likely on older vehicles)
            var repairChance = vehicle.Year < 2020 ? 3 : 1;
            for (int i = 0; i < repairChance && i * 5 < ownershipMonths; i++)
            {
                var monthOffset = -(random.Next(1, ownershipMonths));
                var repairTypes = new[]
                {
                    ("Battery replacement", random.Next(120, 200)),
                    ("Brake pad replacement", random.Next(200, 350)),
                    ("Windshield repair", random.Next(50, 150)),
                    ("Alternator replacement", random.Next(300, 500)),
                    ("Starter replacement", random.Next(250, 400))
                };

                var repair = repairTypes[random.Next(repairTypes.Length)];
                expenses.Add(new Expense
                {
                    Category = ExpenseCategory.Repairs,
                    Amount = repair.Item2,
                    Date = baseDate.AddMonths(monthOffset),
                    Notes = repair.Item1,
                    VehicleId = vehicle.Id
                });
            }

            // Parking (monthly for city vehicles)
            if (vehicle.Make == "Tesla" || vehicle.Make == "Honda")
            {
                for (int i = 0; i < Math.Min(6, ownershipMonths); i++)
                {
                    expenses.Add(new Expense
                    {
                        Category = ExpenseCategory.Parking,
                        Amount = random.Next(15, 45),
                        Date = baseDate.AddMonths(-i),
                        Notes = "Monthly parking permit",
                        VehicleId = vehicle.Id
                    });
                }
            }

            // Tolls (occasional)
            var tollCount = Math.Min(random.Next(3, 10), ownershipMonths);
            for (int i = 0; i < tollCount; i++)
            {
                expenses.Add(new Expense
                {
                    Category = ExpenseCategory.Tolls,
                    Amount = random.Next(5, 25),
                    Date = baseDate.AddMonths(-random.Next(0, ownershipMonths)),
                    Notes = "Highway toll",
                    VehicleId = vehicle.Id
                });
            }

            // Car Wash (occasional)
            var washCount = Math.Min(random.Next(5, 15), ownershipMonths);
            for (int i = 0; i < washCount; i++)
            {
                expenses.Add(new Expense
                {
                    Category = ExpenseCategory.CarWash,
                    Amount = random.Next(12, 30),
                    Date = baseDate.AddMonths(-random.Next(0, ownershipMonths)),
                    Notes = "Car wash and detailing",
                    VehicleId = vehicle.Id
                });
            }

            // Modifications (rare, only for some vehicles)
            if (vehicle.Make == "Honda" || vehicle.Make == "Ford")
            {
                expenses.Add(new Expense
                {
                    Category = ExpenseCategory.Modifications,
                    Amount = random.Next(200, 800),
                    Date = baseDate.AddMonths(-random.Next(2, ownershipMonths)),
                    Notes = "Aftermarket upgrades",
                    VehicleId = vehicle.Id
                });
            }

            // Other (miscellaneous)
            expenses.Add(new Expense
            {
                Category = ExpenseCategory.Other,
                Amount = random.Next(50, 150),
                Date = baseDate.AddMonths(-random.Next(0, ownershipMonths)),
                Notes = "Miscellaneous vehicle expense",
                VehicleId = vehicle.Id
            });
        }

        await _context.Expenses.AddRangeAsync(expenses);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created {Count} expenses", expenses.Count);
    }

    private async Task CreateTestFuelEntriesAsync(List<Vehicle> vehicles)
    {
        _logger.LogInformation("Creating test fuel entries and expenses...");

        var fuelEntries = new List<FuelEntry>();
        var fuelExpensesWithEntry = new List<Expense>();
        var standaloneFuelExpenses = new List<Expense>();
        var baseDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var random = new Random(42); // Seed for consistent results

        foreach (var vehicle in vehicles)
        {
            // Calculate ownership months
            var endDate = vehicle.OwnershipEnd ?? baseDate;
            var ownershipMonths = (endDate.DayNumber - vehicle.OwnershipStart.DayNumber) / 30;

            // Skip very short ownership periods
            if (ownershipMonths < 1)
                continue;

            // Create fuel entries with realistic progression (about 4 fill-ups per month, max 50 entries)
            var fillUpCount = Math.Min(ownershipMonths * 4, 50);
            
            // Calculate starting odometer based on vehicle age at purchase
            var vehicleAgeAtPurchase = vehicle.OwnershipStart.Year - vehicle.Year;
            var startingOdometer = vehicleAgeAtPurchase * 15000; // ~15,000 km/year before purchase
            var currentOdometer = startingOdometer;

            // Generate fuel entries
            for (int i = 0; i < fillUpCount; i++)
            {
                // Weekly intervals
                var weeksOwned = (int)(ownershipMonths * 4.33);
                var weekOffset = -random.Next(0, weeksOwned);
                var entryDate = baseDate.AddDays(weekOffset * 7);

                // Skip if before ownership start or after ownership end
                if (entryDate < vehicle.OwnershipStart || 
                    (vehicle.OwnershipEnd.HasValue && entryDate > vehicle.OwnershipEnd.Value))
                    continue;

                // Increment odometer (200-500 km per week)
                currentOdometer += random.Next(200, 500);

                // Create fuel entry and expense based on vehicle type
                switch (vehicle.VehicleType)
                {
                    case VehicleType.Gasoline:
                        var litersGas = random.Next(30, 55);
                        var pricePerLiterGas = (decimal)(random.Next(135, 165) / 100.0);
                        var fuelCostGas = litersGas * pricePerLiterGas;
                        var fuelNoteGas = $"Gasoline fill-up: {litersGas}L";

                        fuelEntries.Add(new FuelEntry
                        {
                            EnergyType = EnergyType.Gasoline,
                            Amount = litersGas,
                            Cost = fuelCostGas,
                            Odometer = currentOdometer,
                            Date = entryDate,
                            VehicleId = vehicle.Id
                        });

                        fuelExpensesWithEntry.Add(new Expense
                        {
                            Category = ExpenseCategory.Other,
                            Amount = fuelCostGas,
                            Date = entryDate,
                            Notes = fuelNoteGas,
                            VehicleId = vehicle.Id
                        });
                        break;

                    case VehicleType.Diesel:
                        var litersDiesel = random.Next(35, 60);
                        var pricePerLiterDiesel = (decimal)(random.Next(145, 175) / 100.0);
                        var fuelCostDiesel = litersDiesel * pricePerLiterDiesel;
                        var fuelNoteDiesel = $"Diesel fill-up: {litersDiesel}L";

                        fuelEntries.Add(new FuelEntry
                        {
                            EnergyType = EnergyType.Diesel,
                            Amount = litersDiesel,
                            Cost = fuelCostDiesel,
                            Odometer = currentOdometer,
                            Date = entryDate,
                            VehicleId = vehicle.Id
                        });

                        fuelExpensesWithEntry.Add(new Expense
                        {
                            Category = ExpenseCategory.Other,
                            Amount = fuelCostDiesel,
                            Date = entryDate,
                            Notes = fuelNoteDiesel,
                            VehicleId = vehicle.Id
                        });
                        break;

                    case VehicleType.Electric:
                        var kwhElectric = random.Next(30, 60);
                        var pricePerKwhElectric = (decimal)(random.Next(12, 20) / 100.0);
                        var fuelCostElectric = kwhElectric * pricePerKwhElectric;
                        var fuelNoteElectric = $"Charging session: {kwhElectric} kWh";

                        fuelEntries.Add(new FuelEntry
                        {
                            EnergyType = EnergyType.Electricity,
                            Amount = kwhElectric,
                            Cost = fuelCostElectric,
                            Odometer = currentOdometer,
                            Date = entryDate,
                            VehicleId = vehicle.Id
                        });

                        fuelExpensesWithEntry.Add(new Expense
                        {
                            Category = ExpenseCategory.Other,
                            Amount = fuelCostElectric,
                            Date = entryDate,
                            Notes = fuelNoteElectric,
                            VehicleId = vehicle.Id
                        });
                        break;

                    case VehicleType.Hybrid:
                    case VehicleType.PlugInHybrid:
                        // Alternate between gas and electric
                        var useElectric = vehicle.VehicleType == VehicleType.PlugInHybrid
                            ? random.Next(0, 10) < 7 // 70% electric for PHEV
                            : random.Next(0, 10) < 3; // 30% electric for hybrid

                        if (useElectric)
                        {
                            var kwhHybrid = random.Next(15, 35);
                            var pricePerKwhHybrid = (decimal)(random.Next(12, 20) / 100.0);
                            var fuelCostHybridElec = kwhHybrid * pricePerKwhHybrid;
                            var fuelNoteHybridElec = $"Charging session: {kwhHybrid} kWh";

                            fuelEntries.Add(new FuelEntry
                            {
                                EnergyType = EnergyType.Electricity,
                                Amount = kwhHybrid,
                                Cost = fuelCostHybridElec,
                                Odometer = currentOdometer,
                                Date = entryDate,
                                VehicleId = vehicle.Id
                            });

                            fuelExpensesWithEntry.Add(new Expense
                            {
                                Category = ExpenseCategory.Other,
                                Amount = fuelCostHybridElec,
                                Date = entryDate,
                                Notes = fuelNoteHybridElec,
                                VehicleId = vehicle.Id
                            });
                        }
                        else
                        {
                            var litersHybrid = random.Next(25, 45);
                            var pricePerLiterHybrid = (decimal)(random.Next(135, 165) / 100.0);
                            var fuelCostHybridGas = litersHybrid * pricePerLiterHybrid;
                            var fuelNoteHybridGas = $"Gasoline fill-up: {litersHybrid}L";

                            fuelEntries.Add(new FuelEntry
                            {
                                EnergyType = EnergyType.Gasoline,
                                Amount = litersHybrid,
                                Cost = fuelCostHybridGas,
                                Odometer = currentOdometer,
                                Date = entryDate,
                                VehicleId = vehicle.Id
                            });

                            fuelExpensesWithEntry.Add(new Expense
                            {
                                Category = ExpenseCategory.Other,
                                Amount = fuelCostHybridGas,
                                Date = entryDate,
                                Notes = fuelNoteHybridGas,
                                VehicleId = vehicle.Id
                            });
                        }
                        break;
                }
            }
        }

        // CRITICAL: Save expenses FIRST so they get IDs
        await _context.Expenses.AddRangeAsync(fuelExpensesWithEntry);
        await _context.SaveChangesAsync();

        // Now link FuelEntries to their corresponding Expenses (1:1 relationship)
        for (int i = 0; i < fuelEntries.Count; i++)
        {
            fuelEntries[i].ExpenseId = fuelExpensesWithEntry[i].Id;
        }

        // Save FuelEntries with linked ExpenseIds
        await _context.FuelEntries.AddRangeAsync(fuelEntries);
        await _context.SaveChangesAsync();

       _logger.LogInformation(
        "Created {FuelEntryCount} fuel entries (with linked expenses)", 
        fuelEntries.Count);
    }

}
