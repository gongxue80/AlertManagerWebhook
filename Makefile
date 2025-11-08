dev:
	dotnet watch --project AlertManagerWebhook
pub:
	dotnet publish -c Release -r linux-x64 --self-contained false
secrets:
	dotnet user-secrets list --project AlertManagerWebhook
