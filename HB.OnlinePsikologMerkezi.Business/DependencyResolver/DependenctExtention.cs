using HB.OnlinePsikologMerkezi.Business.Helpers;
using HB.OnlinePsikologMerkezi.Business.IdentityCustomize;
using HB.OnlinePsikologMerkezi.Business.IdentityCustomize.EmailTokenProvider;
using HB.OnlinePsikologMerkezi.Business.Managers;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Business.Validations;
using HB.OnlinePsikologMerkezi.Common.Mail;
using HB.OnlinePsikologMerkezi.Data.Context;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Data.UnitOfWork;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HB.OnlinePsikologMerkezi.Business.DependencyResolver
{
    public static class DependenctExtention
    {
        public static void AddBusinessDependencies(this IServiceCollection services, Action<ConnectionStringInfo> connection, Assembly assembly)
        {

            #region ef core dependency

            ConnectionStringInfo info = new();
            connection(info);


            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(info.ConStr);

            });


            #endregion

            #region LİnkHepler

            services.AddHttpContextAccessor();
            services.AddScoped<ILinkHelper, LinkHelper>();

            #endregion
            #region MailService

            services.AddScoped<IMailService, MailManager>();

            #endregion
            #region ValidationRegister

            ServiceValidationExtention.AddValidationToContainer(services);


            #endregion
            #region auto mapper resgister

            services.AddAutoMapper(Assembly.GetExecutingAssembly(), assembly);

            #endregion

            services.AddScoped<IUow, Uow>();


            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.User.AllowedUserNameCharacters =
                "abcçdefgğhiıjklmnoöprsştuüvyz" +
                "ABCÇDEFGĞHİIJKLMNOÖPRSŞTUÜVYZ" +
                "QWX" + "wqx" + "1234567890" + " ";



                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
                options.Lockout.MaxFailedAccessAttempts = 6;

                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;

                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmationTokenProvider";


            }).AddErrorDescriber<CustomErrorDescriber>()
               .AddTokenProvider<CustomEmailConfirmationTokenProvider<AppUser>>("CustomEmailConfirmationTokenProvider")
               .AddDefaultTokenProviders()
               .AddUserValidator<CustomUserValidation>()
               .AddEntityFrameworkStores<AppDbContext>();

            services.Configure<SecurityStampValidatorOptions>(option =>
            {
                option.ValidationInterval = TimeSpan.FromMinutes(100);
            });

            services.Configure<DataProtectionTokenProviderOptions>(option =>
            {
                option.TokenLifespan = TimeSpan.FromHours(3);
            });

            services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromDays(7);
            });


            #region Business Services Register

            services.AddScoped<IAuthService, AuthManager>();
            services.AddScoped<IAdminService, AdminManager>();
            services.AddScoped<IPsychologistService, PsychologistManager>();
            services.AddScoped<IPaymentService, PaymentManager>();
            services.AddScoped<ICategoryService, CategoryManager>();
            services.AddScoped<IBlogService, BlogManager>();

            #endregion





        }
    }
}
