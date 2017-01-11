using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace SimpleAuth.Tests
{
	[TestFixture ()]
	public class OAuthApiTests
	{
		[SetUp]
		public void Setup ()
		{
			Resolver.Register<IAuthStorage,InMemoryAuthStorage > ();
			var storage = Resolver.GetObject<IAuthStorage> () as InMemoryAuthStorage;
			storage.Reset ();
		}

		[Test]
		public async Task SigningIntoApiWorks ()
		{
			var api = new OAuthTestApi (OAuthData.SignInDataWithRefresh);
			var account = await api.Authenticate ();
			Assert.IsTrue (account.IsValid ());
			//Make sure Show authenticator was called
			Assert.IsTrue (api.CurrentShowAuthenticatorCallCount == 1);
		}

		[Test]
		public async Task TokensExpireAfterAuthentication ()
		{
			var api = new OAuthTestApi (OAuthData.SignInDataWithRefresh);
			var account = await api.Authenticate ();
			Assert.IsTrue (account.IsValid ());
			await Task.Delay (6000);
			Assert.IsFalse (account.IsValid ());
		}


		[Test]
		public async Task AuthenticatedApiRequestsRefreshTokens ()
		{
			var api = new OAuthTestApi (OAuthData.SignInDataWithRefresh);
			var account = await api.Authenticate ();
			Assert.IsTrue (account.IsValid ());
			await Task.Delay (6000);
			Assert.IsFalse (account.IsValid ());

			await api.Get ("test");

			Assert.IsTrue (api.CurrentAccount.IsValid ());
		}



		[Test]
		public async Task NonAuthenticatedCallsDoNotRefreshAccounts ()
		{
			var api = new OAuthTestApi (OAuthData.SignInDataWithRefresh);
			var account = await api.Authenticate ();
			Assert.IsTrue (account.IsValid ());
			await Task.Delay (6000);
			Assert.IsFalse (account.IsValid ());


			await api.Authenticate ();

			Assert.IsTrue (api.CurrentAccount.IsValid ());

			await Task.Delay (6000);
			Assert.IsFalse (api.CurrentAccount.IsValid ());


			await api.Get ("test", authenticated: false);

			Assert.IsFalse (api.CurrentAccount.IsValid ());
		}

		[Test]
		public async Task ApiRefreshesTokensOnAuthenticate ()
		{

			var api = new OAuthTestApi (OAuthData.SignInDataWithRefresh);
			var account = await api.Authenticate ();
			Assert.IsTrue (account.IsValid ());

			await Task.Delay (6000);
			Assert.IsFalse (account.IsValid ());

			await api.Authenticate ();
			Assert.IsTrue (api.CurrentAccount.IsValid ());

			Assert.IsTrue (api.CurrentShowAuthenticatorCallCount == 1);
		}

		[Test]
		public async Task MultipleRefreshesWork ()
		{
			var api = new OAuthTestApi (OAuthData.SignInDataWithRefresh);
			var account = await api.Authenticate ();
			Assert.IsTrue (account.IsValid ());
			await Task.Delay (6000);
			Assert.IsFalse (account.IsValid ());


			await api.Authenticate ();

			Assert.IsTrue (api.CurrentAccount.IsValid ());

			await Task.Delay (6000);
			Assert.IsFalse (api.CurrentAccount.IsValid ());


			await api.Get ("test");

			Assert.IsTrue (api.CurrentAccount.IsValid ());

			//Make sure Show authenticator was called
			Assert.IsTrue (api.CurrentShowAuthenticatorCallCount == 1);
		}


		[Test]
		public async Task FailedRefreshRepromptsShowAuthenticator ()
		{

			var api = new OAuthTestApi (OAuthData.SignInDataFailedRefresh);

			var account = await api.Authenticate ();
			Assert.IsTrue (account.IsValid ());

			await Task.Delay (6000);
			Assert.IsFalse (account.IsValid ());

			await api.Authenticate ();
			Assert.IsTrue (api.CurrentAccount.IsValid ());

			Assert.IsTrue (api.CurrentShowAuthenticatorCallCount == 2);
		}

		[Test]
		public async Task LoadingApiRestoresTokens ()
		{

			var api = new OAuthTestApi (OAuthData.SignInDataWithRefresh);
			var account = await api.Authenticate ();
			Assert.IsTrue (account.IsValid ());

			Assert.IsTrue (api.CurrentShowAuthenticatorCallCount == 1);

			//Let tokens expire
			await Task.Delay (6000);
			Assert.IsFalse (account.IsValid ());

			var api2 = new OAuthTestApi (OAuthData.SignInDataWithRefresh);
			var account2 = await api.Authenticate ();
			Assert.IsTrue (account2.IsValid ());

			Assert.IsTrue (api2.CurrentShowAuthenticatorCallCount == 0);
		}

		[Test]
		public async Task AuthenticatedCallsSendAuthHeader ()
		{

			var api = new OAuthTestApi (OAuthData.SignInDataWithRefresh);
			var account = await api.Authenticate ();
			Assert.IsTrue (account.IsValid ());
			await api.Get ("authenticated");
		}


		[Test]
		public async Task RefreshTokenOnUnauthorized ()
		{

			var api = new OAuthTestApi (OAuthData.SignInBadAccessTokenRefresh);
			var account = await api.Authenticate ();
			Assert.IsTrue ((api.CurrentAccount as OAuthAccount).Token == "badAccessToken");
			Assert.IsTrue (account.IsValid ());
			await api.Get ("authenticated");
			Assert.IsTrue ((api.CurrentAccount as OAuthAccount).Token == "accessToken");
		}
	}
}
