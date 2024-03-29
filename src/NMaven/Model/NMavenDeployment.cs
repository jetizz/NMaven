using Microsoft.Build.Framework;

namespace NMaven.Model
{
    public class NMavenDeployment : TaskItemBased
    {
        public NMavenDeployment(ITaskItem item)
            : base(item)
        { }

        public string Name => this.GetItemMetadata("Identity");
        public string ArtifactId => this.GetItemMetadata();
        public string Files => this.GetItemMetadata();
        public string Destination => this.GetItemMetadata();
        
        /// <summary>
        /// When true, the folder structure of the artifact is preserved when deploying. 
        /// </summary>
        public bool PreserveFolderStructure => bool.TryParse(this.GetItemMetadata(), out var b) && b;
        /// <summary>
        /// When set - the relative path inside a package is removed from the artifact when deploying.
        /// Relative path is the directory segment of <see cref="Files"/> parameter.
        /// </summary>
        public bool RemoveRelativePath => bool.TryParse(this.GetItemMetadata(), out var b) && b;
    }
}
