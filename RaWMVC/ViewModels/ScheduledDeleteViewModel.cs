namespace RaWMVC.ViewModels
{
    public class ScheduledDeleteViewModel
    {
        public Guid StoryId { get; set; }
        public string StoryTitle { get; set; }
        public string AuthorName { get; set; }
        public DateTime ApprovedTime { get; set; }
        public DateTime ScheduledDeleteTime { get; set; }
        public TimeSpan TimeRemaining => ScheduledDeleteTime - DateTime.Now;
    }
}
