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
            CreateMap<Invoice, InvoiceDto>();
            CreateMap<CreateInvoiceDto, Invoice>()
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => GenerateInvoiceNumber()))
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()); // We'll calculate this

            // Invoice item mappings
            CreateMap<InvoiceItem, InvoiceItemDto>();
            CreateMap<CreateInvoiceItemDto, InvoiceItem>()
                .ForMember(dest => dest.TotalPrice,
                    opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}