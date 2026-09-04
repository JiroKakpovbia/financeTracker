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
                    Name = "Partner's Net Income"
                },
                new() {
                    CategoryId = income.Id,
                    Name = "Employment Insurance"
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
                    Name = "Combined Packages"
                },
                new() {
                    CategoryId = communications.Id,
                    Name = "Subscriptions"
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
                    Name = "Life"
                },
                new() {
                    CategoryId = insurance.Id,
                    Name = "Medical/Dental"
                },
                new() {
                    CategoryId = insurance.Id,
                    Name = "Disability/Accident"
                },

                // Transportation subcategories
                new() {
                    CategoryId = transportation.Id,
                    Name = "Car Loan/Lease"
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
                    Name = "Public Transit"
                },
                new() {
                    CategoryId = transportation.Id,
                    Name = "Ride Services"
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
                    Name = "Club Memberships"
                },
                new() {
                    CategoryId = recreation.Id,
                    Name = "Tickets"
                },
                new() {
                    CategoryId = recreation.Id,
                    Name = "Sports Equipment"
                },
                new() {
                    CategoryId = recreation.Id,
                    Name = "Entertainment"
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
                    Name = "Professional Fees"
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

        // Get all categories from the database
        public async Task<IReadOnlyList<Category>> GetAllCategoriesAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting all categories from database...");

            List<Category> categories = await db.Table<Category>()
                .ToListAsync();

            Console.WriteLine($"Got {categories.Count} categories from database.\n");

            return categories;
        }

        // Get a single category by ID
        public async Task<Category> GetCategoryAsync(int categoryId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting category {categoryId} from database...");

            Category category = await db.Table<Category>().Where(c => c.Id == categoryId).FirstAsync();

            Console.WriteLine($"Got {category.Name} category from database.\n");

            return category;
        }

        // Insert a single category
        public async Task InsertCategoryAsync(Category category)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Inserting category {category.Id}");

            await db.InsertAsync(category);

            Console.WriteLine($"Category {category.Id} inserted successfully.\n");
        }

        // Update a single category
        public async Task UpdateCategoryAsync(Category category)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Updating category {category.Id}");

            await db.UpdateAsync(category);

            Console.WriteLine($"Category {category.Id} updated successfully.\n");
        }

        // Get all subcategories
        public async Task<IReadOnlyList<SubCategory>> GetAllSubCategoriesAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting all subcategories from database...");

            List<SubCategory> subCategories = await db.Table<SubCategory>()
                .ToListAsync();

            Console.WriteLine($"Got {subCategories.Count} subcategories from database.\n");

            return subCategories;
        }

        // Get all subcategories for a given category
        public async Task<IReadOnlyList<SubCategory>> GetSubCategoriesForCategoryAsync(int categoryId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting subcategories for category {categoryId} from database...");

            List<SubCategory> subCategories = await db.Table<SubCategory>()
                .Where(sc => sc.CategoryId == categoryId)
                .ToListAsync();

            Console.WriteLine($"Got {subCategories.Count} subcategories for category {categoryId} from database.\n");

            return subCategories;
        }
        
        // Get a single subcategory for a given category
        public async Task<SubCategory> GetSubCategoryAsync(int subCategoryId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting subcategory {subCategoryId} from database...");

            SubCategory subCategory = await db.Table<SubCategory>().Where(sc => sc.Id == subCategoryId).FirstAsync();

            Console.WriteLine($"Got {subCategory.Name} subcategory from database.\n");

            return subCategory;
        }

        // Insert a single subcategory
        public async Task InsertSubCategoryAsync(SubCategory subCategory)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Inserting subcategory {subCategory.Id}");

            await db.InsertAsync(subCategory);

            Console.WriteLine($"Subcategory {subCategory.Id} inserted successfully.\n");
        }

        // Update a single subcategory
        public async Task UpdateSubCategoryAsync(SubCategory subCategory)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Updating subcategory {subCategory.Id}");

            await db.UpdateAsync(subCategory);

            Console.WriteLine($"Subcategory {subCategory.Id} updated successfully.\n");
        }

        // Get all bank accounts
        public async Task<IReadOnlyList<BankAccount>> GetAllAccountsAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting all accounts from database...");

            List<BankAccount> accounts = await db.Table<BankAccount>()
                .ToListAsync();

            Console.WriteLine($"Got {accounts.Count} accounts from database.\n");

            return accounts;
        }

        // Get a single bank account
        public async Task<BankAccount> GetAccountAsync(Guid accountId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting account {accountId} from database...");

            BankAccount account = await db.Table<BankAccount>().Where(a => a.Id == accountId).FirstAsync();

            Console.WriteLine($"Got {account.Name} account from database.\n");

            return account;
        }

        // Insert a single bank account
        public async Task InsertAccountAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Inserting account {account.Id}");

            await db.InsertAsync(account);

            Console.WriteLine($"Account {account.Id} inserted successfully.\n");
        }

        // Update a single bank account
        public async Task UpdateAccountAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Updating account {account.Id}");

            await db.UpdateAsync(account);

            Console.WriteLine($"Account {account.Id} updated successfully.\n");
        }

        // Delete a bank account and its associated transactions    
        public async Task DeleteAccountAsync(Guid accountId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            BankAccount account = await db.Table<BankAccount>().Where(a => a.Id == accountId).FirstAsync();

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

            Console.WriteLine($"Account {account.Name} exists: {exists}\n");

            return exists;
        }

        // Get all transactions
        public async Task<IReadOnlyList<Transaction>> GetAllTransactionsAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting all transactions from database...");

            List<Transaction> transactions = await db.Table<Transaction>()
                .ToListAsync();

            Console.WriteLine($"Got {transactions.Count} transactions from database.\n");

            return transactions;
        }
        
        // Get all transactions for a specific bank account
        public async Task<IReadOnlyList<Transaction>> GetTransactionsForAccountAsync(Guid accountId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting transactions for account {accountId}...");

            List<Transaction> transactions = await db.Table<Transaction>()
                .Where(t => t.BankAccountId == accountId)
                .ToListAsync();

            Console.WriteLine($"Got {transactions.Count} transactions for account {accountId}\n");

            return transactions;
        }

        // Get a single transaction by ID
        public async Task<Transaction?> GetTransactionAsync(int transactionId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting transaction {transactionId}...");

            Transaction? transaction = await db.Table<Transaction>()
                .Where(t => t.Id == transactionId)
                .FirstAsync();

            Console.WriteLine($"Got transaction {transactionId}: {transaction?.Description}\n");

            return transaction;
        }

        // Insert a single transaction
        public async Task InsertTransactionAsync(Transaction transaction)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Inserting transaction {transaction.Description}...");

            await db.InsertAsync(transaction);

            Console.WriteLine($"Transaction {transaction.Description} inserted successfully.\n");
        }

        // Update a single transaction
        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Updating transaction {transaction.Description}...");

            await db.UpdateAsync(transaction);

            Console.WriteLine($"Transaction {transaction.Description} updated successfully.\n");
        }

        // Get all import batches
        public async Task<IReadOnlyList<ImportBatch>> GetAllImportBatchesAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting all import batches from database...");

            List<ImportBatch> importBatches = await db.Table<ImportBatch>()
                .ToListAsync();

            Console.WriteLine($"Got {importBatches.Count} import batches from database.\n");

            return importBatches;
        }
        
        // Get all import batches for a specific bank account
        public async Task<IReadOnlyList<ImportBatch>> GetImportBatchesForAccountAsync(Guid accountId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting import batches for account {accountId}...");

            List<ImportBatch> importBatches = await db.Table<ImportBatch>()
                .Where(b => b.BankAccountId == accountId)
                .ToListAsync();

            Console.WriteLine($"Got {importBatches.Count} import batches for account {accountId}\n");

            return importBatches;
        }

        // Get a specific import batch by ID
        public async Task<ImportBatch?> GetImportBatchAsync(int importBatchId)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Getting import batch {importBatchId}...");

            ImportBatch? importBatch = await db.Table<ImportBatch>()
                .Where(b => b.Id == importBatchId)
                .FirstAsync();

            Console.WriteLine($"Got import batch {importBatchId}: {importBatch?.Id}\n");

            return importBatch;
        }

        // Insert an import batch
        public async Task InsertImportBatchAsync(ImportBatch importBatch)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Inserting import batch...");

            await db.InsertAsync(importBatch);

            Console.WriteLine($"Import batch inserted successfully.\n");
        }
    }
}
