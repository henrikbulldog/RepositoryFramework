Set-Location $PSScriptRoot
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.EntityFramework.*
dotnet pack -c Release -o \Packages
