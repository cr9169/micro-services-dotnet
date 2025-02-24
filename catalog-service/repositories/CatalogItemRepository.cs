using Microsoft.EntityFrameworkCore; // Similar to importing Mongoose in Node.js - this gives us tools to work with a database (SQL Server here) via an ORM
using Microsoft.Extensions.Caching.Memory; // Added for IMemoryCache, like a simple in-memory cache module in Node.js (e.g., node-cache)

public class CatalogItemRepository : ICatalogItemRepository
{
    // "_" before "context" is a C# convention for private fields, similar to _variable in JavaScript sometimes
    // readonly means this field can only be set here or in the constructor - prevents accidental changes later
    private readonly CatalogItemsContext _context; // This is the DbContext, like a MongoDB connection in Mongoose, used to access database tables

    /* 
     * ILogger<T> is an interface for logging (like console.log or winston in Node.js, but more powerful).
     * The <CatalogItemRepository> part tags logs as coming from this class, making them easy to filter in log files or tools like Cloudwatch.
     * Example console output:
     * [2025-02-21 14:10:00] INFO  CatalogItemRepository - Fetching all catalog items...
     * [2025-02-21 14:10:05] ERROR CatalogItemRepository - Failed to fetch items from the database.
     */
    private readonly ILogger<CatalogItemRepository> _logger;

    /// <summary>
    /// In-memory cache instance used to store frequently accessed catalog items.
    /// Provides fast access to data, reducing database load and improving response times.
    /// </summary>
    // IMemoryCache is like a simple caching tool in Node.js (e.g., node-cache), storing data in the server's memory for quick retrieval
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the CatalogItemRepository with dependency injection.
    /// </summary>
    /// <param name="context">The database context for accessing catalog items.</param>
    /// <param name="logger">The logger instance for logging operations and errors.</param>
    /// <param name="cache">The in-memory cache instance for storing and retrieving cached data.</param>
    // This constructor uses Dependency Injection (DI), like passing dependencies in Node.js via require and variables
    // In .NET, DI is built-in, and the container automatically injects these objects when the class is created
    public CatalogItemRepository(CatalogItemsContext context, ILogger<CatalogItemRepository> logger, IMemoryCache cache)
    {
        _context = context; // Storing the DbContext for use in methods
        _logger = logger;   // Storing the logger for logging
        _cache = cache;     // Storing the cache for caching operations
    }

    /// <summary>
    /// Retrieves all catalog items, either from the in-memory cache or the database.
    /// Caching is used to improve performance by avoiding repeated database queries for frequently accessed data.
    /// Cache is set with an absolute expiration of 5 minutes and a sliding expiration of 2 minutes.
    /// </summary>
    /// <returns>An enumerable collection of all catalog items.</returns>
    /// <exception cref="CatalogItemRepositoryException">Thrown when an error occurs during database retrieval.</exception>
    public async Task<IEnumerable<CatalogItem>> GetAllAsync()
    {
        // A unique key to identify the full list of items in the cache, like a key in a key-value store in Node.js
        string cacheKey = "all_catalog_items";

        // Checking if the data is already in the cache (similar to cache.get in Node.js)
        // TryGetValue returns true if the key exists and fills the cachedItems variable with the cached data
        if (_cache.TryGetValue(cacheKey, out IEnumerable<CatalogItem> cachedItems))
        {
            // Logging that we're using the cache, like console.log in Node.js
            _logger.LogInformation("Returning all catalog items from cache");
            return cachedItems; // Returning directly from cache, faster than hitting the DB
        }

        // If not in cache, fetch from the database
        try // try-catch is like try-catch in Node.js, catching errors
        {
            // ToListAsync is similar to Model.find() in Mongoose, fetching all records from the CatalogItems table
            // await is like await in Node.js, waiting for the async operation to complete
            var items = await _context.CatalogItems.ToListAsync();

            // Setting cache expiration policies, like ttl (time-to-live) options in Node.js
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) // Cache expires after 5 minutes no matter what
                .SetSlidingExpiration(TimeSpan.FromMinutes(2)); // Cache expires if not accessed for 2 minutes

            // Storing the data in cache, similar to cache.set in Node.js
            _cache.Set(cacheKey, items, cacheOptions);
            _logger.LogInformation("Cached all catalog items for 5 minutes");

            return items; // Returning the data from the DB
        }
        catch (Exception ex) // Catching any error (like Error in Node.js)
        {
            // LogError is like console.error, but with extra details about the error
            _logger.LogError(ex, "Error occurred while retrieving all catalog items...");
            // Custom Exception (like new Error in Node.js) that adds context to the error
            throw new CatalogItemRepositoryException("Failed to retrieve catalog items", ex);
        }
    }

    /// <summary>
    /// Retrieves a catalog item by its ID, either from the in-memory cache or the database.
    /// Caching reduces database load by storing individual items for quick retrieval.
    /// Cache is set with an absolute expiration of 5 minutes and a sliding expiration of 2 minutes.
    /// </summary>
    /// <param name="Id">The ID of the catalog item to retrieve.</param>
    /// <returns>The catalog item with the specified ID.</returns>
    /// <exception cref="CatalogItemNotFoundException">Thrown when the item is not found.</exception>
    /// <exception cref="CatalogItemRepositoryException">Thrown when an error occurs during database retrieval.</exception>
    public async Task<CatalogItem> GetByIdAsync(int Id)
    {
        // Unique key for this specific item, using the Id (like template literals in JS: `catalog_item_${Id}`)
        string cacheKey = $"catalog_item_{Id}";

        // Checking if the item is in the cache
        if (_cache.TryGetValue(cacheKey, out CatalogItem cachedItem))
        {
            _logger.LogInformation("Returning catalog item with ID {Id} from cache", Id);
            return cachedItem; // Quick return from cache
        }

        try
        {
            // FindAsync is like findById in Mongoose, fetching an item by its primary key
            var catalogItem = await _context.CatalogItems.FindAsync(Id);
            // Checking if the item wasn’t found (null), like if (!doc) in Node.js
            if (catalogItem is null)
            {
                _logger.LogWarning("Catalog item with ID {Id} was not found", Id); // Warning is like console.warn
                // Custom Exception, similar to throw new Error("Not found") in Node.js
                throw new CatalogItemNotFoundException(Id);
            }

            // Setting cache expiration for this item
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) // Cache expires after 5 minutes no matter what
                .SetSlidingExpiration(TimeSpan.FromMinutes(2)); // Cache expires if not accessed for 2 minutes

            _cache.Set(cacheKey, catalogItem, cacheOptions);
            _logger.LogInformation("Cached catalog item with ID {Id} for 5 minutes", Id);

            return catalogItem; // Returning from DB
        }
        catch (CatalogItemNotFoundException) // Catching this specific error and passing it up
        {
            throw; // Like throw err in Node.js, continues the error to the caller
        }
        catch (Exception ex) // Catching any other error
        {
            _logger.LogError(ex, "Error occurred while retrieving catalog item with ID {Id}", Id);
            throw new CatalogItemRepositoryException($"Failed to retrieve catalog item with ID {Id}", ex);
        }
    }

    /// <summary>
    /// Creates a new catalog item in the database and invalidates related cache entries.
    /// The "all_catalog_items" cache is cleared to ensure subsequent GetAllAsync calls retrieve updated data.
    /// </summary>
    /// <param name="catalogItem">The catalog item to create.</param>
    /// <returns>The created catalog item.</returns>
    /// <exception cref="CatalogItemRepositoryException">Thrown when an error occurs during creation.</exception>
    public async Task<CatalogItem> CreateAsync(CatalogItem catalogItem)
    {
        try
        {
            // AddAsync is like create in Mongoose, adds the item to the table
            await _context.CatalogItems.AddAsync(catalogItem);
            // SaveChangesAsync is like save in Mongoose, commits changes to the DB
            await _context.SaveChangesAsync();

            // Removing the cache for the full list since it’s changed (like cache.del in Node.js)
            _cache.Remove("all_catalog_items");
            _logger.LogInformation("Invalidated 'all_catalog_items' cache after creating new item");

            return catalogItem; // Returning the created item
        }
        catch (DbUpdateException ex) // Specific Entity Framework error, like a Mongoose ValidationError
        {
            _logger.LogError(ex, "Error occurred while creating catalog item");
            throw new CatalogItemRepositoryException("Failed to create catalog item", ex);
        }
    }

    /// <summary>
    /// Deletes a catalog item by its ID and invalidates related cache entries.
    /// Both the specific item cache and the "all_catalog_items" cache are cleared.
    /// </summary>
    /// <param name="Id">The ID of the catalog item to delete.</param>
    /// <returns>The deleted catalog item.</returns>
    /// <exception cref="CatalogItemNotFoundException">Thrown when the item is not found.</exception>
    /// <exception cref="CatalogItemRepositoryException">Thrown when an error occurs during deletion.</exception>
    public async Task<CatalogItem> DeleteByIdAsync(int Id)
    {
        try
        {
            // Calling GetByIdAsync to find the item (like findById in Node.js before deleting)
            var item = await GetByIdAsync(Id);
            // Remove is like deleteOne in Mongoose, removes the item from the table
            _context.CatalogItems.Remove(item);
            await _context.SaveChangesAsync();

            // Removing cache for this specific item and the full list
            _cache.Remove($"catalog_item_{Id}");
            _cache.Remove("all_catalog_items");
            _logger.LogInformation("Invalidated cache for catalog item with ID {Id} and 'all_catalog_items'", Id);

            return item; // Returning the deleted item
        }
        catch (CatalogItemNotFoundException) // Catching if the item wasn’t found
        {
            throw; // Passing it up
        }
        catch (Exception ex) // Catching any other error (e.g., DB issues)
        {
            _logger.LogError(ex, "Error occurred while deleting catalog item with ID {Id}", Id);
            throw new CatalogItemRepositoryException($"Failed to delete catalog item with ID {Id}", ex);
        }
    }

    /// <summary>
    /// Updates an existing catalog item by its ID and invalidates related cache entries.
    /// Both the specific item cache and the "all_catalog_items" cache are cleared.
    /// </summary>
    /// <param name="id">The ID of the catalog item to update.</param>
    /// <param name="catalogItem">The updated catalog item data.</param>
    /// <returns>The updated catalog item.</returns>
    /// <exception cref="CatalogItemNotFoundException">Thrown when the item is not found.</exception>
    /// <exception cref="CatalogItemRepositoryException">Thrown when an error occurs during update.</exception>
    public async Task<CatalogItem> UpdateByIdAsync(int id, CatalogItem catalogItem)
    {
        try
        {
            // Ensuring the item exists before updating
            var existingItem = await GetByIdAsync(id);
            catalogItem.Id = id; // Making sure the ID stays the same (like _id in Mongoose)

            // Update is like findByIdAndUpdate in Mongoose, updates the item in the table
            _context.CatalogItems.Update(catalogItem);
            await _context.SaveChangesAsync();

            // Removing cache for this item and the full list
            _cache.Remove($"catalog_item_{id}");
            _cache.Remove("all_catalog_items");
            _logger.LogInformation("Invalidated cache for catalog item with ID {Id} and 'all_catalog_items'", id);

            return catalogItem; // Returning the updated item
        }
        catch (CatalogItemNotFoundException)
        {
            throw;
        }
        catch (DbUpdateException ex) // DB update error
        {
            _logger.LogError(ex, "Error occurred while updating catalog item with ID {Id}", id);
            throw new CatalogItemRepositoryException($"Failed to update catalog item with ID {id}", ex);
        }
    }

    /// <summary>
    /// Partially updates a catalog item by its ID and invalidates related cache entries.
    /// Only the provided properties are updated, and both the specific item cache and the "all_catalog_items" cache are cleared.
    /// </summary>
    /// <param name="Id">The ID of the catalog item to patch.</param>
    /// <param name="catalogItemDetails">The DTO containing the properties to update.</param>
    /// <returns>The updated catalog item.</returns>
    /// <exception cref="CatalogItemNotFoundException">Thrown when the item is not found.</exception>
    /// <exception cref="CatalogItemRepositoryException">Thrown when an error occurs during patching.</exception>
    public async Task<CatalogItem> PatchByIdAsync(int Id, CatalogItemPatchDTO catalogItemDetails)
    {
        try
        {
            // Ensuring the item exists
            var existingItem = await GetByIdAsync(Id);

            // Partial update (patch) - like $set in Mongoose, only updating provided fields
            if (catalogItemDetails.Name != null)
                existingItem.Name = catalogItemDetails.Name; // Only changes if a new value is provided
            if (catalogItemDetails.Description != null)
                existingItem.Description = catalogItemDetails.Description;
            if (catalogItemDetails.Price.HasValue) // HasValue checks if there’s a value in a nullable type
                existingItem.Price = catalogItemDetails.Price.Value;

            await _context.SaveChangesAsync();

            // Removing cache for this item and the full list
            _cache.Remove($"catalog_item_{Id}");
            _cache.Remove("all_catalog_items");
            _logger.LogInformation("Invalidated cache for catalog item with ID {Id} and 'all_catalog_items'", Id);

            return existingItem; // Returning the updated item
        }
        catch (CatalogItemNotFoundException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while patching catalog item with ID {Id}", Id);
            throw new CatalogItemRepositoryException($"Failed to patch catalog item with ID {Id}", ex);
        }
    }
}