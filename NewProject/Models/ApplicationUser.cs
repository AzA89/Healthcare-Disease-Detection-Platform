using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace NewProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }
        
        [PersonalData]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
        
        [PersonalData]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        
        [PersonalData]
        public string? ProfilePicture { get; set; }
    }
} 