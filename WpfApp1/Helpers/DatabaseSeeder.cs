using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1.Helpers
{
    public class DatabaseSeeder
    {
        private readonly DatabaseHelper _dbHelper;

        public DatabaseSeeder(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        private void SeedDefaultUser()
        {
            _dbHelper.AddUser("CarloSr", "tired");
        }

        public void SeedDataIfNeeded()
        {
            if (IsSeedingNeeded())
            {
                SeedData();
                SeedDefaultUser();
            }
        }

        public void ForceSeedData()
        {
            _dbHelper.ClearAllData();
            SeedData();
            SeedDefaultUser();
        }

        private bool IsSeedingNeeded()
        {
            return !_dbHelper.GetCustomers().Any() && !_dbHelper.GetVehicles().Any() && !_dbHelper.GetTireBrands().Any();
        }

        private void SeedData()
        {
            SeedVehicleCategories(); //Run this first
            SeedVehicles(); // Run this second
            SeedCustomers(); // This should come after SeedVehicles
            SeedTireBrands();
        }

        private void SeedVehicleCategories()
        {
            Console.WriteLine("Seeding vehicle categories...");
            _dbHelper.EnsureVehicleCategories();
            Console.WriteLine("Vehicle categories seeded.");
        }

        private void SeedVehicles()
        {
            Console.WriteLine("Seeding vehicles...");
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Brand = "TUFF Motors", Category = "Truck" },
                new Vehicle { Brand = "Sannis", Category = "Sedan" },
                new Vehicle { Brand = "Nikola Wheels", Category = "Electric" },
                new Vehicle { Brand = "FamilyMov", Category = "SUV" },
                new Vehicle { Brand = "SpeedyGo", Category = "Coupe" },
                new Vehicle { Brand = "CityRunner", Category = "Hatchback" },
                new Vehicle { Brand = "WorkHorse", Category = "Van" },
                new Vehicle { Brand = "EcoRide", Category = "Electric" },
                new Vehicle { Brand = "LuxeCruiser", Category = "Sedan" },
                new Vehicle { Brand = "AdventureSUV", Category = "SUV" }
            };

            foreach (var vehicle in vehicles)
            {
                _dbHelper.AddOrUpdateVehicle(vehicle);
                Console.WriteLine($"Added/Updated vehicle: {vehicle.Brand} (Category: {vehicle.Category})");
            }
        }

        private void SeedCustomers()
        {
            Console.WriteLine("Seeding customers...");

            var vehicles = _dbHelper.GetVehicles();

            var customers = new List<Customer>
    {
        new Customer
        {
            Name = "Steven Seagull",
            Phone = "555-123-6789",
            Email = "steven.seagull@tidesmail.com",
            Address = "456 Ocean Breeze Ave",
            Address2 = "Apt 2B",
            City = "Seabreeze",
            Country = "USA",
            VehicleBrand = "FamilyMov",
            VehicleCategory = "SUV"
        },
        new Customer
        {
            Name = "Tony Starkfish",
            Phone = "555-987-1234",
            Email = "tony.starkfish@arcnet.com",
            Address = "99 Shell Drive",
            Address2 = "Suite 100",
            City = "Coral Cove",
            Country = "USA",
            VehicleBrand = "Nikola Wheels",
            VehicleCategory = "Electric"
        },
        new Customer
        {
            Name = "Ella Vator",
            Phone = "555-246-1357",
            Email = "ella.vator@upanddown.com",
            Address = "77 Skyline Tower",
            Address2 = "Unit 55B",
            City = "Elevatia",
            Country = "USA",
            VehicleBrand = "CityRunner",
            VehicleCategory = "Hatchback"
        },
        new Customer
        {
            Name = "Jack Hammerstone",
            Phone = "555-765-4321",
            Email = "jack.hammerstone@toolshed.net",
            Address = "200 Quarry Lane",
            Address2 = "Building 3",
            City = "Rockville",
            Country = "USA",
            VehicleBrand = "TUFF Motors",
            VehicleCategory = "Truck"
        },
        new Customer
        {
            Name = "Wanda Blurr",
            Phone = "555-432-8751",
            Email = "wanda.blurr@speedmail.com",
            Address = "10 Velocity Street",
            City = "Fastlane",
            Country = "USA",
            VehicleBrand = "SpeedyGo",
            VehicleCategory = "Coupe"
        },
        new Customer
        {
            Name = "Bruce D. Rain",
            Phone = "555-321-6785",
            Email = "bruce.rain@cloudyweather.com",
            Address = "32 Thunderbolt Terrace",
            Address2 = "Unit 11C",
            City = "Stormport",
            Country = "USA",
            VehicleBrand = "EcoRide",
            VehicleCategory = "Electric"
        },
        new Customer
        {
            Name = "Dexter Chievous",
            Phone = "555-876-5432",
            Email = "dexter.chievous@inlab.com",
            Address = "5010 Experiment Road",
            Address2 = "Suite 404",
            City = "Testfield",
            Country = "USA",
            VehicleBrand = "LuxeCruiser",
            VehicleCategory = "Sedan"
        },
        new Customer
        {
            Name = "Lola Mowtive",
            Phone = "555-290-6512",
            Email = "lola.mowtive@gearworks.com",
            Address = "47 Engine Court",
            Address2 = "Apt 3C",
            City = "Motortown",
            Country = "USA",
            VehicleBrand = "WorkHorse",
            VehicleCategory = "Van"
        },
        new Customer
        {
            Name = "Alana Wattson",
            Phone = "555-810-2929",
            Email = "alana.wattson@voltcity.com",
            Address = "158 Circuit Avenue",
            Address2 = "Apt 14D",
            City = "Powersurge",
            Country = "USA",
            VehicleBrand = "Nikola Wheels",
            VehicleCategory = "Electric"
        },
        new Customer
        {
            Name = "Brooke Lynn Bridge",
            Phone = "555-902-4875",
            Email = "brooke.bridge@cityspans.com",
            Address = "600 River Crossing Lane",
            Address2 = "Apt 7B",
            City = "Spanville",
            Country = "USA",
            VehicleBrand = "Sannis",
            VehicleCategory = "Sedan"
        },
        new Customer
        {
            Name = "Ray Shore",
            Phone = "555-903-1234",
            Email = "ray.shore@surfmail.com",
            Address = "12 Coastal Drive",
            Address2 = "Apt 1A",
            City = "Seaside Heights",
            Country = "USA",
            VehicleBrand = "FamilyMov",
            VehicleCategory = "SUV"
        },
        new Customer
        {
            Name = "Chris P. Bacon",
            Phone = "555-221-7772",
            Email = "chris.bacon@frymail.com",
            Address = "55 Maplewood Terrace",
            Address2 = "Unit 12",
            City = "Greaseville",
            Country = "USA",
            VehicleBrand = "TUFF Motors",
            VehicleCategory = "Truck"
        },
        new Customer
        {
            Name = "Tim Burr",
            Phone = "555-765-1010",
            Email = "tim.burr@treefellows.com",
            Address = "444 Timber Way",
            Address2 = "Unit 2C",
            City = "Woodcrest",
            Country = "USA",
            VehicleBrand = "AdventureSUV",
            VehicleCategory = "SUV"
        },
        new Customer
        {
            Name = "Roxanne Roll",
            Phone = "555-310-1222",
            Email = "roxanne.roll@rocknmail.com",
            Address = "9 Melody Lane",
            Address2 = "Apt 9F",
            City = "Musictown",
            Country = "USA",
            VehicleBrand = "SpeedyGo",
            VehicleCategory = "Coupe"
        }
    };

            foreach (var customer in customers)
            {
                var vehicle = vehicles.FirstOrDefault(v => v.Brand == customer.VehicleBrand);
                if (vehicle != null)
                {
                    customer.VehicleBrandId = vehicle.Id;
                }

                var customerId = _dbHelper.AddOrUpdateCustomer(customer);
                Console.WriteLine($"Added/Updated customer: {customer.Name} (ID: {customerId})");
            }
        }


        private void SeedTireBrands()
        {
            Console.WriteLine("Seeding tire brands...");
            var tireBrands = new List<TireBrand>
            {
                new TireBrand { BrandName = "DUTY Tires", TireCategory = "Truck", StockQuantity = 50 },
                new TireBrand { BrandName = "AV Allseason", TireCategory = "Sedan", StockQuantity = 100 },
                new TireBrand { BrandName = "TufRite Rubber", TireCategory = "SUV", StockQuantity = 75 },
                new TireBrand { BrandName = "TorqField Tires", TireCategory = "Electric", StockQuantity = 60 },
                new TireBrand { BrandName = "SpeedMaster", TireCategory = "Coupe", StockQuantity = 40 },
                new TireBrand { BrandName = "CityGrip", TireCategory = "Hatchback", StockQuantity = 80 },
                new TireBrand { BrandName = "HeavyHauler", TireCategory = "Van", StockQuantity = 30 }
            };

            foreach (var brand in tireBrands)
            {
                _dbHelper.AddOrUpdateTireBrand(brand);
                Console.WriteLine($"Added/Updated tire brand: {brand.BrandName} (Category: {brand.TireCategory})");
            }
        }
    }
}