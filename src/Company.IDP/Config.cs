using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4;
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
                        new Claim("address", "1973 S 900 E"),
                        new Claim("role", "FreeUser")
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
                        new Claim("address", "17 Bark Street"),
                        new Claim("role", "PayingUser")
                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource("roles", "Your Role(s)", new List<string>{"role"})
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RedirectUris = new List<string>{ "https://localhost:44361/signin-oidc" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles"
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    PostLogoutRedirectUris = { "https://localhost:44361/signout-callback-oidc" }
                    // AlwaysIncludeUserClaimsInIdToken = true // This is not IdentityServer's default behavior because of security concerns
                }
            };
        }
    }
}
