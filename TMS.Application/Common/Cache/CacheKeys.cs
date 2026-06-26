namespace TMS.Application
{
    public static class CacheKeys
    {
        public static string ProjectsForUserPrefix(string ownerId) =>
            $"projects:user:{ownerId}";

        public static string ProjectsPaged(string ownerId, int pageNumber, int pageSize) =>
            $"{ProjectsForUserPrefix(ownerId)}:page:{pageNumber}:size:{pageSize}";

        public static string ProjectById(string ownerId, Guid projectId) =>
            $"project:user:{ownerId}:id:{projectId}";

        public static string TasksByProject(string ownerId, Guid projectId) =>
            $"tasks:user:{ownerId}:project:{projectId}";
    }
}
