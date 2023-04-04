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
    }
}
