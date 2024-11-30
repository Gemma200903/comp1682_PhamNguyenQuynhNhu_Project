namespace RaWMVC.Data.Entities
{
    public class Library
    {
        public string Id { get; set; }
        public bool InMyLibrary { get; set; }
        public Guid? StoryId { get; set; }
        public virtual Story Story { get; set; }
        public string UserId { get; set; }
    }
}
