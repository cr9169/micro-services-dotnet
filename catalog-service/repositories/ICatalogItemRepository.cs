public interface ICatalogItemRepository
{
    Task<IEnumerable<CatalogItem>> GetAllAsync();
    Task<CatalogItem> GetByIdAsync(Guid id);
    Task<CatalogItem> CreateAsync(CatalogItemCreateDTO catalogItemCreateDto); // שימוש ב-DTO ליצירה
    Task<CatalogItem> DeleteByIdAsync(Guid id);
    Task<CatalogItem> UpdateByIdAsync(Guid id, CatalogItemUpdateDTO catalogItemUpdateDto); // DTO לעדכון מלא
    Task<CatalogItem> PatchByIdAsync(Guid id, CatalogItemPatchDTO catalogItemPatchDto);    // DTO לעדכון חלקי
}
