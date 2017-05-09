Remove-Item "project.lock.json"
& "dotnet" restore --no-cache
& "dotnet" pack -c Release -o \Packages
