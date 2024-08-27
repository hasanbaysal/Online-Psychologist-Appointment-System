using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Identity;

namespace HB.OnlinePsikologMerkezi.Business.IdentityCustomize
{
    public class CustomUserValidation : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {

            var errros = new List<IdentityError>();


            if (user.UserName == null || user.UserName == string.Empty)
            {
                errros.Add(new IdentityError() { Code = "UserName", Description = "kullanıcı adı boş olamaz" });
                return Task.FromResult(IdentityResult.Failed(errros.ToArray()));
            }

            if (user.UserName.Length < 5)
            {
                errros.Add(new IdentityError() { Code = "UserName", Description = "kullanıcı adı minimum 5 karakterli olmalı" });
                return Task.FromResult(IdentityResult.Failed(errros.ToArray()));
            }

            if (user.UserName.Length > 1)
            {
                if (user.UserName[0] == ' ' || user.UserName[user.UserName.Length - 1] == ' ')
                {
                    errros.Add(new IdentityError() { Code = "UserName", Description = "Kullanıcı adı boşlukla bitemez ve boşlukla başlayamaz" });
                }
            }

            if (user.UserName.Contains(' '))
            {
                if (user.UserName.Split(' ').Length - 1 >= 2)
                {
                    errros.Add(new IdentityError() { Code = "UserName", Description = "kullanıcı adı çok fazla boşluk içeremez" });
                }
            }



            if (errros.Any())
            {
                return Task.FromResult(IdentityResult.Failed(errros.ToArray()));
            }
            return Task.FromResult(IdentityResult.Success);
            //a---a
            //a------
            //aaa aa aa aa-

        }

    }

    //public class CustomPasswordValidator : IPasswordValidator<AppUser>
    //{
    //    public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
    //    {
    //        //123456aA
    //        //Aa123456

    //        var errors = new List<IdentityError>();
    //        if (password!.ToLower().Contains(user.UserName!))
    //        {
    //            errors.Add(new() { Code = "2", Description = "password cannot contain username" });
    //        }
    //        if (password!.ToLower().Contains("12345"))
    //        {

    //            errors.Add(new() { Code = "Password", Description = "şifre çok basit" });
    //        }

    //        if (errors.Any())
    //        {
    //            return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
    //        }
    //        return Task.FromResult(IdentityResult.Success);
    //    }
    //}
}
