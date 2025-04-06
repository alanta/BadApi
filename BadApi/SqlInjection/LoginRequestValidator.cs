using FluentValidation;

namespace BadApi.SqlInjection;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(u => u.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(u => u.Password).NotEmpty().WithMessage("Password is required.");
    }
}