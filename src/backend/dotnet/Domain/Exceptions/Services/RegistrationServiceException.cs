namespace Eventor.Services.Exceptions;

public class RegistrationServiceException : ServiceException
{
    public RegistrationServiceException(string message, Exception ex) : base(message, ex) { }
}

public class RegistrationNotFoundException : RegistrationServiceException
{
    public RegistrationNotFoundException(string message) : base(message, null!) { }
}

public class RegistrationAlreadyExistsException : RegistrationServiceException
{
    public RegistrationAlreadyExistsException(string message, Exception ex) : base(message, ex) { }
}
