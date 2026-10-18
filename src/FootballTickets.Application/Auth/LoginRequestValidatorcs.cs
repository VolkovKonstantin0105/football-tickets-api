using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Application.Auth
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

}
