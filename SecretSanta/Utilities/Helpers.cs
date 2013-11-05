using System;
using System.Linq;
using System.Security.Principal;
using SecretSanta.Models;

namespace SecretSanta.Utilities
{
    public static class Helpers
    {
        public static Account GetAccount(this IPrincipal user)
        {
            if (!user.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(user.Identity.Name))
                return null;

            return DataRepository.GetAll<Account>()
                .SingleOrDefault(a => a.Email.Equals(user.Identity.Name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}