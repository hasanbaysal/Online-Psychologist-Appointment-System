using Microsoft.AspNetCore.Identity;

namespace HB.OnlinePsikologMerkezi.Business.IdentityCustomize
{
    public class CustomErrorDescriber : IdentityErrorDescriber
    {

        public override IdentityError InvalidUserName(string userName)
        {

            return new IdentityError { Code = "UserName", Description = "kullanıcı özel karakter içemez .!@-(){}; vb" };
        }
        public override IdentityError DefaultError()
        {
            return new IdentityError { Code = "UserName", Description = "beklenmeyen bir hata meydana geldi daha sonra  tekrar deneyiniz" };
        }
        public override IdentityError DuplicateEmail(string email)
        {
            return
                new IdentityError { Code = "Email", Description = "Bu e-posta sistemde kayıtlı" };
        }
        public override IdentityError DuplicateUserName(string userName)
        {
            return
                new IdentityError { Code = "UserName", Description = "Bu isim zaten kayıtlı" };
        }
        public override IdentityError PasswordRequiresDigit()
        {
            return
               new IdentityError { Code = "Password", Description = "Şifrende en az bir tane rakam barındırmalısın " };
        }
        public override IdentityError PasswordTooShort(int length)
        {
            return
                  new IdentityError { Code = "Password", Description = "şifre en az 8 karakterli olmalı" };
        }
        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError { Code = "Password", Description = "şifren en az bir küçük harf içermeli " };
        }
        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError { Code = "Password", Description = "şifren en az bir büyük harf içermeli " };
        }
        //public override IdentityError UserLockoutNotEnabled()
        //{
        //    return 
        //}
    }
}
