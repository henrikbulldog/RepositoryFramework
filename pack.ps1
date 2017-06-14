Set-Location $PSScriptRoot

cd RepositoryFramework.Api
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Api.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.AWS.S3
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.AWS.S3.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.Azure.Blob
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Azure.Blob.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.Dapper
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Dapper.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.EntityFramework
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.EntityFramework.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.Interfaces
Remove-Item "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Interfaces.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.MongoDB
Remove-Item  "project.lock.json"
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.MongoDB.*
dotnet pack -c Release -o \Packages

cd ..