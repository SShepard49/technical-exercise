using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("ProcessLog")]
    public class ProcessLog
    {
        public int Id { get; set; }
        public DateTimeOffset OccurredAtUtc { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }

    public class ProcessLogConfiguration : IEntityTypeConfiguration<ProcessLog>
    {
        public void Configure(EntityTypeBuilder<ProcessLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.OccurredAtUtc).IsRequired();
            builder.Property(x => x.Level).HasMaxLength(32).IsRequired();
            builder.Property(x => x.Message).HasMaxLength(1024).IsRequired();
            builder.Property(x => x.Path).HasMaxLength(512).IsRequired();
            builder.Property(x => x.Method).HasMaxLength(16).IsRequired();
        }
    }
}
