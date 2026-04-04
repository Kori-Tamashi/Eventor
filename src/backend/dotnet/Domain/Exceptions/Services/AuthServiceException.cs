namespace Eventor.Services.Exceptions;

public class AuthServiceException : ServiceException
{
    public AuthServiceException(string message) : base(message) { }
    public AuthServiceException(string message, Exception ex) : base(message, ex) { }
}

public class UserLoginAlreadyExistsException : AuthServiceException
{
    public UserLoginAlreadyExistsException(string message) : base(message) { }
}

public class UserLoginNotFoundException : AuthServiceException
{
    public UserLoginNotFoundException(string message, Exception ex) : base(message, ex) { }
}

public class IncorrectPasswordException : AuthServiceException
{
    public IncorrectPasswordException(string message) : base(message) { }
}
