Configuration=Release

cd RepositoryFramework.Interfaces
Remove-Item "project.lock.json"
& "dotnet" restore --no-cache
& "dotnet" pack -c Configuration -o \Packages

cd ..\RepositoryFramework.Api
Remove-Item "project.lock.json"
& "dotnet" restore --no-cache
& "dotnet" pack -c Configuration -o \Packages

cd ..\RepositoryFramework.Dapper
Remove-Item "project.lock.json"
& "dotnet" restore --no-cache
& "dotnet" pack -c Configuration -o \Packages

cd ..\RepositoryFramework.EntityFramework
Remove-Item "project.lock.json"
& "dotnet" restore --no-cache
& "dotnet" pack -c Configuration -o \Packages

cd ..\RepositoryFramework.MongoDB
Remove-Item "project.lock.json"
& "dotnet" restore --no-cache
& "dotnet" pack -c Configuration -o \Packages 