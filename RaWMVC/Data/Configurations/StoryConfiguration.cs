using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaWMVC.Data.Entities;

namespace RaW.MVC.Data.Configurations
{
    public class StoryConfiguration : IEntityTypeConfiguration<Story>
    {
        public void Configure(EntityTypeBuilder<Story> builder)
        {
            builder.Property(s => s.StoryTitle)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.StoryDescription)
                .IsRequired()
                .HasMaxLength(5000);

            builder.Property(s => s.CoverImage)
                .HasMaxLength(100);

            builder.HasKey(s => new { s.StoryId });

            builder.HasMany(s => s.Libraries)
               .WithOne(l => l.Story)
               .HasForeignKey(l => l.StoryId)
               .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
