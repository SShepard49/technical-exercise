using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("AstronautDuty")]
    public class AstronautDuty
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public string Rank { get; set; } = string.Empty;

        public string DutyTitle { get; set; } = string.Empty;

        public DateOnly DutyStartDate { get; set; }

        public DateOnly? DutyEndDate { get; set; }

        public virtual Person? Person { get; set; }
    }

    public class AstronautDutyConfiguration : IEntityTypeConfiguration<AstronautDuty>
    {
        public void Configure(EntityTypeBuilder<AstronautDuty> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasOne(x => x.Person)
                .WithMany(x => x.AstronautDuties)
                .HasForeignKey(x => x.PersonId)
                .IsRequired();

            //A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time.
            builder.HasIndex(x => new { x.PersonId, x.DutyTitle, x.DutyStartDate }).IsUnique();
            
            //A Person's Current Duty will not have a Duty End Date.
            builder.HasIndex(x => x.PersonId)
                .IsUnique()
                .HasFilter("\"DutyEndDate\" IS NULL");
        }
    }
}
