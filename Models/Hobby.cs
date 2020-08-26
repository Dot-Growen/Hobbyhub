using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace belt_exam.Models {
    public class Hobby {

        [Key]
        public int HobbyId { get; set; }

        [Required (ErrorMessage = "Enter a Hobby")]
        [MinLength (2, ErrorMessage = "Name must be at least 2 characters")]
        public string Name { get; set; }

        [Required (ErrorMessage = "Enter a description")]
        [MinLength (2, ErrorMessage = "description must be at least 2 characters")]
        public string Description { get; set; }

        public List<Association> Enthusiasts {get;set;}

        public int Novice {get;set;} = 0;
        public int Intermediate {get;set;} = 0;
        public int Expert {get;set;} = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}