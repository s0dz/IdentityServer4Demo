using System.Collections.Generic;
using System.Linq;

namespace Company.IDP.Entities
{
    public static class UserContextExtensions
    {
        // Add demo users if there aren't any users yet
        public static void EnsureSeedDataForContext(this UserContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            var users = new List<User>
            {
                new User
                {
                    SubjectId = "8249",
                    Username = "Garrett",
                    Password = "password",
                    Claims = new List<UserClaim>
                    {
                        new UserClaim("given_name", "Garrett"),
                        new UserClaim("family_name", "McTear"),
                        new UserClaim("address", "1973 S 900 E"),
                        new UserClaim("role", "FreeUser"),
                        new UserClaim("subscriptionlevel", "FreeUser"),
                        new UserClaim("country", "usa")
                    }
                },
                new User
                {
                    SubjectId = "7484",
                    Username = "Leo",
                    Password = "password",
                    Claims = new List<UserClaim>
                    {
                        new UserClaim("given_name", "Leo"),
                        new UserClaim("family_name", "Pants"),
                        new UserClaim("address", "17 Bark Street"),
                        new UserClaim("role", "PayingUser"),
                        new UserClaim("subscriptionlevel", "PayingUser"),
                        new UserClaim("country", "canada")
                    }
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}
