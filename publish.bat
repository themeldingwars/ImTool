
dotnet publish Demo/Demo.csproj -r win-x64 -c Release -o ./publish /p:PublishSingleFile=true /p:PublishReadyToRun=false /p:PublishTrimmed=false /p:TrimMode=link /p:EnableCompressionInSingleFile=true /p:IncludeAllContentForSelfExtract=true --self-contained true
move publish\Demo.exe publish\Demo-win-x64.exe