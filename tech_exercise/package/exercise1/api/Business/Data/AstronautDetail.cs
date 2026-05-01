using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("AstronautDetail")]
    public class AstronautDetail
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public string CurrentRank { get; set; } = string.Empty;

        public string CurrentDutyTitle { get; set; } = string.Empty;

        public DateOnly CareerStartDate { get; set; } //changed DateTime to DateOnly for simplicity.

        public DateOnly? CareerEndDate { get; set; } //changed DateTime to DateOnly for simplicity.

        public virtual Person? Person { get; set; }
    }

    public class AstronautDetailConfiguration : IEntityTypeConfiguration<AstronautDetail>
    {
        public void Configure(EntityTypeBuilder<AstronautDetail> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            //AstronautDetail cannot exist without a Person
            builder.HasOne(x => x.Person)
                .WithOne(x => x.AstronautDetail)
                .HasForeignKey<AstronautDetail>(x => x.PersonId)
                .IsRequired();
        }
    }
}
