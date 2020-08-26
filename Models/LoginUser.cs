using System.ComponentModel.DataAnnotations;

namespace belt_exam.Models {
    public class LoginUser {

        [Required (ErrorMessage = "Enter your email")]
        public string LoginUserName { get; set; }

        [Required (ErrorMessage = "Enter a password")]
        [DataType (DataType.Password)]
        public string LoginPassword { get; set; }
    }
}