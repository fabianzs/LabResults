namespace LabResults.Domain.Entities
{
    public class Patient
    {
        public int Id { get; set; }

        public string PatientName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }

        public ICollection<Sample> Samples { get; set; } = new List<Sample>();
    }
}
