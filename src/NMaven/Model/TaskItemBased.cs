using System.Runtime.CompilerServices;
using Microsoft.Build.Framework;

namespace NMaven.Model
{
    public abstract class TaskItemBased
    {
        private readonly ITaskItem _item;

        public TaskItemBased(ITaskItem item)
        {
            _item = item;
        }

        protected string GetItemMetadata([CallerMemberName] string property = null) => _item.GetMetadata(property);
    }
}
