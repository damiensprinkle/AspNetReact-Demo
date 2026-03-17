using Application.DTOs;
using AutoMapper;
using Domain;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Domain → response DTO
            CreateMap<Activity, ActivityDto>();

            // Request DTO → domain (create)
            CreateMap<ActivityFormDto, Activity>();

            // Request DTO → tracked domain entity (edit — maps onto existing instance)
            CreateMap<ActivityFormDto, Activity>()
                .ForMember(d => d.Id, opt => opt.Ignore());
        }
    }
}
