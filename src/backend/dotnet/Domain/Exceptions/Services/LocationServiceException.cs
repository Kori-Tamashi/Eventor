namespace Eventor.Services.Exceptions;

public class LocationServiceException : ServiceException
{
    public LocationServiceException(string message, Exception ex) : base(message, ex) { }
}

public class LocationNotFoundException : LocationServiceException
{
    public LocationNotFoundException(string message) : base(message, null) { }
}

public class LocationCreateException : LocationServiceException
{
    public LocationCreateException(string message, Exception ex) : base(message, ex) { }
}

public class LocationUpdateException : LocationServiceException
{
    public LocationUpdateException(string message, Exception ex) : base(message, ex) { }
}

public class LocationDeleteException : LocationServiceException
{
    public LocationDeleteException(string message, Exception ex) : base(message, ex) { }
}