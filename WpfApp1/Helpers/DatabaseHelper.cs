using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using WpfApp1.Models;

namespace WpfApp1.Helpers
{
    public class DatabaseHelper
    {
        private readonly string _databasePath;

        public DatabaseHelper()
        {
            _databasePath = Path.Combine(Directory.GetCurrentDirectory(), "tireShopDatabase.db");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_databasePath))
            {
                SQLiteConnection.CreateFile(_databasePath);
                CreateTables();
            }
        }

        private void CreateTables()
        {
            using (var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT NOT NULL UNIQUE,
                            PasswordHash TEXT NOT NULL
                        )");

                        ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS Customers (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Phone TEXT NOT NULL,
                            Email TEXT,
                            Address TEXT,
                            Address2 TEXT,
                            City TEXT,
                            Country TEXT,
                            VehicleBrandId INTEGER,
                            FOREIGN KEY (VehicleBrandId) REFERENCES Vehicles(Id)
                        )");

                        ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS Vehicles (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Brand TEXT NOT NULL,
                            Category TEXT NOT NULL
                        )");

                        ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS Appointments (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            CustomerId INTEGER,
                            Date TEXT NOT NULL,
                            Description TEXT,
                            SelectedTireBrand TEXT,
                            FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
                        )");

                        ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS TireBrands (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            BrandName TEXT NOT NULL,
                            TireCategory TEXT NOT NULL,
                            StockQuantity INTEGER NOT NULL DEFAULT 0
                        )");

                        ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS VehicleCategories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Category TEXT NOT NULL UNIQUE
                        )");

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error creating database tables", ex);
                    }
                }
            }
            EnsureVehicleCategories();
        }

        // USER MANAGEMENT /////////////////////////////////////////////////////////////////////////////////

        public bool AddUser(string username, string password)
        {
            string passwordHash = HashPassword(password);
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string query = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (SQLiteException)
            {
                return false; // User already exists or other database error
            }
        }

        public bool VerifyUser(string username, string password)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT PasswordHash FROM Users WHERE Username = @Username";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        string storedHash = result.ToString();
                        return VerifyPassword(password, storedHash);
                    }
                }
            }
            return false;
        }

        private string HashPassword(string password)
        {
            // For simplicity, we're using a basic hash. In a real-world scenario, use a more secure method.
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            string inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }


        // CUSTOMERS //////////////////////////////////////////////////////////////////////////////////////

        public int AddOrUpdateCustomer(Customer customer)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int customerId;
                        if (customer.Id == 0)
                        {
                            string insertQuery = @"INSERT INTO Customers (Name, Phone, Email, Address, Address2, City, Country, VehicleBrandId) 
                                               VALUES (@Name, @Phone, @Email, @Address, @Address2, @City, @Country, @VehicleBrandId);
                                               SELECT last_insert_rowid();";
                            using (var command = new SQLiteCommand(insertQuery, connection, transaction))
                            {
                                AddCustomerParameters(command, customer);
                                customerId = Convert.ToInt32(command.ExecuteScalar());
                            }
                        }
                        else
                        {
                            string updateQuery = @"UPDATE Customers 
                                               SET Name = @Name, Phone = @Phone, Email = @Email, 
                                                   Address = @Address, Address2 = @Address2, City = @City, 
                                                   Country = @Country, VehicleBrandId = @VehicleBrandId
                                               WHERE Id = @Id";
                            using (var command = new SQLiteCommand(updateQuery, connection, transaction))
                            {
                                AddCustomerParameters(command, customer);
                                command.Parameters.AddWithValue("@Id", customer.Id);
                                command.ExecuteNonQuery();
                            }
                            customerId = customer.Id;
                        }

                        transaction.Commit();
                        return customerId;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error adding or updating customer", ex);
                    }
                }
            }
        }

        private void AddCustomerParameters(SQLiteCommand command, Customer customer)
        {
            command.Parameters.AddWithValue("@Name", customer.Name);
            command.Parameters.AddWithValue("@Phone", customer.Phone);
            command.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address2", customer.Address2 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@City", customer.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", customer.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@VehicleBrandId", customer.VehicleBrandId ?? (object)DBNull.Value);
        }

        public List<Customer> GetCustomers()
        {
            List<Customer> customers = new List<Customer>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string selectQuery = @"SELECT c.*, v.Brand as VehicleBrand, v.Category as VehicleCategory 
                                   FROM Customers c
                                   LEFT JOIN Vehicles v ON c.VehicleBrandId = v.Id";
                using (var command = new SQLiteCommand(selectQuery, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Email = reader["Email"].ToString(),
                            Address = reader["Address"].ToString(),
                            Address2 = reader["Address2"].ToString(),
                            City = reader["City"].ToString(),
                            Country = reader["Country"].ToString(),
                            VehicleBrandId = reader["VehicleBrandId"] != DBNull.Value ? Convert.ToInt32(reader["VehicleBrandId"]) : (int?)null,
                            VehicleBrand = reader["VehicleBrand"].ToString(),
                            VehicleCategory = reader["VehicleCategory"].ToString()
                        });
                    }
                }
            }

            return customers;
        }

        public void DeleteCustomer(int customerId, bool deleteAppointments = false)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string checkAppointmentsQuery = "SELECT COUNT(*) FROM Appointments WHERE CustomerId = @Id";
                        int appointmentCount;
                        using (var checkCommand = new SQLiteCommand(checkAppointmentsQuery, connection, transaction))
                        {
                            checkCommand.Parameters.AddWithValue("@Id", customerId);
                            appointmentCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                        }

                        if (appointmentCount > 0 && !deleteAppointments)
                        {
                            throw new InvalidOperationException($"Cannot delete customer. There are {appointmentCount} appointment(s) associated with this customer.");
                        }

                        if (deleteAppointments)
                        {
                            ExecuteNonQuery(connection, "DELETE FROM Appointments WHERE CustomerId = @Id", new SQLiteParameter("@Id", customerId));
                        }

                        ExecuteNonQuery(connection, "DELETE FROM Customers WHERE Id = @Id", new SQLiteParameter("@Id", customerId));
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error deleting customer", ex);
                    }
                }
            }
        }

        // APPOINTMENTS ///////////////////////////////////////////////////////////////////////////////////

        public void AddOrUpdateAppointment(Appointment appointment)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = appointment.Id == 0
                            ? "INSERT INTO Appointments (CustomerId, Date, Description, SelectedTireBrand) VALUES (@CustomerId, @Date, @Description, @SelectedTireBrand); SELECT last_insert_rowid();"
                            : "UPDATE Appointments SET CustomerId = @CustomerId, Date = @Date, Description = @Description, SelectedTireBrand = @SelectedTireBrand WHERE Id = @Id";

                        using (var command = new SQLiteCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CustomerId", appointment.CustomerId);
                            command.Parameters.AddWithValue("@Date", appointment.Date);
                            command.Parameters.AddWithValue("@Description", appointment.Description);
                            command.Parameters.AddWithValue("@SelectedTireBrand", appointment.SelectedTireBrand);
                            if (appointment.Id != 0)
                            {
                                command.Parameters.AddWithValue("@Id", appointment.Id);
                            }

                            if (appointment.Id == 0)
                            {
                                appointment.Id = Convert.ToInt32(command.ExecuteScalar());
                            }
                            else
                            {
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<Appointment> GetAppointments()
        {
            var appointments = new List<Appointment>();
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
            SELECT a.*, c.Name as CustomerName 
            FROM Appointments a
            LEFT JOIN Customers c ON a.CustomerId = c.Id";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        appointments.Add(new Appointment
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            Date = reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            SelectedTireBrand = reader.IsDBNull(4) ? null : reader.GetString(4),
                            CustomerName = reader.IsDBNull(5) ? null : reader.GetString(5)
                        });
                    }
                }
            }
            return appointments;
        }


        public int GetAppointmentCountForCustomer(int customerId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Appointments WHERE CustomerId = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", customerId);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void DeleteAppointment(int appointmentId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = "DELETE FROM Appointments WHERE Id = @Id";
                        using (var command = new SQLiteCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", appointmentId);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // TIRES //////////////////////////////////////////////////////////////////////////////////////////

        public void AddTireBrand(string brandName, string tireCategory)
        {
            var tireBrand = new TireBrand
            {
                BrandName = brandName,
                TireCategory = tireCategory,
                StockQuantity = 0,
            };
            AddOrUpdateTireBrand(tireBrand);
        }

        public void AddOrUpdateTireBrand(TireBrand tireBrand)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = tireBrand.Id == 0
                    ? "INSERT INTO TireBrands (BrandName, TireCategory, StockQuantity) VALUES (@BrandName, @TireCategory, @StockQuantity); SELECT last_insert_rowid();"
                    : "UPDATE TireBrands SET BrandName = @BrandName, TireCategory = @TireCategory, StockQuantity = @StockQuantity WHERE Id = @Id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BrandName", tireBrand.BrandName);
                    command.Parameters.AddWithValue("@TireCategory", tireBrand.TireCategory);
                    command.Parameters.AddWithValue("@StockQuantity", tireBrand.StockQuantity);
                    if (tireBrand.Id != 0)
                    {
                        command.Parameters.AddWithValue("@Id", tireBrand.Id);
                    }

                    if (tireBrand.Id == 0)
                    {
                        tireBrand.Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<TireBrand> GetTireBrands()
        {
            List<TireBrand> tireBrands = new List<TireBrand>();
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT Id, BrandName, TireCategory, StockQuantity FROM TireBrands";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tireBrands.Add(new TireBrand
                        {
                            Id = reader.GetInt32(0),
                            BrandName = reader.GetString(1),
                            TireCategory = reader.GetString(2),
                            StockQuantity = reader.GetInt32(3)
                        });
                    }
                }
            }
            return tireBrands;
        }

        public void DeleteTireBrandAndUpdateReferences(int tireBrandId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update appointments to remove references to the deleted tire brand
                        string updateAppointmentsQuery = "UPDATE Appointments SET SelectedTireBrand = NULL WHERE SelectedTireBrand = (SELECT BrandName FROM TireBrands WHERE Id = @TireBrandId)";
                        ExecuteNonQuery(connection, updateAppointmentsQuery, new SQLiteParameter("@TireBrandId", tireBrandId));

                        // Delete the tire brand
                        string deleteTireBrandQuery = "DELETE FROM TireBrands WHERE Id = @TireBrandId";
                        ExecuteNonQuery(connection, deleteTireBrandQuery, new SQLiteParameter("@TireBrandId", tireBrandId));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public string GetRecommendedTireBrand(string vehicleCategory)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT BrandName FROM TireBrands WHERE TireCategory = @VehicleCategory LIMIT 1";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VehicleCategory", vehicleCategory);
                    var result = command.ExecuteScalar();
                    return result != null ? result.ToString() : string.Empty;
                }
            }
        }

        // VEHICLES ///////////////////////////////////////////////////////////////////////////////////////

        public List<Vehicle> GetVehicles()
        {
            List<Vehicle> vehicles = new List<Vehicle>();
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT Id, Brand, Category FROM Vehicles";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vehicles.Add(new Vehicle
                        {
                            Id = reader.GetInt32(0),
                            Brand = reader.GetString(1),
                            Category = reader.GetString(2)
                        });
                    }
                }
            }
            return vehicles;
        }

        public int AddOrUpdateVehicle(Vehicle vehicle)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ExecuteNonQuery(connection, "INSERT OR IGNORE INTO VehicleCategories (Category) VALUES (@Category)", new SQLiteParameter("@Category", vehicle.Category));

                        string query = vehicle.Id == 0
                            ? "INSERT INTO Vehicles (Brand, Category) VALUES (@Brand, @Category); SELECT last_insert_rowid();"
                            : "UPDATE Vehicles SET Brand = @Brand, Category = @Category WHERE Id = @Id";

                        using (var command = new SQLiteCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Brand", vehicle.Brand);
                            command.Parameters.AddWithValue("@Category", vehicle.Category);
                            if (vehicle.Id != 0)
                            {
                                command.Parameters.AddWithValue("@Id", vehicle.Id);
                            }

                            if (vehicle.Id == 0)
                            {
                                vehicle.Id = Convert.ToInt32(command.ExecuteScalar());
                            }
                            else
                            {
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return vehicle.Id;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error adding or updating vehicle", ex);
                    }
                }
            }
        }

        public void RemoveVehicleBrandFromCustomers(int vehicleBrandId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = "UPDATE Customers SET VehicleBrandId = NULL WHERE VehicleBrandId = @Id";
                        using (var command = new SQLiteCommand(updateQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", vehicleBrandId);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<string> GetVehicleBrands()
        {
            var brands = new List<string>();
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT DISTINCT Brand FROM Vehicles", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        brands.Add(reader.GetString(0));
                    }
                }
            }
            return brands;
        }

        public void DeleteVehicleAndUpdateReferences(int vehicleId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ExecuteNonQuery(connection, "UPDATE Customers SET VehicleBrandId = NULL WHERE VehicleBrandId = @Id", new SQLiteParameter("@Id", vehicleId));
                        ExecuteNonQuery(connection, "UPDATE Appointments SET SelectedTireBrand = NULL WHERE CustomerId IN (SELECT Id FROM Customers WHERE VehicleBrandId = @Id)", new SQLiteParameter("@Id", vehicleId));
                        ExecuteNonQuery(connection, "DELETE FROM Vehicles WHERE Id = @Id", new SQLiteParameter("@Id", vehicleId));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void EnsureVehicleCategories()
        {
            var categories = new[] { "Sedan", "SUV", "Truck", "Electric", "Van", "Coupe", "Hatchback" };

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // First, create the table if it doesn't exist
                        ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS VehicleCategories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Category TEXT NOT NULL UNIQUE
                    )");

                        foreach (var category in categories)
                        {
                            ExecuteNonQuery(connection,
                                "INSERT OR IGNORE INTO VehicleCategories (Category) VALUES (@Category)",
                                new SQLiteParameter("@Category", category));
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<string> GetVehicleCategories()
        {
            var categories = new List<string>();
            using (var connection = GetConnection())
            {
                connection.Open();
                try
                {
                    // Check if the table exists, if not, create and populate it
                    ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS VehicleCategories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Category TEXT NOT NULL UNIQUE
                )");

                    EnsureVehicleCategories();

                    using (var command = new SQLiteCommand("SELECT Category FROM VehicleCategories ORDER BY Category", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(reader.GetString(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error getting vehicle categories", ex);
                }
            }
            return categories;
        }

        // DATABASE CONNECTION //////////////////////////////////////////////////////////////////////////////

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={_databasePath};Version=3;");
        }

        private void ExecuteNonQuery(SQLiteConnection connection, string query, params SQLiteParameter[] parameters)
        {
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        // FOR TESTING PURPOSES ONLY //////////////////////////////////////////////////////////////////////

        public void PurgeDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string[] tables = { "Customers", "Appointments", "Vehicles", "TireBrands" };
                        foreach (var table in tables)
                        {
                            ExecuteNonQuery(connection, $"DELETE FROM {table}");
                            ExecuteNonQuery(connection, $"DELETE FROM sqlite_sequence WHERE name='{table}'");
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error purging database", ex);
                    }
                }
            }
        }

        public void ClearAllData()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ExecuteNonQuery(connection, "DELETE FROM TireBrands");
                        ExecuteNonQuery(connection, "DELETE FROM Customers");
                        ExecuteNonQuery(connection, "DELETE FROM Vehicles");
                        ExecuteNonQuery(connection, "DELETE FROM Appointments");

                        // Reset auto-increment counters
                        ExecuteNonQuery(connection, "DELETE FROM sqlite_sequence");

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
