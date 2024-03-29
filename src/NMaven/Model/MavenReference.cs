using System.IO;
using Microsoft.Build.Framework;

namespace NMaven.Model
{
    public class MavenReference : TaskItemBased
    {
        public MavenReference(ITaskItem item)
            : base(item)
        { }

        public string ArtifactId => this.GetItemMetadata("Identity");
        public string GroupId => this.GetItemMetadata();
        public string Version => this.GetItemMetadata();
        /// <summary>
        /// When <c>true</c>, the artifact will be redownloaded and overwritten if it already exists on disk.
        /// </summary>
        public bool Overwrite => bool.TryParse(this.GetItemMetadata(), out var b) && b;

        public string ArtifactFileName => $"{this.ArtifactId}-{this.Version}.jar";

        public string GetRepositoryUrl(MavenRepository repository)
        {
            var url = string.Join("/",
                repository.Url,
                this.GroupId.Replace('.', '/'),
                this.ArtifactId,
                this.Version,
                this.ArtifactFileName);

            return url;
        }

        public DirectoryInfo GetArtifactDirectory(DirectoryInfo nmvnPackageRoot)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(nmvnPackageRoot.FullName, this.ArtifactId, this.Version));

            return directoryInfo;
        }

        public FileInfo GetArtifactFilePath(DirectoryInfo nmvnPackageRoot)
        {
            var fileInfo = new FileInfo(Path.Combine(this.GetArtifactDirectory(nmvnPackageRoot).FullName, this.ArtifactFileName));

            return fileInfo;
        }
    }
}
