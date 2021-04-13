using System.Runtime.Serialization;

namespace Webserver.Models
{
    public class UpdateModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        
        [DataMember]
        public string Password { get; set; }
    }
}