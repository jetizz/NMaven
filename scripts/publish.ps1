param ($Version)

if (!$Version) {
    $Version = Read-Host -Prompt 'Enter the package version'
}

dotnet pack ..\src\NMaven\NMaven.csproj -c Release -o .. /p:Version=$Version