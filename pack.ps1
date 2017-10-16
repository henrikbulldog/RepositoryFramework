Set-Location $PSScriptRoot

cd RepositoryFramework.Api
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Api.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.AWS.S3
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.AWS.S3.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.Azure.Blob
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Azure.Blob.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.Dapper
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Dapper.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.EntityFramework
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.EntityFramework.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.Interfaces
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.Interfaces.*
dotnet pack -c Release -o \Packages

cd ..\RepositoryFramework.MongoDB
dotnet restore --no-cache
del E:\Packages\RepositoryFramework.MongoDB.*
dotnet pack -c Release -o \Packages

cd ..