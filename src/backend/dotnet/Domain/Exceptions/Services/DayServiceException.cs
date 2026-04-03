using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventor.Services.Exceptions;

public class DayServiceException : ServiceException
{
    public DayServiceException(string message) : base(message) { }
    public DayServiceException(string message, Exception ex) : base(message, ex) { }
}

public class DayNotFoundException : DayServiceException
{
    public DayNotFoundException(string message) : base(message) { }
    public DayNotFoundException(string message, Exception ex) : base(message, ex) { }
}

public class DayCreateException : DayServiceException
{
    public DayCreateException(string message, Exception ex) : base(message, ex) { }
}

public class DayUpdateException : DayServiceException
{
    public DayUpdateException(string message, Exception ex) : base(message, ex) { }
}

public class DayConflictException : DayServiceException
{
    public DayConflictException(string message, Exception ex) : base(message, ex) { }
}

public class DayDeleteException : DayServiceException
{
    public DayDeleteException(string message, Exception ex) : base(message, ex) { }
}