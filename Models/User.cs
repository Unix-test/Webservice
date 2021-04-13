using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;


namespace Webserver.Models
{
    [BsonIgnoreExtraElements] 
    public class User
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonId]
        public string Id { get; set; }
        
        
        [BsonElement("username")]
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [DataMember]
        public string Firstname { get; set; }
        
        [DataMember]
        public string Lastname { get; set; }
        
        [DataMember]
        public string Email { get; set; }
        
        [DataMember]
        public byte[] PasswordHash { get; set; }
        
        [DataMember]
        public byte[] PasswordSalt { get; set; }
        
        [DataMember] 
        public string Role { get; set; }

        [DataMember]
        public string[] ImageProfile { get; set; }
        
        [DataMember]
        public DateTime CreateAt { get; set; }
        
        [DataMember]
        [Timestamp]
        public string UpdateAt { get; set; }
    }
}