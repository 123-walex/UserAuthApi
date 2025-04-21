using System;
using System.Runtime.CompilerServices;
using AutoMapper;
using User_Authapi.DTO_s;

namespace User_Authapi.Mappings
{
    public class MappingProfile : Profile 
    {
        public MappingProfile()
        {   
            // mapping dtos to entities 
            CreateMap<CreateUserDTO, Person>();
            CreateMap<UpdateUserDTO, Person>();
        }
    }
}
