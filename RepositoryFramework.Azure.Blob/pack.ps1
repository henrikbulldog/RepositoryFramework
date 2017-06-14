Set-Location $PSScriptRoot
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Azure.Blob.*
dotnet pack -c Release -o \Packages
