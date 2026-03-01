namespace Eventor.Services.Exceptions;

public class ItemServiceException : ServiceException
{
    public ItemServiceException(string message, Exception ex) : base(message, ex) { }
}

public class ItemNotFoundException : ItemServiceException
{
    public ItemNotFoundException(string message) : base(message, null) { }
}

public class ItemCreateException : ItemServiceException
{
    public ItemCreateException(string message, Exception ex) : base(message, ex) { }
}

public class ItemUpdateException : ItemServiceException
{
    public ItemUpdateException(string message, Exception ex) : base(message, ex) { }
}

public class ItemDeleteException : ItemServiceException
{
    public ItemDeleteException(string message, Exception ex) : base(message, ex) { }
}