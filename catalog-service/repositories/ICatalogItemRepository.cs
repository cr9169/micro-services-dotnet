public interface ICatalogItemRepository
{
    Task<IEnumerable<CatalogItem>> GetAllAsync();
    Task<CatalogItem> GetByIdAsync(Guid Id);
    Task<CatalogItem> CreateAsync(CatalogItemCreateDTO catalogItemCreateDto); // שימוש ב-DTO ליצירה
    Task<CatalogItem> DeleteByIdAsync(Guid Id);
    Task<CatalogItem> UpdateByIdAsync(Guid Id, CatalogItemUpdateDTO catalogItemUpdateDto); // DTO לעדכון מלא
    Task<CatalogItem> PatchByIdAsync(Guid Id, CatalogItemPatchDTO catalogItemPatchDto);    // DTO לעדכון חלקי
}
