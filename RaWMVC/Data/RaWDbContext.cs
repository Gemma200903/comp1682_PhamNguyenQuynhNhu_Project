using Microsoft.EntityFrameworkCore;
using RaW.MVC.Data.Configurations;
using RaWMVC.Data.Configurations;
using RaWMVC.Data.Entities;
using System.Reflection.Emit;

namespace RaWMVC.Data
{
    public class RaWDbContext : DbContext
    {
        public RaWDbContext(DbContextOptions<RaWDbContext> options)
        : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Server=QUYNHNHU;Database=RaWDbContext;User Id=itsjuneka; password=Quynhnhu@14; TrustServerCertificate=True; Trusted_Connection=False; MultipleActiveResultSets=true;";
            optionsBuilder.UseSqlServer(connectionString);

        }

        protected override void OnModelCreating(ModelBuilder Builder)
        {
            Builder
                .ApplyConfiguration(new TagConfiguration());
            Builder
                .ApplyConfiguration(new StatusConfiguration());
            Builder
                .ApplyConfiguration(new GenreConfiguration());
            Builder
                .ApplyConfiguration(new StoryConfiguration());
            Builder
                .ApplyConfiguration(new ChapterConfiguration());
            Builder
               .ApplyConfiguration(new MediumConfiguration());
            Builder
                .ApplyConfiguration(new LibraryConfiguration());
            Builder
                .ApplyConfiguration(new ChapterReadCountConfiguration());
            Builder
                .ApplyConfiguration(new CommentConfiguration());
            Builder
                .ApplyConfiguration(new LikeConfiguration());
            Builder
                .ApplyConfiguration(new FollowConfiguration());
            Builder
                .ApplyConfiguration(new PostConfiguration());
            Builder
                .ApplyConfiguration(new ReplyConfiguration());
            Builder
                .ApplyConfiguration(new ReportConfiguration());
            Builder
                .ApplyConfiguration(new NotificationConfigutation()); 
            Builder
                .ApplyConfiguration(new ScheduledDeleteConfiguration());
        }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Medium> Media { get; set; }
        public DbSet<ChapterReadCount> ChapterReadCounts { get; set; }
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Comment> Comments{ get; set; }
        public DbSet<Like> Like{ get; set; }
        public DbSet<Follow> Follows{ get; set; }
        public DbSet<Post> Posts{ get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications{ get; set; }
        public DbSet<ScheduledDelete> ScheduledDeletes{ get; set; }
    }
}
