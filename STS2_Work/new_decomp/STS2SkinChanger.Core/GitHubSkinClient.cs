using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace STS2SkinChanger.Core;

internal sealed class GitHubSkinClient : IDisposable
{
	private const int MaxJsonBytes = 4194304;

	private const long MaxDownloadBytes = 134217728L;

	private const int MaxRedirects = 5;

	private readonly HttpClient _client;

	private readonly string _userAgent;

	public GitHubSkinClient(HttpMessageHandler handler, string userAgent)
	{
		_client = new HttpClient(handler, disposeHandler: true)
		{
			Timeout = TimeSpan.FromSeconds(60L)
		};
		_userAgent = userAgent;
	}

	public Task<string> SearchPageAsync(int page, CancellationToken token)
	{
		return GetJsonAsync(GitHubSkinPolicy.SearchUri(page), token);
	}

	public async Task<string?> LatestReleaseAsync(GitHubRepoRef repo, CancellationToken token)
	{
		using HttpResponseMessage response = await SendAsync(Request(GitHubSkinPolicy.LatestReleaseUri(repo)), token, allowNotFound: true);
		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			return null;
		}
		return await ReadTextAsync(response, token);
	}

	public async Task<string?> InfoFileAsync(GitHubRepoRef repo, CancellationToken token)
	{
		using HttpResponseMessage response = await SendAsync(Request(GitHubSkinPolicy.InfoUri(repo), "text/plain"), token, allowNotFound: true);
		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			return null;
		}
		return await ReadTextAsync(response, token, 65536);
	}

	private async Task<string> GetJsonAsync(Uri uri, CancellationToken token)
	{
		using HttpResponseMessage response = await SendAsync(Request(uri), token);
		return await ReadTextAsync(response, token);
	}

	public async Task<string?> ReadmeAsync(GitHubRepoRef repo, CancellationToken token)
	{
		using HttpResponseMessage response = await SendAsync(Request(GitHubSkinPolicy.ReadmeUri(repo), "application/vnd.github.raw"), token, allowNotFound: true);
		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			return null;
		}
		return await ReadTextAsync(response, token);
	}

	private HttpRequestMessage Request(Uri uri, string accept = "application/vnd.github+json")
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
		httpRequestMessage.Headers.UserAgent.ParseAdd(_userAgent);
		httpRequestMessage.Headers.Accept.ParseAdd(accept);
		return httpRequestMessage;
	}

	private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token, bool allowNotFound = false)
	{
		if (request.RequestUri == null || !GitHubSkinPolicy.IsAllowedHost(request.RequestUri.Host))
		{
			throw new InvalidOperationException("Refused a request outside the GitHub allowlist: " + request.RequestUri);
		}
		HttpResponseMessage httpResponseMessage = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
		HttpStatusCode statusCode = httpResponseMessage.StatusCode;
		if ((statusCode == HttpStatusCode.Forbidden || statusCode == HttpStatusCode.TooManyRequests) ? true : false)
		{
			httpResponseMessage.Dispose();
			throw new GitHubRateLimitedException("GitHub API rate limit reached.");
		}
		if (allowNotFound && httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
		{
			return httpResponseMessage;
		}
		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			int statusCode2 = (int)httpResponseMessage.StatusCode;
			httpResponseMessage.Dispose();
			throw new HttpRequestException("GitHub request failed: " + statusCode2);
		}
		return httpResponseMessage;
	}

	private static async Task<string> ReadTextAsync(HttpResponseMessage response, CancellationToken token, int maxBytes = 4194304)
	{
		if (response.Content.Headers.ContentLength > maxBytes)
		{
			throw new InvalidDataException("GitHub response too large.");
		}
		string obj = await response.Content.ReadAsStringAsync(token);
		if (obj.Length > maxBytes)
		{
			throw new InvalidDataException("GitHub response too large.");
		}
		return obj;
	}

	public async Task<long> DownloadAsync(Uri uri, string destination, CancellationToken token)
	{
		if (uri.Scheme != Uri.UriSchemeHttps || !GitHubSkinPolicy.IsAllowedHost(uri.Host))
		{
			throw new InvalidOperationException("Refused a download outside the GitHub allowlist: " + uri);
		}
		Uri current = uri;
		for (int hop = 0; hop <= 5; hop++)
		{
			using HttpRequestMessage request = Request(current);
			using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
			HttpStatusCode statusCode = response.StatusCode;
			if (((uint)(statusCode - 301) <= 2u || statusCode == HttpStatusCode.TemporaryRedirect) ? true : false)
			{
				Uri uri2 = response.Headers.Location ?? throw new HttpRequestException("GitHub redirect without a target.");
				current = (uri2.IsAbsoluteUri ? uri2 : new Uri(current, uri2));
				if (current.Scheme != Uri.UriSchemeHttps || !GitHubSkinPolicy.IsAllowedHost(current.Host))
				{
					throw new InvalidOperationException("Refused a redirect outside the GitHub allowlist: " + current);
				}
				continue;
			}
			statusCode = response.StatusCode;
			if ((statusCode == HttpStatusCode.Forbidden || statusCode == HttpStatusCode.TooManyRequests) ? true : false)
			{
				throw new GitHubRateLimitedException("GitHub download limit reached.");
			}
			if (!response.IsSuccessStatusCode)
			{
				throw new HttpRequestException("GitHub download failed: " + (int)response.StatusCode);
			}
			long? contentLength = response.Content.Headers.ContentLength;
			if (contentLength.HasValue)
			{
				long valueOrDefault = contentLength.GetValueOrDefault();
				if (valueOrDefault > 134217728)
				{
					throw new InvalidDataException("GitHub asset too large.");
				}
			}
			string part = destination + ".part";
			string directoryName = Path.GetDirectoryName(destination);
			if (directoryName != null && directoryName.Length > 0)
			{
				Directory.CreateDirectory(directoryName);
			}
			long written = 0L;
			try
			{
				await using (Stream input = await response.Content.ReadAsStreamAsync(token))
				{
					await using (FileStream output = File.Create(part))
					{
						byte[] buffer = new byte[65536];
						int num;
						while ((num = await input.ReadAsync(buffer, token)) > 0)
						{
							written += num;
							if (written > 134217728)
							{
								throw new InvalidDataException("GitHub asset too large.");
							}
							await output.WriteAsync(buffer.AsMemory(0, num), token);
						}
					}
					File.Move(part, destination, overwrite: true);
				}
				return written;
			}
			catch
			{
				try
				{
					if (File.Exists(part))
					{
						File.Delete(part);
					}
				}
				catch (IOException)
				{
				}
				throw;
			}
		}
		throw new InvalidOperationException("Too many GitHub redirects.");
	}

	public void Dispose()
	{
		_client.Dispose();
	}
}
