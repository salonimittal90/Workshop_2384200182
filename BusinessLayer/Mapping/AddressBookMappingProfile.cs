using ModelLayer.Model;
using RepositoryLayer.Entity;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Mapping
{
    public class AddressBookMappingProfile : Profile
    {
        public AddressBookMappingProfile()
        {
            // Entity -> DTO mapping
            CreateMap<AddressBookEntity, AddressBookDTO>();

            // DTO -> Entity mapping (useful when saving data)
            CreateMap<AddressBookDTO, AddressBookEntity>();

            // UserDTO to UserEntity Mapping
            CreateMap<UserDTO, UserEntity>()
           .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password)); // Map Password to PasswordHash
        }
    }
}
