namespace RaWMVC.Data.Entities
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string CommentContent { get; set; }
        public DateTime CreateOn { get; set; }
        public Guid ChapterId { get; set; }
        public Chapter Chapter { get; set; }
        public string ProfilePicture { get; set; }
    }
}