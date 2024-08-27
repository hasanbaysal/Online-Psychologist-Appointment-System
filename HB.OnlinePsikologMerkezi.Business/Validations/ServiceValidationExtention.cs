using FluentValidation;
using HB.OnlinePsikologMerkezi.Business.Validations.PskValidation;
using HB.OnlinePsikologMerkezi.Business.Validations.UserValidation;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace HB.OnlinePsikologMerkezi.Business.Validations
{
    public static class ServiceValidationExtention
    {
        public static void AddValidationToContainer(IServiceCollection services)
        {

            services.AddTransient<IValidator<UserSignUpDto>, UserSignUpDtoValidator>();
            services.AddTransient<IValidator<UserSigInDto>, UserSignInDtoValidator>();
            services.AddTransient<IValidator<ForgetPasswordDto>, ForgetPasswordDtoValidator>();
            services.AddTransient<IValidator<ResetPasswordDto>, ResetPassworDtoValidator>();
            services.AddTransient<IValidator<PsychologistAddDto>, PskAddValidation>();
            services.AddTransient<IValidator<PsychologistUpdateDto>, PskUpdateValidation>();

        }
    }
}
