using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;

namespace Xy.Isdc.IdentityServer.OAuth
{
    public class Config
    {
        /// <summary>
        /// 获取 客户端
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",

                    // 使用客户端凭证授权  没有交互性用户，使用 clientid/secret 实现认证。
                    //AllowedGrantTypes = GrantTypes.ClientCredentials,

                    //使用 用户名密码授权   当你发送令牌到身份 API 端点的时候，你会发现与客户端凭证授权 相比，资源所有者密码授权有一个很小但很重要的区别。访问令牌现在将包含一个 sub 信息，该信息是用户的唯一标识。sub 信息可以在调用 API 后通过检查内容变量来被查看，并且也将被控制台应用程序显示到屏幕上
                    //AllowedGrantTypes = GrantTypes. GrantTypes.ResourceOwnerPassword,

                    //Implicit Flow是指使用OAuth2的Implicit流程获取Id Token和Access Token
                    //AllowedGrantTypes = GrantTypes.Implicit,

                    //Hybrid Flow是指混合Authorization Code Flow（OAuth授权码流程）和Implici Flow
                    //AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    RequireConsent = false, //禁用 consent 页面确认

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // where to redirect to after login
                    RedirectUris           = { "http://localhost:5002/signin-oidc" },
                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    // 客户端有权访问的范围（Scopes）
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    },
                    AllowOfflineAccess = true
                }
            };
        }


        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "My API")
                {
                    UserClaims = new List<string>(){"Role"}
                }
            };
        }
    }
}
