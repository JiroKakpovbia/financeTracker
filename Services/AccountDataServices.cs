using SQLite;
using System.Collections.ObjectModel;
using trackr.Models;

namespace trackr
{
    public class AccountDataService
    {
        private static SQLiteAsyncConnection? _db;

        // Ensure the database is initialized before any operations
        private static async Task EnsureInitializedAsync()
        {
            if (_db == null)
                await InitializeAccountDataAsync();
        }

        // Get the SQLiteAsyncConnection, initializing it if necessary
        private static async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            await EnsureInitializedAsync();
            return _db ?? throw new InvalidOperationException("SQLite connection was not initialized.");
        }

        // Initialize the SQLite database connection and create tables if they don't exist
        private static async Task InitializeAccountDataAsync()
        {
            if (_db != null)
                return;

            Console.WriteLine("Initializing SQLite database connection...");

            try
            {
                var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "trackr.db3");
                _db = new SQLiteAsyncConnection(
                    dbPath,
                    SQLiteOpenFlags.ReadWrite |
                    SQLiteOpenFlags.Create |
                    SQLiteOpenFlags.FullMutex);
                await _db.CreateTablesAsync<BankAccount, Transaction, Category, SubCategory, ImportBatch>();

                Console.WriteLine($"SQLite database initialized at {dbPath}\n");

                // Seed the database with default categories if none exist
                await SeedCategoriesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SQLite database: {ex.Message}\n");
                throw;
            }
        }

        // Seed the database with default categories if none exist
        private static async Task SeedCategoriesAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            // Retrieve existing categories to check if seeding is necessary
            var existingCategories = await db.Table<Category>().ToListAsync();

            // If no categories exist, seed the database with default categories
            if (existingCategories.Count == 0)
            {
                Console.WriteLine("Seeding default categories into the database...");

                // Define default categories with their respective colors and icons
                var defaultCategories = new List<Category>
                {
                    new()
                    {
                        Name = "Income",
                        Colour = "#FF6347",
                        Icon = "grocery_icon.png"
                    },
                    new()
                    {
                        Name = "Savings",
                        Colour = "#1E90FF",
                        Icon = "utilities_icon.png"
                    },
                    new()
                    {
                        Name = "Housing",
                        Colour = "#32CD32",
                        Icon = "entertainment_icon.png"
                    },
                    new()
                    {
                        Name = "Communications",
                        Colour = "#FFD700",
                        Icon = "transportation_icon.png"
                    },
                    new()
                    {
                        Name = "Food",
                        Colour = "#FF69B4",
                        Icon = "healthcare_icon.png"
                    },
                    new()
                    {
                        Name = "Insurance",
                        Colour = "#FF69B4",
                        Icon = "insurance_icon.png"
                    },
                    new()
                    {
                        Name = "Transportation",
                        Colour = "#FF69B4",
                        Icon = "transportation_icon.png"
                    },
                    new()
                    {
                        Name = "Education",
                        Colour = "#FF69B4",
                        Icon = "education_icon.png"
                    },
                    new()
                    {
                        Name = "Recreation",
                        Colour = "#FF69B4",
                        Icon = "recreation_icon.png"
                    },
                    new()
                    {
                        Name = "Personal Care",
                        Colour = "#FF69B4",
                        Icon = "personal_care_icon.png"
                    },
                    new()
                    {
                        Name = "Fees",
                        Colour = "#FF69B4",
                        Icon = "fees_icon.png"
                    }
                };

                // Insert the default categories into the database
                await db.InsertAllAsync(defaultCategories);

                // Seed subcategories after seeding categories
                await SeedSubCategoriesAsync();

                Console.WriteLine("Default categories and subcategories seeded successfully.\n");
            }
        }

        // Seed the database with default subcategories if none exist
        private static async Task SeedSubCategoriesAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            // Retrieve default categories to associate with subcategories
            var income = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Income");

            var savings = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Savings");

            var housing = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Housing");

            var communications = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Communications");

            var food = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Food");

            var insurance = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Insurance");

            var transportation = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Transportation");

            var education = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Education");

            var recreation = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Recreation");

            var personalCare = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Personal Care");

            var fees = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Fees");

            // Define default subcategories for each category
            var defaultSubCategories = new List<SubCategory>
            {
                // Income subcategories
                new() {
                    CategoryId = income.Id,
                    Name = "Net Income"
                },
                new() {
                    CategoryId = income.Id,
                    Name = "Spouse/Partner's Net Income"
                },
                new() {
                    CategoryId = income.Id,
                    Name = "Employment Insurance (EI)"
                },

                // Savings subcategories
                new() {
                    CategoryId = savings.Id,
                    Name = "Emergency Fund"
                },
                new() {
                    CategoryId = savings.Id,
                    Name = "Retirement"
                },
                new() {
                    CategoryId = savings.Id,
                    Name = "Education"
                },
                new() {
                    CategoryId = savings.Id,
                    Name = "Home Purchase"
                },
                new() {
                    CategoryId = savings.Id,
                    Name = "Car Purchase"
                },
                new() {
                    CategoryId = savings.Id,
                    Name = "Income Tax"
                },

                // Housing subcategories
                new() {
                    CategoryId = housing.Id,
                    Name = "Rent/Mortgage"
                },
                new() {
                    CategoryId = housing.Id,
                    Name = "Tenant Insurance"
                },
                new() {
                    CategoryId = housing.Id,
                    Name = "Furniture/Appliances"
                },
                new() {
                    CategoryId = housing.Id,
                    Name = "Electricity"
                },
                new() {
                    CategoryId = housing.Id,
                    Name = "Water/Sewer"
                },
                new() {
                    CategoryId = housing.Id,
                    Name = "Heating"
                },

                // Communications subcategories
                new() {
                    CategoryId = communications.Id,
                    Name = "Telephone/Cell Phone"
                },
                new() {
                    CategoryId = communications.Id,
                    Name = "Cable/Satellite TV"
                },
                new() {
                    CategoryId = communications.Id,
                    Name = "Internet"
                },
                new() {
                    CategoryId = communications.Id,
                    Name = "Combined Packages (TV, Internet, Phone)"
                },
                new() {
                    CategoryId = communications.Id,
                    Name = "Entertainment Subscriptions (Netflix, Spotify, etc.)"
                },

                // Food subcategories
                new() {
                    CategoryId = food.Id,
                    Name = "Groceries"
                },
                new() {
                    CategoryId = food.Id,
                    Name = "Restaurants/Takeout"
                },
                new() {
                    CategoryId = food.Id,
                    Name = "Coffee Shops"
                },

                // Insurance subcategories
                new() {
                    CategoryId = insurance.Id,
                    Name = "Life Insurance"
                },
                new() {
                    CategoryId = insurance.Id,
                    Name = "Medical/Dental Insurance"
                },
                new() {
                    CategoryId = insurance.Id,
                    Name = "Disability/Accident Insurance"
                },

                // Transportation subcategories
                new() {
                    CategoryId = transportation.Id,
                    Name = "Car Loan/Lease Payments"
                },
                new() {
                    CategoryId = transportation.Id,
                    Name = "Car Insurance"
                },
                new() {
                    CategoryId = transportation.Id,
                    Name = "Gas/Fuel"
                },
                new() {
                    CategoryId = transportation.Id,
                    Name = "Maintenance & Repairs"
                },
                new() {
                    CategoryId = transportation.Id,
                    Name = "Car License/Registration"
                },
                new() {
                    CategoryId = transportation.Id,
                    Name = "Parking"
                },
                new() {
                    CategoryId = transportation.Id,
                    Name = "Public Transit (Bus, Subway, Train)"
                },
                new() {
                    CategoryId = transportation.Id,
                    Name = "Ride Services (Uber, Lyft, etc.)"
                },

                // Education subcategories
                new() {
                    CategoryId = education.Id,
                    Name = "Tuition"
                },
                new() {
                    CategoryId = education.Id,
                    Name = "Textbooks & Supplies"
                },

                // Recreation subcategories
                new() {
                    CategoryId = recreation.Id,
                    Name = "Travel & Vacations"
                },
                new() {
                    CategoryId = recreation.Id,
                    Name = "Club Memberships (Gym, Sports Clubs, etc.)"
                },
                new() {
                    CategoryId = recreation.Id,
                    Name = "Tickets (Movies, Concerts, Sports Events)"
                },
                new() {
                    CategoryId = recreation.Id,
                    Name = "Sports Equipment & Gear"
                },
                new() {
                    CategoryId = recreation.Id,
                    Name = "Entertainment (Hobbies, Games, etc.)"
                },
                new() {
                    CategoryId = recreation.Id,
                    Name = "Alcohol & Nightlife"
                },
                new() {
                    CategoryId = recreation.Id,
                    Name = "Smoking & Tobacco"
                },

                // Personal Care subcategories
                new() {
                    CategoryId = personalCare.Id,
                    Name = "Hairdresser/Barber"
                },
                new() {
                    CategoryId = personalCare.Id,
                    Name = "Cosmetics & Skincare"
                },
                new() {
                    CategoryId = personalCare.Id,
                    Name = "Spa & Beauty Care"
                },

                // Fees subcategories
                new() {
                    CategoryId = fees.Id,
                    Name = "Bank Fees"
                },
                new() {
                    CategoryId = fees.Id,
                    Name = "Credit Card Fees"
                },
                new() {
                    CategoryId = fees.Id,
                    Name = "Professional Fees (Legal, Accounting, etc.)"
                }
            };

            // Insert the default subcategories into the database
            await db.InsertAllAsync(defaultSubCategories);
        }

        // Load all categories
        private static async Task<ObservableCollection<Category>> LoadCategoriesAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            List<Category> categories = await db.Table<Category>().OrderBy(c => c.Name).ToListAsync();

            return new ObservableCollection<Category>(categories);
        }

        // Load all subcategories for a given category
        private static async Task<ObservableCollection<SubCategory>> LoadSubCategoriesAsync(Category category)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            List<SubCategory> subCategories = await db.Table<SubCategory>().Where(sc => sc.CategoryId == category.Id).OrderBy(sc => sc.Name).ToListAsync();

            return new ObservableCollection<SubCategory>(subCategories);
        }

        // Save or update a single bank account
        public static async Task SaveAccountAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Saving account {account.Id}");

            try
            {
                await db.InsertOrReplaceAsync(account);

                Console.WriteLine($"Account {account.Id} saved successfully.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving account {account.Id}: {ex.Message}\n");
                throw;
            }
        }

        // Load all bank accounts
        public static async Task<ObservableCollection<BankAccount>> LoadAccountsAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine("Loading accounts from database...");

            try
            {
                List<BankAccount> accounts = await db.Table<BankAccount>().ToListAsync();

                Console.WriteLine($"Loaded {accounts.Count} accounts from database.\n");

                return new ObservableCollection<BankAccount>(accounts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading accounts: {ex.Message}\n");
                throw;
            }
        }

        // Delete a bank account and its associated transactions    
        public static async Task DeleteAccountAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Deleting account {account.Id}");

            try
            {
                await db.RunInTransactionAsync(conn =>
                {
                    conn.Execute(
                        "DELETE FROM Transactions WHERE BankAccountId = ?",
                        account.Id);
                });

                await db.DeleteAsync(account);

                Console.WriteLine($"Deleted account {account.Id}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account {account.Id}: {ex.Message}\n");
                throw;
            }
        }

        // Save a collection of transactions
        public static async Task SaveTransactionsAsync(ObservableCollection<Transaction> transactions)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Saving {transactions.Count} transactions...");

            try
            {
                await db.InsertAllAsync(transactions);

                Console.WriteLine($"Saved {transactions.Count} transactions successfully.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving transactions: {ex.Message}\n");
                throw;
            }
        }

        // Load all transactions for a specific bank account
        public static async Task<ObservableCollection<Transaction>> LoadTransactionsAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Loading transactions for account {account.Id}...");

            try
            {
                List<Transaction> transactions = await db.Table<Transaction>()
                    .Where(t => t.BankAccountId == account.Id)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                Console.WriteLine($"Loaded {transactions.Count} transactions for account {account.Id}.\n");

                return new ObservableCollection<Transaction>(transactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions for account {account.Id}: {ex.Message}\n");
                throw;
            }
        }

        // Save an import batch
        public static async Task SaveImportBatchAsync(ImportBatch importBatch)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Saving import batch {importBatch.Id}");

            await db.InsertAsync(importBatch);
        }

        // Load import batches for a specific bank account
        public static async Task<ObservableCollection<ImportBatch>> LoadImportBatchesAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Loading import batches for account {account.Id}");

            try
            {
                List<ImportBatch> importBatches = await db.Table<ImportBatch>()
                    .Where(b => b.BankAccountId == account.Id)
                    .ToListAsync();

                Console.WriteLine($"Loaded {importBatches.Count} import batches for account {account.Id}.");

                return new ObservableCollection<ImportBatch>(importBatches);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading import batches for account {account.Id}: {ex.Message}\n");
                throw;
            }
        }
    }
}
