using Microsoft.AspNetCore.Mvc;
using RaWMVC.Data.Entities;

namespace RaWMVC.ViewModels
{
    [Bind("UserId, Username, AvatarUrl, JoinedDate, Introduction, FollowersCount, FollowingCount, IsFollowing")]
    public class ProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime JoinedDate { get; set; }
        public string Introduction { get; set; }

        public int FollowersCount { get; set; }
        public int ReadingListCount { get; set; }
        public bool IsFollowing { get; set; }

        public Guid PostId { get; set; }

        public List<LibraryViewModel> CurrentReadingStories { get; set; }

        public List<FollowViewModel> FollowingUsers { get; set; }

        public List<FollowViewModel> Follows { get; set; }
        public List<PostViewModel> Posts { get; set; }
    }
}
