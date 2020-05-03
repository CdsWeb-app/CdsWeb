using Microsoft.AspNetCore.Identity;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CdsWeb.Identity
{
    public partial class CdsContactUserStore<TUser, TOrganizationService> : IUserStore<TUser>, IUserLoginStore<TUser>, IUserEmailStore<TUser>
        where TUser : CdsContactIdentityUser, new()
        where TOrganizationService : class, IOrganizationService
    {
        public IdentityErrorDescriber ErrorDescriber { get; }

        private readonly TOrganizationService _orgService;
        private readonly OrganizationServiceContext _context;

        public CdsContactUserStore(TOrganizationService orgService, IdentityErrorDescriber errorDescriber = null)
        {
            ErrorDescriber = errorDescriber;
            _orgService = orgService;
            _context = new OrganizationServiceContext(orgService);
        }

        public Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var contact = new Entity("contact");
            contact.Attributes["firstname"] = user.Id;
            contact.Attributes["lastname"] = user.UserName;
            contact.Attributes["middlename"] = user.NormalizedUserName;

            _orgService.Create(contact);

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var contact = _context.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<string>("firstname") == user.Id);

            if (contact == null)
            {
                return Task.FromResult(IdentityResult.Failed());
            }

            try
            {
                _orgService.Delete("contact", contact.Id);
            }
            catch (Exception ex)
            {
                return Task.FromResult(IdentityResult.Failed());
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public void Dispose()
        {
            _context.Dispose();            
        }

        public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var contact = _context.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<string>("firstname") == userId);

            if (contact == null)
            {
                return Task.FromResult<TUser>(null);
            }
            var user = new TUser()
            {
                Id = contact.GetAttributeValue<string>("firstname"),
                UserName = contact.GetAttributeValue<string>("lastname"),
                NormalizedUserName = contact.GetAttributeValue<string>("middlename")
            };

            return Task.FromResult(user);
        }

        public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // IUserLoginStore
        public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // IUserEmailStore
        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
