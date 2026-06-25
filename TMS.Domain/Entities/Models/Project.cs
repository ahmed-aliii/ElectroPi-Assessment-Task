namespace TMS.Domain
{
    public class Project : BaseEntity
    {
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }

        private readonly List<Task> _tasks = [];
        public IReadOnlyCollection<Task> Tasks => _tasks.AsReadOnly();

        private Project() { }

        #region Domain Methods
        public static Project Create(string name, string? description, Guid? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name is required.");

            return new Project
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Description = description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void Update(string name, string? description, Guid? updatedBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name is required.");

            Name = name.Trim();
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public Task AddTask(
            string title,
            string? description,
            TaskStatus status,
            DateTime? dueDate,
            TaskPriority priority,
            Guid? createdBy = null)
        {
            var task = Task.Create(Id, title, description, status, dueDate, priority, createdBy);
            _tasks.Add(task);
            return task;
        }

        public void SoftDelete(Guid deletedBy)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        } 
        #endregion

    }
}
