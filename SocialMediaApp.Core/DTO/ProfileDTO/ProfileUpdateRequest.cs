using Microsoft.AspNetCore.Http;
using SocialMediaApp.Core.Enumeration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.ProfileDTO
{
    public class ProfileUpdateRequest
    {
        [Required(ErrorMessage = "First name is required")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Profile image is required")]
        public IFormFile ProfileImg { get; set; }

        [Required(ErrorMessage = "Profile background is required")]
        public IFormFile ProfileBackground { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public GenderOptions Gender { get; set; } = GenderOptions.MALE;

        [Required(ErrorMessage = "Birth date is required")]
        public DateTime BirthDate { get; set; }

        public Guid UserID { get; set; }
    }
}
