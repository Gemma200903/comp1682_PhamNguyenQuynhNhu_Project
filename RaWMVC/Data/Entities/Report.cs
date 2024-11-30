using System.ComponentModel.DataAnnotations;

namespace RaWMVC.Data.Entities
{
    public class Report
    {
        public Guid ReportId { get; set; }

        public Guid StoryId { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public virtual Story Story { get; set; }

        public string UserId { get; set; }
        public string Username { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }

        public bool IsReviewed { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public DateTime ReportDate { get; set; }

        public string? AdminComments { get; set; }
    }
}
