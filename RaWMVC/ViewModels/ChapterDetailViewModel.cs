using RaWMVC.Data.Entities;

namespace RaWMVC.ViewModels
{
    public class ChapterDetailViewModel
    {
        public Guid ChapterId { get; set; }
        public Guid StoryId { get; set; }
        public ChapterViewModel Chapter { get; set; }
        public CommentViewModel Comments { get; set; }
        public List<StoryViewModel> SuggestedStories { get; set; }
        public int TotalReadCount { get; set; }
        public CommentViewModel CommentVM { get; set; }
        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
        public string UserId{ get; set; }
        public string Username { get; set; }
        public string ProfilePicture { get; set; }
        public Chapter NextChapter { get; set; }
        public IEnumerable<ChapterViewModel> Chapters { get; set; }

    }
}
