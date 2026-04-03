namespace Eventor.Services.Exceptions;

public class RegistrationServiceException : ServiceException
{
    public RegistrationServiceException(string message, Exception ex) : base(message, ex) { }
}
