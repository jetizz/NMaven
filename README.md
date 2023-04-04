# NMaven
NMaven is a package containing a MSBuild task allowing to download maven artifact at build time. 

The aim of this tool is to automate the retrieval of files (XSD files for example) from maven artifacts. Each maven artifact referenced in a project will be downloaded into a `.nmvn` folder, next to the `.nuget` folder, and following the same rules. Then, by defining some deployment rules, some files contained in the downloaded artifacts will be copied to your project.

 passed# How to use
1. Install the `NMaven` NuGet package to add to your project the required tools:

> dotnet add package NMaven

2. Add the following items in your `csproj`:

```
<ItemGroup>
    <MavenRepository Include="Repository" Url="https://path.to.your/repository" />
    <MavenReference Include="artifact-id" GroupId="group.id" Version="X.Y.Z.R" />
    <NMavenDeployment Include="DeploymentRule" ArtifactId="artifact-id" Files="path/to/file.ext" Destination="Destination/Folder" />
</ItemGroup>
```

You can add as many of each item as required. Especially, you can add several deployment rules for the same artifact as long as the `NMavenDeployment.ArtifactId` metadata references an existing `MavenReference.Include`.

3. Don't forget to add the deployed file(s) to your `.gitignore` since they will be deployed at each build.

# NMavenDeployment rules
The `Files` metadata is used to select the files to deploy to the project from within the jar file. This selector relies on the `DirectoryInfo.GetFiles(string searchPattern, SearchOption option)` method, with `SearchOption.AllDirectories` passed.

The `Destination` metadata simply indicates the destination folder within the .NET project.

# NMaven.Example

Take a look at the `NMaven.Example` project for a working example.

This app simply references an arbitrary artifact and display its MANIFEST.LF file in the console.