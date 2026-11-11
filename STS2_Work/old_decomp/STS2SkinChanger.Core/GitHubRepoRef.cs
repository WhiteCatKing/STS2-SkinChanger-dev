namespace STS2SkinChanger.Core;

internal sealed record GitHubRepoRef(string Owner, string Name)
{
	public string FullName => Owner + "/" + Name;

	public string Url => "https://github.com/" + FullName;
}
