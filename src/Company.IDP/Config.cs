using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Company.IDP
{
    public static class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "8249",
                    Username = "Garrett",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Garrett"),
                        new Claim("family_name", "McTear"),
                    }
                },
                new TestUser
                {
                    SubjectId = "7484",
                    Username = "Leo",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Leo"),
                        new Claim("family_name", "Pants"),
                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>();
        }
    }
}
