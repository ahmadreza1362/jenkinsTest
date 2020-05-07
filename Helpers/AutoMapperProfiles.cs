using AutoMapper;
using Datingapp.API.Dtos;
using Datingapp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datingapp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {

            CreateMap<User, UserForListDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(d => d.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            
            CreateMap<User, UserForDetailsDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(d => d.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge())); ;
            CreateMap<Photo, PhotosForDetailedDto>();

            CreateMap<UserForUpdateDto, User>();
            CreateMap<PhotoForCreationDto,Photo>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForReturnDto, Photo>();
            CreateMap<UserForRegisterDto, User>();
            CreateMap<MessageForCreationDto, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnDto>()
                .ForMember(u=>u.SenderPhotoUrl, opt => opt.MapFrom(d=>d.Sender.Photos.FirstOrDefault(d=>d.IsMain).Url))
                .ForMember(u=>u.RecipientPhotoUrl , opt => opt.MapFrom(d=>d.Recipient.Photos.FirstOrDefault(d=>d.IsMain).Url));

        }
    }
}
