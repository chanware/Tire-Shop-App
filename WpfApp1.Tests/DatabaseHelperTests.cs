using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Data.SQLite;
using System.Linq;
using WpfApp1.Helpers;
using WpfApp1.Models;

namespace WpfApp1.Tests
{
    [TestClass]
    public class DatabaseHelperTests
    {
        private DatabaseHelper _dbHelper;
        private string _testDbPath;

        [TestInitialize]
        public void TestInitialize()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), "test_database.db");
            Environment.SetEnvironmentVariable("DATABASE_PATH", _testDbPath);
            _dbHelper = new DatabaseHelper();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
            Environment.SetEnvironmentVariable("DATABASE_PATH", null);
        }

        [TestMethod]
        public void AddOrUpdateCustomer_ShouldAddNewCustomer()
        {
            // Arrange
            var newCustomer = new Customer
            {
                Name = "John Doe",
                Phone = "123-456-7890",
                Email = "john@example.com",
                Address = "123 Test St",
                City = "Testville",
                Country = "USA"
            };

            // Act
            int customerId = _dbHelper.AddOrUpdateCustomer(newCustomer);

            // Assert
            Assert.IsTrue(customerId > 0);
            var savedCustomer = GetCustomerById(customerId);
            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual(newCustomer.Name, savedCustomer.Name);
            Assert.AreEqual(newCustomer.Phone, savedCustomer.Phone);
            Assert.AreEqual(newCustomer.Email, savedCustomer.Email);
        }

        [TestMethod]
        public void AddOrUpdateCustomer_ShouldUpdateExistingCustomer()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Jane Doe",
                Phone = "987-654-3210",
                Email = "jane@example.com",
                Address = "456 Test Ave",
                City = "Testburg",
                Country = "Canada"
            };
            int customerId = _dbHelper.AddOrUpdateCustomer(customer);

            customer.Id = customerId;
            customer.Name = "Jane Smith";
            customer.Phone = "555-555-5555";

            // Act
            _dbHelper.AddOrUpdateCustomer(customer);

            // Assert
            var updatedCustomer = GetCustomerById(customerId);
            Assert.IsNotNull(updatedCustomer);
            Assert.AreEqual("Jane Smith", updatedCustomer.Name);
            Assert.AreEqual("555-555-5555", updatedCustomer.Phone);
            Assert.AreEqual("jane@example.com", updatedCustomer.Email);
        }

        [TestMethod]
        public void GetCustomers_ShouldReturnAllCustomers()
        {
            // Arrange
            var customer1 = new Customer { Name = "Customer 1", Phone = "111-111-1111" };
            var customer2 = new Customer { Name = "Customer 2", Phone = "222-222-2222" };
            _dbHelper.AddOrUpdateCustomer(customer1);
            _dbHelper.AddOrUpdateCustomer(customer2);

            // Act
            var customers = _dbHelper.GetCustomers();

            // Assert
            Assert.IsTrue(customers.Count >= 2);
            Assert.IsTrue(customers.Any(c => c.Name == "Customer 1" && c.Phone == "111-111-1111"));
            Assert.IsTrue(customers.Any(c => c.Name == "Customer 2" && c.Phone == "222-222-2222"));
        }

        [TestMethod]
        public void DeleteCustomer_ShouldRemoveCustomerFromDatabase()
        {
            // Arrange
            var customer = new Customer { Name = "To Be Deleted", Phone = "999-999-9999" };
            int customerId = _dbHelper.AddOrUpdateCustomer(customer);

            // Act
            _dbHelper.DeleteCustomer(customerId);

            // Assert
            var deletedCustomer = GetCustomerById(customerId);
            Assert.IsNull(deletedCustomer);
        }

        private Customer GetCustomerById(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Customers WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                                Country = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString(reader.GetOrdinal("Country"))
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}