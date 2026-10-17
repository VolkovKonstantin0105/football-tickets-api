namespace FootballTickets.Domain.Enums
{
    public enum UserRole 
    { 
        User = 0, 
        Admin = 1 
    }
    public enum MatchStatus 
    {  
        Scheduled = 0,
        Cancelled = 1,
        Finished = 2 
    }
    public enum OrderStatus 
    { 
        Paid = 0, 
        Cancelled = 1 
    }

}
