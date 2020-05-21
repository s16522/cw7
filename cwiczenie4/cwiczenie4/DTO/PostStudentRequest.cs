using System.ComponentModel.DataAnnotations;

namespace cwiczenie3.DTO
{
    public class PostStudentRequest
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [RegularExpression("^s[0-9]+$")]
        public string IndexNumber { get; set; }
        [Required]
        public string BirthDate { get; set; }
        [Required]
        public string Studies { get; set; }
    }
}