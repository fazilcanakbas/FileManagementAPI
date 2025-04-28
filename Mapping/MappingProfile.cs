using AutoMapper;
using FileManagementAPI.DTOs;
using FileManagementAPI.Models;

namespace FileManagementAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
           
            CreateMap<AppUser, UserDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); 

            CreateMap<FileEntity, FileDetailsDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.FolderName, opt => opt.MapFrom(src => src.Folder.Name));

            CreateMap<Folder, FolderDetailsDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.ParentFolderName, opt => opt.MapFrom(src => src.ParentFolder.Name))
                .ForMember(dest => dest.SubFolderCount, opt => opt.MapFrom(src => src.SubFolders.Count))
                .ForMember(dest => dest.FileCount, opt => opt.MapFrom(src => src.Files.Count));

           
            CreateMap<FileUploadDto, FileEntity>();
            CreateMap<FolderCreateDto, Folder>();
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}