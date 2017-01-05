using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimpleAuth.Providers
{
    public class DropBoxApi : OAuthApi
    {
        public DropBoxApi(string identifier,string client_id, string client_secret, HttpMessageHandler handler = null)
            : base(identifier, client_id, client_secret, handler)
        {
            this.ScopesRequired = false;
            this.TokenUrl = "https://api.dropbox.com/1/oauth2/token";
        }

        protected override WebAuthenticator CreateAuthenticator()
        {
            return new DropBoxAuthenticator
            {
                ClientId = ClientId,
            };
        }

        protected override Task<OAuthAccount> GetAccountFromAuthCode(WebAuthenticator authenticator, string identifier)
        {
            var author = (DropBoxAuthenticator)authenticator;
            var account = new OAuthAccount()
            {
                ExpiresIn = -1,
                Created = DateTime.UtcNow,
                RefreshToken = null,
                Scope = new string[0],
                TokenType = author.TokenType,
                Token = author.Token,
                ClientId = ClientId,
                Identifier = identifier,
            };
            account.UserData["Uid"] = author.Uid;
            return Task.FromResult(account);
        }
    }

    public class DropBoxAuthenticator : OAuthAuthenticator
    {
        public DropBoxAuthenticator()
        {
            BaseUrl = "https://www.dropbox.com/1/oauth2/authorize";
            RedirectUrl = new Uri("http://localhost");
        }

        ///Up to 500 bytes of arbitrary data that will be passed back to <paramref name="redirectUri"/>
        public string State { get; set; }

        public override bool CheckUrl(Uri url, Cookie[] cookies)
        {
            try
            {
                if (url == null)
                    return false;
                if (url.Host != RedirectUrl.Host || url.Fragment == null || url.Fragment.Length < 1)
                    return false;
                var parts = HttpUtility.ParseQueryString(url.Fragment.Substring(1));
                var code = parts["access_token"];
                if (!string.IsNullOrWhiteSpace(code) && tokenTask != null)
                {
                    Token = code;
                    TokenType = parts["token_type"];
                    Uid = parts["uid"];
                    FoundAuthCode(code);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return false;
        }
        public string Token { get; private set; }
        public string TokenType { get; private set; }
        public string Uid { get; private set; }

        public override async Task<Uri> GetInitialUrl()
        {
            var result = await base.GetInitialUrl();
            return result;
        }

        public override Dictionary<string, string> GetInitialUrlQueryParameters()
        {
            var result = new Dictionary<string, string>()
            {
                {"client_id", ClientId},
                {"response_type","token"},
                {"redirect_uri",RedirectUrl.AbsoluteUri},
            };
            if (string.IsNullOrEmpty(State))
                result["state"] = State;
            return result;
        }
    }
}
