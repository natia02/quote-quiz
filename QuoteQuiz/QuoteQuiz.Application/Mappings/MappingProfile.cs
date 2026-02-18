using AutoMapper;
using QuoteQuiz.Application.DTOs.Admin;
using QuoteQuiz.Application.DTOs.Auth;
using QuoteQuiz.Domain.Entities;

namespace QuoteQuiz.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        CreateMap<User, AuthResponseDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.Token, opt => opt.Ignore());

        // Quote mappings
        CreateMap<Quote, QuoteDto>()
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedBy.Username));

        // GameHistory mappings
        CreateMap<GameHistory, GameHistoryDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.QuoteText, opt => opt.MapFrom(src => src.Quote.QuoteText))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Quote.AuthorName))
            .ForMember(dest => dest.QuizMode, opt => opt.MapFrom(src => src.QuizMode.ToString()));
    }
}