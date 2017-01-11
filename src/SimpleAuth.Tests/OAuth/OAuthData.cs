using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace SimpleAuth.Tests
{
	public static class OAuthData
	{
		public static Dictionary<RequestMessage, RequestResponse> SignInDataWithRefresh = new Dictionary<RequestMessage, RequestResponse> {
				{
					new RequestMessage{
						Url = "https://localhost/o/oauth2/token",
						Method = HttpMethod.Post,
						Content = "grant_type=authorization_code&code=authtoken&client_id=clientId&client_secret=clientSecret&redirect_uri=http%3A%2F%2Flocalhost%2F",

					},
					new RequestResponse{
						ContentType = "application/json",
						StatusCode = HttpStatusCode.OK,
						Content = new OauthResponse {
							AccessToken = "accessToken",
							ExpiresIn = 5,
							RefreshToken = "refreshToken",
							TokenType = "Bearer",
						}.ToJson(),
					}
				},
				{
					new RequestMessage{
						Url = "https://localhost/o/oauth2/token",
						Method = HttpMethod.Post,
						Content = "grant_type=refresh_token&refresh_token=refreshToken&client_id=clientId&client_secret=clientSecret"
					},
					new RequestResponse{
						ContentType = "application/json",
						StatusCode = HttpStatusCode.OK,
						Content = new OauthResponse {
							AccessToken = "accessToken",
							ExpiresIn = 5,
							RefreshToken = "refreshToken",
							TokenType = "Bearer",
						}.ToJson()
					}
				},
				{
					new RequestMessage{
						Url = "https://localhost/test",
						Method = HttpMethod.Get,
					},
					new RequestResponse{
						ContentType = "application/text",
						StatusCode = HttpStatusCode.OK,
						Content = "success"
					}
				},
				{
					new RequestMessage{
						Url = "https://localhost/authenticated",
						Method = HttpMethod.Get,
					},
					new RequestResponse{
						ContentType = "application/text",
						StatusCode = HttpStatusCode.OK,
						RequiredAuthToken = "accessToken",
						Content = "success"
					}
				}
			};

		public static Dictionary<RequestMessage, RequestResponse> SignInDataFailedRefresh = new Dictionary<RequestMessage, RequestResponse> {
				{
					new RequestMessage{
						Url = "https://localhost/o/oauth2/token",
						Method = HttpMethod.Post,
						Content = "grant_type=authorization_code&code=authtoken&client_id=clientId&client_secret=clientSecret&redirect_uri=http%3A%2F%2Flocalhost%2F",

					},
					new RequestResponse{
						ContentType = "application/json",
						StatusCode = HttpStatusCode.OK,
						Content = new OauthResponse {
							AccessToken = "accessToken",
							ExpiresIn = 5,
							RefreshToken = "refreshToken",
							TokenType = "Bearer",
						}.ToJson(),
					}
				},
				{
					new RequestMessage{
						Url = "https://localhost/test",
						Method = HttpMethod.Get,
					},
					new RequestResponse{
						ContentType = "application/text",
						StatusCode = HttpStatusCode.OK,
						Content = "success"
					}
				}
			};

		public static Dictionary<RequestMessage, RequestResponse> SignInBadAccessTokenRefresh = new Dictionary<RequestMessage, RequestResponse> {
				{
					new RequestMessage{
						Url = "https://localhost/o/oauth2/token",
						Method = HttpMethod.Post,
						Content = "grant_type=authorization_code&code=authtoken&client_id=clientId&client_secret=clientSecret&redirect_uri=http%3A%2F%2Flocalhost%2F",

					},
					new RequestResponse{
						ContentType = "application/json",
						StatusCode = HttpStatusCode.OK,
						Content = new OauthResponse {
							AccessToken = "badAccessToken",
							ExpiresIn = 5,
							RefreshToken = "refreshToken",
							TokenType = "Bearer",
						}.ToJson(),
					}
				},
				{
					new RequestMessage{
						Url = "https://localhost/o/oauth2/token",
						Method = HttpMethod.Post,
						Content = "grant_type=refresh_token&refresh_token=refreshToken&client_id=clientId&client_secret=clientSecret"
					},
					new RequestResponse{
						ContentType = "application/json",
						StatusCode = HttpStatusCode.OK,
						Content = new OauthResponse {
							AccessToken = "accessToken",
							ExpiresIn = 5,
							RefreshToken = "refreshToken",
							TokenType = "Bearer",
						}.ToJson()
					}
				},
				{
					new RequestMessage{
						Url = "https://localhost/authenticated",
						Method = HttpMethod.Get,
					},
					new RequestResponse{
						ContentType = "application/text",
						StatusCode = HttpStatusCode.OK,
						RequiredAuthToken = "accessToken",
						Content = "success"
					}
				}
			};

	}
}
