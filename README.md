# Description

Simple .NET Maui GUI app for https://github.com/yt-dlp/yt-dlp

# Publishing

```
dotnet publish "path to csproj" -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64 -p:WindowsPackageType=None
```