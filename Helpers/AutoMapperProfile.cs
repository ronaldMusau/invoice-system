using AutoMapper;
using InvoiceSystem.DTOs;
using InvoiceSystem.Models;

namespace InvoiceSystem.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<UserRegisterDto, User>();

            // Invoice mappings
            CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.AssignedUserName,
                    opt => opt.MapFrom(src => src.AssignedUser.Username));

            CreateMap<CreateInvoiceDto, Invoice>()
                .ForMember(dest => dest.InvoiceNumber,
                    opt => opt.MapFrom(src => GenerateInvoiceNumber()))
                .ForMember(dest => dest.IssueDate,
                    opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.TotalAmount,
                    opt => opt.Ignore()); // We calculate this separately

            // Invoice item mappings
            CreateMap<InvoiceItem, InvoiceItemDto>();
            CreateMap<CreateInvoiceItemDto, InvoiceItem>()
                .ForMember(dest => dest.TotalPrice,
                    opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));
        }

        private string GenerateInvoiceNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"INV-{date}-{random}";
        }
    }
}