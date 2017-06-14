Set-Location $PSScriptRoot
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.AWS.S3.*
dotnet pack -c Release -o \Packages
 