using System.ComponentModel.DataAnnotations;

namespace belt_exam.Models {
    public class Association {

        [Key]
        public int AssociationId { get; set; }
        
        public int HobbyId { get; set; }

        public Hobby UsersHobbies { get; set; }

        public int UserId { get; set; }

        public User HobbiesUsers { get; set; }

        [Required (ErrorMessage = "Enter proficiency level")]
        public string Proficiency { get; set; }

    }
}