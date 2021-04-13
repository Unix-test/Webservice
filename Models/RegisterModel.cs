using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Webserver.Models
{
    public class RegisterModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Email { get; set; }

        [Required]
        [DataMember]
        public string Password { get; set; }
    }
}