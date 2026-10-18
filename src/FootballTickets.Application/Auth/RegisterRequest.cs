using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Application.Auth
{
    public record RegisterRequest(string Email, string Password, string FullName);
}
