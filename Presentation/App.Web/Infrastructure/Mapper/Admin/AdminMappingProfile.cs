using AutoMapper;
using App.Core.Domain.Directory;
using App.Core.Domain.Localization;
using App.Core.Domain.Registrations;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Registrations;

namespace App.Web.Infrastructure.Mapper.Admin
{
    public class AdminMappingProfile : Profile
    {
        public AdminMappingProfile()
        {
            // Directory
            CreateMap<Country, CountryModel>().ReverseMap();
            CreateMap<StateProvince, StateProvinceModel>().ReverseMap();

            // Localization
            CreateMap<Language, LanguageModel>().ReverseMap();

            // Registrations
            CreateMap<Registration, RegistrationModel>().ReverseMap();
            CreateMap<FIContact, ContactModel>().ReverseMap();
            CreateMap<FIDocument, DocumentModel>().ReverseMap();
            CreateMap<FIRegistrationStatusLog, StatusLogModel>().ReverseMap();
        }
    }
}
