namespace RaWMVC.Data.Entities
{
    public class Reply
    {
        public Guid ReplyId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string ReplyContent { get; set; }
        public DateTime CreateAt { get; set; }
        public string ProfilePicture { get; set; }

        public Guid PostId { get; set; }
        public Post Post { get; set; }
    }
}
