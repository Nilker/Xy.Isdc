using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;

namespace Xy.Isdc.IdentityServer.OAuth
{
    public class MyClientStore : IClientStore
    {
        readonly Dictionary<string, Client> _clients;

        public MyClientStore()
        {
            _clients = new Dictionary<string, Client>()
            {
                {
                    "mvc",
                    new Client
                    {
                        ClientId = "mvc",
                        ClientName = "MVC Client",
                        AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                        RequireConsent = false,

                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },

                        RedirectUris           = { "http://localhost:5002/signin-oidc" },
                        PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },
                        //AccessTokenLifetime = 3600, //AccessToken过期时间， in seconds (defaults to 3600 seconds / 1 hour)
                        //AuthorizationCodeLifetime = 300,  //设置AuthorizationCode的有效时间，in seconds (defaults to 300 seconds / 5 minutes)
                        //AbsoluteRefreshTokenLifetime = 2592000,  //RefreshToken的最大过期时间，in seconds. Defaults to 2592000 seconds / 30 day
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "api1"
                        },
                        AllowOfflineAccess = true
                    }
                }
            };
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            Client client;
            _clients.TryGetValue(clientId, out client);
            return Task.FromResult(client);
        }
    }
}
