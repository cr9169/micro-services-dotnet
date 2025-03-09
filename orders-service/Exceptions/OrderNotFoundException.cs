public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid id)
        : base($"Catalog item with ID {id} was not found...") { }
}