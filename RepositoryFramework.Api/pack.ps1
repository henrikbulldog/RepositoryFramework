Set-Location $PSScriptRoot
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Api.*
dotnet pack -c Release -o \Packages
