using System.Collections.ObjectModel;

namespace RaWMVC.Data.Entities
{
    public class Story
    {
        public Story()
        {
            StoryId = Guid.NewGuid();
            Chapters = new Collection<Chapter>();
            Reports = new Collection<Report>();
        }
        public Guid StoryId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string StoryTitle { get; set; }
        public string? StoryDescription { get; set; }
        public string? CoverImage { get; set; }
        public DateTime PublishDate { get; set; }
        public int Position { get; set; }
        public int PostedChapter { get; set; }
        public int DraftChapter { get; set; }

        // === Navigational Property === //
        public Guid GenreId { get; set; }
        public virtual Genre Genre { get; set; }
        public Guid StatusId { get; set; }
        public virtual Status Status { get; set; }
        public Guid TagId { get; set; }
        public virtual Tag Tag { get; set; }
        public Guid? MediumId { get; set; }
        public virtual Medium Medium { get; set; }

        //=== A story has many ===//
        public ICollection<Chapter> Chapters { get; set; }
        public virtual ICollection<Library> Libraries { get; set; } = new List<Library>();
        public ICollection<Report> Reports { get; set; }
    }
}
