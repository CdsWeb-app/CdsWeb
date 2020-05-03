using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace CdsWeb.Identity
{
    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder UseCdsContactStoreAdaptor<TOrganizationService>(this IdentityBuilder builder)
            where TOrganizationService : class, IOrganizationService
            => builder
                .AddCdsContactUserStore<TOrganizationService>();
                //.AddCdsContactRoleStore<TDocumentStore>();

        private static IdentityBuilder AddCdsContactUserStore<TOrganizationService>(this IdentityBuilder builder)
        {
            var userStoreType = typeof(CdsContactUserStore<,>).MakeGenericType(builder.UserType, typeof(TOrganizationService));

            builder.Services.AddScoped(
                typeof(IUserStore<>).MakeGenericType(builder.UserType),
                userStoreType
            );

            return builder;
        }

        //private static IdentityBuilder AddRavenDBRoleStore<TOrganizationService>(this IdentityBuilder builder)
        //{
        //    var roleStoreType = typeof(CdsContactRoleStore<,>).MakeGenericType(builder.RoleType, typeof(TOrganizationService));

        //    builder.Services.AddScoped(
        //        typeof(IRoleStore<>).MakeGenericType(builder.RoleType),
        //        roleStoreType
        //    );

        //    return builder;
        //}
    }
}
