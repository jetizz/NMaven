# NMaven
NMaven is a package containing a MSBuild task allowing to download maven artifact at build time. 

The aim of this tool is to automate the retrieval of files (XSD files for example) from maven artifacts. Each maven artifact referenced in a project will be downloaded into a `.nmvn` folder, next to the `.nuget` folder, and following the same rules. Then, by defining some deployment rules, some files contained in the downloaded artifacts will be copied to your project.


# How to use
1. Install the `NMaven` NuGet package to add to your project the required tools:

> dotnet add package NMaven

2. Add the following items in your `csproj`:

```
<ItemGroup>
    <MavenRepository Include="Repository" Url="https://path.to.your/repository" Username="optional-username" Password="optional-password" />
    <MavenReference Include="artifact-id" GroupId="group.id" Version="X.Y.Z.R" />
    <NMavenDeployment Include="DeploymentRule" ArtifactId="artifact-id" Files="path/to/file.ext" Destination="Destination/Folder" />
</ItemGroup>
```

You can add as many of each item as required. Especially, you can add several deployment rules for the same artifact as long as the `NMavenDeployment.ArtifactId` metadata references an existing `MavenReference.Include`.

If `username` and `password` are provided, they will be used to authenticate against the repository using basic scheme. If not, the repository will be accessed anonymously.

3. Don't forget to add the deployed file(s) to your `.gitignore` since they will be deployed at each build.


# NMavenDeployment rules
The `Files` metadata is used to select the files to deploy to the project from within the jar file. This selector relies on the `DirectoryInfo.GetFiles(string searchPattern, SearchOption option)` method, with `SearchOption.AllDirectories` passed.

The `Destination` metadata simply indicates the destination folder within the .NET project.

Optional parameter `PreserveFolderStructure` (default `false`) can be set to `true` to preserve the folder structure of the files within the jar file during deployment.

Optional parameter `RemoveRelativePath` (default `false`) can be set to `true` to remove the relative in relation to `Files` path of the files within the jar file during deployment. Value is ignored if `PreserveFolderStructure` is not used.


# NMaven.Example

Take a look at the `NMaven.Example` project for a working example.

This app simply references an arbitrary artifact and display its MANIFEST.LF file in the console.