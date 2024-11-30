using Microsoft.AspNetCore.Mvc;
using RaWMVC.Commons;
using RaWMVC.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace RaWMVC.ViewModels
{
    [Bind("ChapterId, ChapterTitle, Position, ChapterContent, PublishDate, IsPublished, IsDraft, StoryId, UserId, Username, ReadCount, LikeCount")]
    public class ChapterViewModel
    {
        public Guid ChapterId { get; set; }
        [Required(ErrorMessage = "Do not leave blank!!!")]
        public string ChapterTitle { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public int Position { get; set; }
        public string? ChapterContent { get; set; }
        public DateTime PublishDate { get; set; }
        public Boolean IsPublished { get; set; }
        public Boolean IsDraft { get; set; }
        public Guid StoryId { get; set; }
        public int ReadCount{ get; set; }
        public int LikeCount { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
