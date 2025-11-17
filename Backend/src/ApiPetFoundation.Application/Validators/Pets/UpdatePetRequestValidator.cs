using FluentValidation;
using ApiPetFoundation.Application.DTOs.Pets;

namespace ApiPetFoundation.Application.Validators.Pets
{
    public class UpdatePetRequestValidator : AbstractValidator<UpdatePetRequest>
    {
        public UpdatePetRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(27);
            RuleFor(x => x.Species).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Breed).NotEmpty().MaximumLength(15);
            RuleFor(x => x.Age).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Sex).NotEmpty();
            RuleFor(x => x.Size).NotEmpty();
            RuleFor(x => x.Description).NotEmpty().MaximumLength(250);
            RuleFor(x => x.Status).NotEmpty();
        }
    }
}
