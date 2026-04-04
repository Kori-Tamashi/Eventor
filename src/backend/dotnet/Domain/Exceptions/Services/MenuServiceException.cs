namespace Eventor.Services.Exceptions;

public class MenuServiceException : ServiceException
{
    public MenuServiceException(string message, Exception ex) : base(message, ex) { }
}

public class MenuNotFoundException : MenuServiceException
{
    public MenuNotFoundException(string message) : base(message, null) { }
    public MenuNotFoundException(string message, Exception ex) : base(message, ex) { }
}

public class MenuCreateException : MenuServiceException
{
    public MenuCreateException(string message, Exception ex) : base(message, ex) { }
}

public class MenuUpdateException : MenuServiceException
{
    public MenuUpdateException(string message, Exception ex) : base(message, ex) { }
}

public class MenuDeleteException : MenuServiceException
{
    public MenuDeleteException(string message, Exception ex) : base(message, ex) { }
}