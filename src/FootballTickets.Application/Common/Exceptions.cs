namespace FootballTickets.Application.Common;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

public class NotFoundException : AppException 
{ 
    public NotFoundException(string m) : base(m) { } 
}
public class ConflictException : AppException
{ 
    public ConflictException(string m) : base(m) { } 
}
public class ForbiddenException : AppException 
{
    public ForbiddenException(string m) : base(m) { } 
}
public class UnauthorizedException : AppException 
{ 
    public UnauthorizedException(string m) : base(m) { }
}
public class BusinessRuleException : AppException 
{
    public BusinessRuleException(string m) : base(m) { }
}
