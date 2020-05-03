using Microsoft.AspNetCore.Identity;
using System;

namespace CdsWeb.Identity
{
    public class CdsContactIdentityUser : IdentityUser<string>
    {
        public CdsContactIdentityUser()
        {
            Id = Guid.NewGuid().ToString();
        }
        public CdsContactIdentityUser(string userName) : this()
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        }
    }
}
