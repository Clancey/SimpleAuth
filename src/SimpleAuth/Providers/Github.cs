using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleAuth.Providers
{
	public class GithubApi : OAuthApi
	{
		public static class AvailableApiScopes
		{

			/// <summary>
			/// Grants read-only access to public information (includes public user profile info, public repository info, and gists)
			/// </summary>
			public const string EmptyScope = null;

			/// <summary>
			/// Grants read/write access to profile info only. Note that this scope includes user:email and user:follow.
			/// </summary>
			public const string User = "user";

			/// <summary>
			/// Grants read access to a user's email addresses.
			/// </summary>
			public const string UserEmail = "user:email";

			/// <summary>
			/// Grants access to follow or unfollow other users.
			/// </summary>
			public const string UserFollow = "user:follow";

			/// <summary>
			/// Grants read/write access to code, commit statuses, collaborators, and deployment statuses for public repositories and organizations. Also required for starring public repositories.
			/// </summary>
			public const string PublicRepos = "public_repo";

			/// <summary>
			/// Grants read/write access to code, commit statuses, repository invitations, collaborators, and deployment statuses for public and private repositories and organizations.
			/// </summary>
			public const string Repo = "repo";

			/// <summary>
			/// Grants access to deployment statuses for public and private repositories. This scope is only necessary to grant other users or services access to deployment statuses, without granting access to the code.
			/// </summary>
			public const string RepoDeployment = "repo_deployment";

			/// <summary>
			/// Grants read/write access to public and private repository commit statuses. This scope is only necessary to grant other users or services access to private repository commit statuses without granting access to the code.
			/// </summary>
			public const string RepoStatus = "repo:status";

			/// <summary>
			/// Grants access to delete adminable repositories.
			/// </summary>
			public const string DeleteRepo = "delete_repo";

			/// <summary>
			/// Grants read access to a user's notifications. repo also provides this access.
			/// </summary>
			public const string Notifications = "notifications";

			/// <summary>
			/// Grants write access to gists.
			/// </summary>
			public const string Gist = "gist";

			/// <summary>
			/// Grants read and ping access to hooks in public or private repositories.
			/// </summary>
			public const string ReadRepoHook = "read:repo_hook";

			/// <summary>
			/// Grants read, write, and ping access to hooks in public or private repositories.
			/// </summary>
			public const string WriteRepoHook = "write:repo_hook";

			/// <summary>
			/// Grants read, write, ping, and delete access to hooks in public or private repositories.
			/// </summary>
			public const string AdminRepoHook = "admin:repo_hook";

			/// <summary>
			/// Grants read, write, ping, and delete access to organization hooks. Note: OAuth tokens will only be able to perform these actions on organization hooks which were created by the OAuth application. Personal access tokens will only be able to perform these actions on organization hooks created by a user.
			/// </summary>
			public const string OrgRepoHook = "admin:org_hook";

			/// <summary>
			/// Read-only access to organization, teams, and membership.
			/// </summary>
			public const string ReadOrg = "read:org";

			/// <summary>
			/// Publicize and unpublicize organization membership.
			/// </summary>
			public const string WriteOrg = "write:org";

			/// <summary>
			/// Fully manage organization, teams, and memberships.
			/// </summary>
			public const string AdminOrg = "admin:org";

			/// <summary>
			/// List and view details for public keys.
			/// </summary>
			public const string ReadPublicKeys = "read:public_key";

			/// <summary>
			/// Create, list, and view details for public keys.
			/// </summary>
			public const string WritePublicKeys = "write:public_key";

			/// <summary>
			/// Fully manage public keys.
			/// </summary>
			public const string AdminPublicKeys = "admin:public_key";

			/// <summary>
			/// List and view details for GPG keys.
			/// </summary>
			public const string ReadGppKey = "read:gpg_key";

			/// <summary>
			/// Create, list, and view details for GPG keys.
			/// </summary>
			public const string WriteGppkey = "write:gpg_key";

			/// <summary>
			/// Fully manage GPG keys.
			/// </summary>
			public const string AdminGppKey = "admin:gpg_key";


		}

		public GithubApi (string identifier, string clientId, string clientSecret, string redirectUrl = "http://localhost", HttpMessageHandler handler = null) :
			base (identifier, clientId, clientSecret, "https://github.com/login/oauth/access_token", "https://github.com/login/oauth/authorize", redirectUrl, handler: handler)
		{
			UserAgent = "Clancey.SimplAuth";
			ScopesRequired = false;
			/// <summary>
			/// https://developer.github.com/v3/
			/// </summary>
			DefaultAccepts = "application/vnd.github.v3+json";
			BaseAddress = new Uri ("https://api.github.com");
		}

		//public override Task PrepareClient (HttpClient client)
		//{
		//	//For some reason GIthub uses Token instead of Bearer.
		//	client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue ("token", CurrentOAuthAccount.Token);
		//	return Task.FromResult (true);
		//}

		[Path ("user")]
		public Task<GitHubUser> GetUser ()
		{
			return Get<GitHubUser> ();
		}

		public class Plan
		{

			[JsonProperty ("name")]
			public string Name { get; set; }

			[JsonProperty ("space")]
			public int Space { get; set; }

			[JsonProperty ("private_repos")]
			public int PrivateRepos { get; set; }

			[JsonProperty ("collaborators")]
			public int Collaborators { get; set; }
		}

		public class GitHubUser
		{

			[JsonProperty ("login")]
			public string Login { get; set; }

			[JsonProperty ("id")]
			public int Id { get; set; }

			[JsonProperty ("avatar_url")]
			public string AvatarUrl { get; set; }

			[JsonProperty ("gravatar_id")]
			public string GravatarId { get; set; }

			[JsonProperty ("url")]
			public string Url { get; set; }

			[JsonProperty ("html_url")]
			public string HtmlUrl { get; set; }

			[JsonProperty ("followers_url")]
			public string FollowersUrl { get; set; }

			[JsonProperty ("following_url")]
			public string FollowingUrl { get; set; }

			[JsonProperty ("gists_url")]
			public string GistsUrl { get; set; }

			[JsonProperty ("starred_url")]
			public string StarredUrl { get; set; }

			[JsonProperty ("subscriptions_url")]
			public string SubscriptionsUrl { get; set; }

			[JsonProperty ("organizations_url")]
			public string OrganizationsUrl { get; set; }

			[JsonProperty ("repos_url")]
			public string ReposUrl { get; set; }

			[JsonProperty ("events_url")]
			public string EventsUrl { get; set; }

			[JsonProperty ("received_events_url")]
			public string ReceivedEventsUrl { get; set; }

			[JsonProperty ("type")]
			public string Type { get; set; }

			[JsonProperty ("site_admin")]
			public bool SiteAdmin { get; set; }

			[JsonProperty ("name")]
			public string Name { get; set; }

			[JsonProperty ("company")]
			public string Company { get; set; }

			[JsonProperty ("blog")]
			public string Blog { get; set; }

			[JsonProperty ("location")]
			public string Location { get; set; }

			[JsonProperty ("email")]
			public string Email { get; set; }

			[JsonProperty ("hireable")]
			public bool? Hireable { get; set; }

			[JsonProperty ("bio")]
			public string Bio { get; set; }

			[JsonProperty ("public_repos")]
			public int PublicRepos { get; set; }

			[JsonProperty ("public_gists")]
			public int PublicGists { get; set; }

			[JsonProperty ("followers")]
			public int Followers { get; set; }

			[JsonProperty ("following")]
			public int Following { get; set; }

			[JsonProperty ("created_at")]
			public DateTime CreatedAt { get; set; }

			[JsonProperty ("updated_at")]
			public DateTime UpdatedAt { get; set; }

			[JsonProperty ("total_private_repos")]
			public int TotalPrivateRepos { get; set; }

			[JsonProperty ("owned_private_repos")]
			public int OwnedPrivateRepos { get; set; }

			[JsonProperty ("private_gists")]
			public int PrivateGists { get; set; }

			[JsonProperty ("disk_usage")]
			public int DiskUsage { get; set; }

			[JsonProperty ("collaborators")]
			public int Collaborators { get; set; }

			[JsonProperty ("plan")]
			public Plan Plan { get; set; }
		}
	}
}

