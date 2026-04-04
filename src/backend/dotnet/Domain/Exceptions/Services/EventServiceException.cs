namespace Eventor.Services.Exceptions;

public class EventServiceException : ServiceException
{
    public EventServiceException(string message, Exception ex) : base(message, ex) { }
}

public class EventNotFoundException : EventServiceException
{
    public EventNotFoundException(string message) : base(message, null) { }
    public EventNotFoundException(string message, Exception ex) : base(message, ex) { }
}

public class EventCreateException : EventServiceException
{
    public EventCreateException(string message, Exception ex) : base(message, ex) { }
}

public class EventUpdateException : EventServiceException
{
    public EventUpdateException(string message, Exception ex) : base(message, ex) { }
}

public class EventDeleteException : EventServiceException
{
    public EventDeleteException(string message, Exception ex) : base(message, ex) { }
}