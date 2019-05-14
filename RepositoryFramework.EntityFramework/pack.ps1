Set-Location $PSScriptRoot
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del C:\Export\Packages\RepositoryFramework.EntityFramework.*
dotnet pack -c Release -o C:\Export\Packages
