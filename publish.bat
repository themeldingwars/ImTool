
dotnet publish Demo/Demo.csproj -r win-x64 -c Release -o ./publish /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TrimMode=Link /p:IncludeAllContentForSelfExtract=true --self-contained true
move publish\Demo.exe publish\Demo-win-x64.exe