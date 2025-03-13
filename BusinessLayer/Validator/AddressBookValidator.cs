using FluentValidation;
using ModelLayer.Model;


// yh validate krne k liye h ki name required hona chahiye , phone number  10 digit hona chahiye and email valid format hona chahiye
public class AddressBookValidator : AbstractValidator<AddressBookDTO>
{
    public AddressBookValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{10}$").WithMessage("Invalid phone number.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.");
    }
}

