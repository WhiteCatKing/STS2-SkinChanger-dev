namespace ModLaunchManager;

internal interface IConfigCloud
{
	string AccountKey { get; }

	bool CanWrite { get; }

	string? Read(string name);

	bool Write(string name, string content);
}
