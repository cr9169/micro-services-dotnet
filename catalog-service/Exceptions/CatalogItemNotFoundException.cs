public class CatalogItemNotFoundException : Exception
{
    public CatalogItemNotFoundException(Guid id)
        : base($"Catalog item with ID {id} was not found...") { }
}