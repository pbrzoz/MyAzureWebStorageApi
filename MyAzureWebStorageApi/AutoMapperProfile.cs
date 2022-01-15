using AutoMapper;
using MyAzureWebStorageApi.Models;

namespace MyAzureWebStorageApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<UserCreationModel, User>();
        }
    }
}
