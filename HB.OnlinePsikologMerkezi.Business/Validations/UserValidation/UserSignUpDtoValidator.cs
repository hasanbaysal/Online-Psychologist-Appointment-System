using FluentValidation;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Validations.UserValidation
{
    public class UserSignUpDtoValidator : AbstractValidator<UserSignUpDto>
    {
        public UserSignUpDtoValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("Kullanıcı adı boş bırakılamaz");
            RuleFor(x => x.UserName).MaximumLength(15).WithMessage("Kullanıcı adı en fazla 15 karakter olabilir");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Bir şifre belirlemelisiniz");

            RuleFor(x => x.Password)
           .Must(x => x != null && x.Any(char.IsUpper))
           .WithMessage("Şifren en az bir büyük harf içermeli");

            RuleFor(x => x.Password)
           .Must(x => x != null && x.Any(char.IsLower))
           .WithMessage("Şifren en az bir küçük harf içermeli");

            RuleFor(x => x.Password)
           .Must(x => x != null && x.Any(char.IsDigit))
           .WithMessage("Şifren en az bir adet rakam harf içermeli");

            RuleFor(x => x.Password).MinimumLength(8).WithMessage("şifre minimum 8 karakter olmalı");

            RuleFor(x => x.Email).EmailAddress().WithMessage("Geçerli bir email girmeniz gerekli");


        }

    }
}
