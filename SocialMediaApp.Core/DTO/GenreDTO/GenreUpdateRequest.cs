using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.GenreDTO
{
    public class GenreUpdateRequest
    {
        [Required(ErrorMessage = "Genre ID is required")]
        public Guid GenreID { get; set; }
        [Required(ErrorMessage = "Genre Name is required")]
        public string? GenreName { get; set; }
    }
}
