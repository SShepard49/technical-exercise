namespace StargateAPI.Business.Dtos
{
    public class PersonAstronaut
    {
        public int PersonId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string CurrentRank { get; set; } = string.Empty;

        public string CurrentDutyTitle { get; set; } = string.Empty;

        public DateOnly? CareerStartDate { get; set; } //changed DateTime to DateOnly for simplicity.

        public DateOnly? CareerEndDate { get; set; } //changed DateTime to DateOnly for simplicity.

        public bool IsRetired => string.Equals(CurrentDutyTitle, "RETIRED", StringComparison.OrdinalIgnoreCase);
    }
}
