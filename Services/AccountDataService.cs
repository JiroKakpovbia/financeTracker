using SQLite;
using trackr.Models;

namespace trackr.Services
{
    public class AccountDataService : IAccountDataService
    {
        private SQLiteAsyncConnection? _db;

        // Ensure the database is initialized before any operations
        private async Task EnsureInitializedAsync()
        {
            if (_db is null)
                await InitializeAccountDataAsync();
        }

        // Get the SQLiteAsyncConnection, initializing it if necessary
        private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            await EnsureInitializedAsync();

            return _db ??
                throw new InvalidOperationException(
                    "SQLite connection was not initialized.");
        }

        // Initialize the SQLite database connection and create tables if they don't exist
        private async Task InitializeAccountDataAsync()
        {
            if (_db is not null)
                return;

            Console.WriteLine("Initializing SQLite database connection...");

            string dbPath = Path.Combine(
                FileSystem.Current.AppDataDirectory,
                "trackr.db3");

            _db = new SQLiteAsyncConnection(
                dbPath,
                SQLiteOpenFlags.ReadWrite |
                SQLiteOpenFlags.Create |
                SQLiteOpenFlags.FullMutex);

            await _db.CreateTablesAsync<
                BankAccount,
                Transaction,
                Category,
                SubCategory,
                ImportBatch>();

            // Seed the database with default categories if none exist
            await SeedCategoriesAsync(_db);

            Console.WriteLine($"SQLite database initialized at {dbPath}\n");
        }

        // Seed the database with default categories if none exist
        private async Task SeedCategoriesAsync(SQLiteAsyncConnection db)
        {
            // Retrieve existing categories to check if seeding is necessary
            List<Category> existingCategories = await db.Table<Category>().ToListAsync();

            // If no categories exist, seed the database with default categories
            if (existingCategories.Count == 0)
            {
                Console.WriteLine("Seeding default categories into the database...");

                // Define default categories
                List<Category> defaultCategories =
                [
                    new()
                    {
                        Name = "Income"
                    },
                    new()
                    {
                        Name = "Savings"
                    },
                    new()
                    {
                        Name = "Housing"
                    },
                    new()
                    {
                        Name = "Communications"
                    },
                    new()
                    {
                        Name = "Food"
                    },
                    new()
                    {
                        Name = "Insurance"
                    },
                    new()
                    {
                        Name = "Transportation"
                    },
                    new()
                    {
                        Name = "Education"
                    },
                    new()
                    {
                        Name = "Recreation"
                    },
                    new()
                    {
                        Name = "Personal Care"
                    },
                    new()
                    {
                        Name = "Fees"
                    },
                    new()
                    {
                        Name = "Transfers"
                    }
                ];

                // Insert the default categories into the database
                await db.InsertAllAsync(defaultCategories);

                // Seed subcategories after seeding categories
                await SeedSubCategoriesAsync(db);

                Console.WriteLine("Default categories and subcategories seeded successfully.\n");
            }
        }

        // Seed the database with default subcategories if none exist
        private async Task SeedSubCategoriesAsync(SQLiteAsyncConnection db)
        {
            // Retrieve default categories to associate with subcategories
            Category income = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Income");

            Category savings = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Savings");

            Category housing = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Housing");

            Category communications = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Communications");

            Category food = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Food");

            Category insurance = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Insurance");

            Category transportation = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Transportation");

            Category education = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Education");

            Category recreation = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Recreation");

            Category personalCare = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Personal Care");

            Category fees = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Fees");

            Category transfers = await db
                .Table<Category>()
                .FirstAsync(c => c.Name == "Transfers");

            // Define default subcategories for each category
            List<SubCategory> defaultSubCategories =
            [
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
                },

                // Transfers subcategories
                new() {
                    CategoryId = transfers.Id,
                    Name = "Between Accounts"
                },
                new() {
                    CategoryId = transfers.Id,
                    Name = "E-Transfers"
                },
                new() {
                    CategoryId = transfers.Id,
                    Name = "Wire Transfers"
                }
            ];

            // Insert the default subcategories into the database
            await db.InsertAllAsync(defaultSubCategories);
        }

        public async Task SaveCategoryAsync(Category category)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Saving category {category.Id}");

            await db.InsertOrReplaceAsync(category);

            Console.WriteLine($"Category {category.Id} saved successfully.\n");
        }

        // Load a single category by ID
        public async Task<Category> LoadCategoryAsync(int categoryId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Loading category {categoryId} from database...");

            Category category = await db.Table<Category>().Where(c => c.Id == categoryId).FirstOrDefaultAsync();

            Console.WriteLine($"Loaded {category.Name} category from database.\n");

            return category;
        }

        public async Task SaveSubCategoryAsync(SubCategory subCategory)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Saving subcategory {subCategory.Id}");

            await db.InsertOrReplaceAsync(subCategory);

            Console.WriteLine($"Subcategory {subCategory.Id} saved successfully.\n");
        }

        // Load all subcategories for a given category
        public async Task<SubCategory> LoadSubCategoryAsync(int subCategoryId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Loading subcategory {subCategoryId} from database...");

            SubCategory subCategory = await db.Table<SubCategory>().Where(sc => sc.Id == subCategoryId).FirstOrDefaultAsync();

            Console.WriteLine($"Loaded {subCategory.Name} subcategory from database.\n");

            return subCategory;
        }

        // Save or update a single bank account
        public async Task SaveAccountAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Saving account {account.Id}");

            await db.InsertOrReplaceAsync(account);

            Console.WriteLine($"Account {account.Id} saved successfully.\n");
        }

        // Load all bank accounts
        public async Task<IReadOnlyList<BankAccount>> LoadAccountsAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine("Loading accounts from database...");

            List<BankAccount> accounts = await db.Table<BankAccount>().ToListAsync();

            Console.WriteLine($"Loaded {accounts.Count} accounts from database.\n");

            return [.. accounts];
        }

        // Delete a bank account and its associated transactions    
        public async Task DeleteAccountAsync(Guid accountId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            BankAccount account = await db.Table<BankAccount>().Where(a => a.Id == accountId).FirstOrDefaultAsync();

            Console.WriteLine($"Deleting account {accountId}");

            await db.RunInTransactionAsync(conn =>
            {
                conn.Execute(
                    "DELETE FROM Transactions WHERE BankAccountId = ?",
                    accountId);
            });

            await db.DeleteAsync(account);

            Console.WriteLine($"Deleted account {accountId}\n");
        }

        // Check if a bank account exists (used to check for duplicates before adding a new account)
        public async Task<bool> AccountExistsAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Checking if account {account.Name} exists...");

            BankAccount? existingAccount = await db.Table<BankAccount>()
                .Where(a => a.Name == account.Name).Where(a => a.Institution == account.Institution).Where(a => a.Type == account.Type).FirstOrDefaultAsync();

            bool exists = existingAccount is not null;

            Console.WriteLine($"Account {account.Name} exists: {existingAccount is not null}\n");

            return exists;
        }

        // Save a collection of transactions
        public async Task SaveTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Saving {transactions.Count()} transactions...");

            await db.InsertAllAsync(transactions);

            Console.WriteLine($"Saved {transactions.Count()} transactions successfully.\n");
        }

        // Load all transactions for a specific bank account or all transactions if accountId is null
        public async Task<IReadOnlyList<Transaction>> LoadTransactionsAsync(Guid? accountId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();
            List<Transaction> transactions = [];

            Console.WriteLine($"Loading transactions for account {accountId}...");

            // If accountId is null, load all transactions; otherwise, load transactions for the specified account
            if (accountId is null)
            {
                transactions = await db.Table<Transaction>()
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();
            }
            else
            {
                transactions = await db.Table<Transaction>()
                    .Where(t => t.BankAccountId == accountId)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                Console.WriteLine($"Loaded {transactions.Count} transactions for account {accountId}.\n");
            }

            return [.. transactions];
        }

        // Save an import batch
        public async Task SaveImportBatchAsync(ImportBatch importBatch)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Saving import batch...");

            await db.InsertAsync(importBatch);
        }

        // Load import batches for a specific bank account
        public async Task<IReadOnlyList<ImportBatch>> LoadImportBatchesAsync(Guid accountId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Loading import batches for account {accountId}");

            List<ImportBatch> importBatches = await db.Table<ImportBatch>()
                .Where(b => b.BankAccountId == accountId)
                .ToListAsync();

            Console.WriteLine($"Loaded {importBatches.Count} import batches for account {accountId}.");

            return [.. importBatches];
        }
    }
}
