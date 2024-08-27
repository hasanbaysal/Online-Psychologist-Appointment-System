using FluentValidation;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Validations.UserValidation
{
    public class ResetPassworDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPassworDtoValidator()
        {
            RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre boş olamaz");
            RuleFor(x => x.RepeatPassword).NotEmpty().WithMessage("Şifre boş olamaz");

            RuleFor(x => x.RepeatPassword).Equal(x => x.RepeatPassword).WithMessage("girdiğiniz şifreler uyuşmuyor");

        }
    }
}
