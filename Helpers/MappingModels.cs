using AutoMapper;
using Webserver.Models;

namespace Webserver.Helpers
{
    public class MappingModels:Profile
    {
        public MappingModels()
        {
            CreateMap<User, UserModels>();
            CreateMap<RegisterModel, User>();
            CreateMap<UpdateModel, User>();
        }
    }
}