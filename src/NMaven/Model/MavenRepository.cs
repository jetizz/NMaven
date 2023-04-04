using Microsoft.Build.Framework;

namespace NMaven.Model
{
    public class MavenRepository : TaskItemBased
    {
        public MavenRepository(ITaskItem item)
            : base(item)
        { }

        public string Name => this.GetItemMetadata("Identity");
        public string Url => this.GetItemMetadata();
    }
}
