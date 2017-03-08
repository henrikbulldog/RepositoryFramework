How to create NuGet package
===========================
Create package:
Run pack.ps1

Set up NuGet source:
nuget sources add -source https://api.nuget.org/v3/index.json -name nuget.org

Push package to server:
nuget setapikey 0408b0f4-e92f-413a-92c5-9a472d96d901 -source https://api.nuget.org/v3/index.json
nuget push "RepositoryFramework.1.0.0.symbols.nupkg" -source nuget.org
