using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Webserver.Models
{
    public class AuthenticateModel
    {
        [Required]
        [DataMember]
        public string Email { get; set; }
        
        [Required]
        [DataMember]
        public string Password { get; set; }
    }
}