dev:
	dotnet watch --project AlertManagerWebhook
version:
	dotnet run --project AlertManagerWebhook -- --version
pub:
	dotnet publish -c Release -r linux-x64 --self-contained false
secrets:
	dotnet user-secrets list --project AlertManagerWebhook
