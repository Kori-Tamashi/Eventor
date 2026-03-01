namespace Eventor.Services.Exceptions;

public class FeedbackServiceException : ServiceException
{
    public FeedbackServiceException(string message, Exception ex) : base(message, ex) { }
}

public class FeedbackNotFoundException : FeedbackServiceException
{
    public FeedbackNotFoundException(string message) : base(message, null) { }
}

public class FeedbackCreateException : FeedbackServiceException
{
    public FeedbackCreateException(string message, Exception ex) : base(message, ex) { }
}

public class FeedbackUpdateException : FeedbackServiceException
{
    public FeedbackUpdateException(string message, Exception ex) : base(message, ex) { }
}

public class FeedbackDeleteException : FeedbackServiceException
{
    public FeedbackDeleteException(string message, Exception ex) : base(message, ex) { }
}