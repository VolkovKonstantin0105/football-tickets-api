using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Application.Auth
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        }
    }

}
