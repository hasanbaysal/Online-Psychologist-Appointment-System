using FluentValidation;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Validations.UserValidation
{
    public class UserSignInDtoValidator : AbstractValidator<UserSigInDto>
    {
        public UserSignInDtoValidator()
        {
            RuleFor(x => x.Email).EmailAddress().WithMessage("Geçerli bir email adresi girmelisin").NotEmpty().WithMessage("Giriş yapmak için kim olduğunu bilmeliyim :) ");
            RuleFor(x => x.Password).NotEmpty().WithMessage("bence şifreyi girmelisin :)");


        }
    }
}
