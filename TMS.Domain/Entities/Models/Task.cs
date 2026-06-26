using System.ComponentModel.DataAnnotations.Schema;

namespace TMS.Domain
{
    public class Task : BaseEntity
    {
        public string Title { get; private set; } = null!;
        public string? Description { get; private set; }
        public TaskStatus Status { get; private set; }
        public DateTime? DueDate { get; private set; }
        public TaskPriority Priority { get; private set; }


        [ForeignKey("Project")]
        public Guid ProjectId { get; private set; }
        public Project Project { get; private set; } = null!;



        private Task() { }

        #region Domain Methods
        public static Task Create(
    Guid projectId,
    string title,
    string? description,
    TaskStatus status,
    DateTime? dueDate,
    TaskPriority priority,
    Guid? createdBy = null)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException("Project id is required.");
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title is required.");

            return new Task
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = title.Trim(),
                Description = description?.Trim(),
                Status = status,
                DueDate = dueDate,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void UpdateDetails(string title, string? description, Guid? updatedBy = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title is required.");

            Title = title.Trim();
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void ChangeStatus(TaskStatus status, Guid? updatedBy = null)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void UpdateDueDate(DateTime? dueDate, Guid? updatedBy = null)
        {
            DueDate = dueDate;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void ChangePriority(TaskPriority priority, Guid? updatedBy = null)
        {
            Priority = priority;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
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
