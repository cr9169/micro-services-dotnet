public interface ICatalogItemRepository
{
    Task<IEnumerable<CatalogItem>> GetAllAsync();
    Task<CatalogItem> GetByIdAsync(int Id);
    Task<CatalogItem> CreateAsync(CatalogItem catalogItem);
    Task<CatalogItem> DeleteByIdAsync(int Id);
    Task<CatalogItem> UpdateByIdAsync(int Id, CatalogItem catalogItem);
    Task<CatalogItem> PatchByIdAsync(int Id, CatalogItemPatchDTO catalogItemDetails);
}