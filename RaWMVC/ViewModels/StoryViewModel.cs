using Microsoft.AspNetCore.Mvc;
using RaWMVC.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace RaWMVC.ViewModels
{
    [Bind("StoryId, StoryTitle, StoryDescription, UserId, Username, ProfilePicture, CoverImage, PublishDate, TagId, StatusId, GenreId, Chapters, Medium, PublishedChapter, DraftChapter")]
    public class StoryViewModel
    {
        public Guid StoryId { get; set; }
        [Required(ErrorMessage = "Do not leave blank!!!")]
        //[MaxLength(200, ErrorMessage = "Only allowed to contain 200 characters!!!")]
        public string StoryTitle { get; set; }
        //[MaxLength(500, ErrorMessage = "Only allowed to contain 500 characters!!!")]
        public string? StoryDescription { get; set; }
        public IFormFile CoverImage { get; set; }
        public DateTime PublishDate { get; set; }
        public Guid TagId { get; set; }
        public string TagName { get; set; }
        public Guid StatusId { get; set; }
        public string StatusName { get; set; }
        public Guid GenreId { get; set; }
        public string GenreName { get; set; }
        public int PublishedChapter { get; set; }
        public int DraftChapter { get; set; }

        public int TotalReadCount { get; set; }
        public int TotalLikeCount { get; set; }
        public Guid? CurrentListId { get; set; }
        public bool HasNotifications { get; set; }

        public string UserId { get; set; }
        public string Username { get; set; }
        public string ProfilePicture { get; set; }
        //public Guid chapterId { get; set; }
        public List<ChapterViewModel>? Chapters { get; set; }
        public List<StoryViewModel> SuggestedStories { get; set; }
        public List<StoryViewModel> NewestStories { get; set; }
        public List<StoryViewModel> HotestStories { get; set; }
        public List<StoryViewModel> CompletedStories { get; set; }
        public Medium Medium { get; set; }
    }
}