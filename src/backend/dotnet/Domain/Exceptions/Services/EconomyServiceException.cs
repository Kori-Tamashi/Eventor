namespace Eventor.Services.Exceptions;

public class EconomyServiceException : ServiceException
{
    public EconomyServiceException(string message) : base(message) { }
    public EconomyServiceException(string message, Exception ex) : base(message, ex) { }
}
