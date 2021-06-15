
dotnet publish Demo/Demo.csproj -r win-x64 -c Debug -o ./publish/win-x64/Demo /p:PublishSingleFile=true /p:PublishTrimmed=true /p:IncludeAllContentForSelfExtract=true --self-contained true
dotnet publish Demo/Demo.csproj -r linux-x64 -c Debug -o ./publish/linux-x64/Demo /p:PublishSingleFile=false /p:PublishTrimmed=true /p:IncludeAllContentForSelfExtract=false --self-contained true
