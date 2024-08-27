using HB.OnlinePsikologMerkezi.Common.CustomErros;
using Microsoft.AspNetCore.Identity;

namespace HB.OnlinePsikologMerkezi.Business.Extentions
{
    public static class ValidationHelper
    {
        public static List<CustomValidationError> CustomErrorList(this FluentValidation.Results.ValidationResult validationResult)
        {

            var data = validationResult.Errors.ToList().Select(x =>
            new CustomValidationError
            {
                ErrorMessage = x.ErrorMessage,
                PropertyName = x.PropertyName
            }).ToList();


            return new List<CustomValidationError>(data);
        }
        public static List<CustomValidationError> IdentityErrorList(this IdentityResult error)
        {
            var data = error.Errors.ToList().Select(x =>
           new CustomValidationError
           {
               ErrorMessage = x.Description,
               PropertyName = x.Code
           }).ToList();


            return new List<CustomValidationError>(data);


        }

    }
}
