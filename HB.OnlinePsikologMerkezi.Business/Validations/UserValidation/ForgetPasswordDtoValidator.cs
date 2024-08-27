using FluentValidation;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Validations.UserValidation
{
    public class ForgetPasswordDtoValidator : AbstractValidator<ForgetPasswordDto>
    {
        //[Required(ErrorMessage = "Mutlaka bir email adresi girmelisin")]
        //[EmailAddress(ErrorMessage = "Geçerli bir email adresi girmelisin")]
        public ForgetPasswordDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Mutkala bir e-posta addresi girmelisin");
            RuleFor(x => x.Email).EmailAddress().WithMessage("Geçerli bir email adresi girmelisin");

        }
    }
}
