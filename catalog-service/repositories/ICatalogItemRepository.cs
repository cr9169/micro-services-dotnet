public interface ICatalogItemRepository
{
    Task<IEnumerable<CatalogItem>> GetAllAsync();
    Task<CatalogItem> GetByIdAsync(Guid Id);
    Task<CatalogItem> CreateAsync(CatalogItem catalogItem);
    Task<CatalogItem> DeleteByIdAsync(Guid Id);
    Task<CatalogItem> UpdateByIdAsync(Guid Id, CatalogItem catalogItem);
    Task<CatalogItem> PatchByIdAsync(Guid Id, CatalogItemPatchDTO catalogItemDetails);
}