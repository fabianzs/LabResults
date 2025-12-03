using System.ComponentModel.DataAnnotations;

namespace LabResults.Entities
{
    public class Patient
    {
        public long Id { get; set; }

        public string PatientName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        public ICollection<Sample> Samples { get; set; } = new List<Sample>();
    }
}
