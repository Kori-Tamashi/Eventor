namespace Eventor.Services.Exceptions;

public class UserServiceException : ServiceException
{
    public UserServiceException(string message, Exception ex) : base(message, ex) { }
}

public class UserNotFoundException : UserServiceException
{
    public UserNotFoundException(string message) : base(message, null) { }
}

public class UserCreateException : UserServiceException
{
    public UserCreateException(string message, Exception ex) : base(message, ex) { }
}

public class UserUpdateException : UserServiceException
{
    public UserUpdateException(string message, Exception ex) : base(message, ex) { }
}

public class UserDeleteException : UserServiceException
{
    public UserDeleteException(string message, Exception ex) : base(message, ex) { }
}