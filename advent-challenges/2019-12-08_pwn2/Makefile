distrib:
	cp Program.cs dist && cp pwn2.csproj dist && zip -r dist.zip dist/* && cp dist.zip $$(sha256sum dist.zip | awk '{ print $$1 }')-dist.zip