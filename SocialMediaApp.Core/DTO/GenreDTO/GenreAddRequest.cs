using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.GenreDTO
{
    public class GenreAddRequest
    {
        [Required(ErrorMessage = "Genre name is required")]
        [StringLength(40, ErrorMessage = "Genre name must be less than 40 characters")]
        public string? GenreName { get; set; }
    }
}
