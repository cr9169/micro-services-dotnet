public class CatalogItemNotFoundException : Exception
{
    public CatalogItemNotFoundException(int id)
        : base($"Catalog item with ID {id} was not found...") { }
}