namespace Eventor.Services.Exceptions;

public class CalculationServiceException : ServiceException
{
    public CalculationServiceException(string message) : base(message) { }
    public CalculationServiceException(string message, Exception ex) : base(message, ex) { }
}

