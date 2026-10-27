namespace Reviews.Application.Common.Mappings;

using AutoMapper;
using Reviews.Application.Common.Models;
using Reviews.Domain.Entities;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.Author != null ? s.Author.DisplayName : string.Empty))
            .ForMember(d => d.CommentsCount, o => o.MapFrom(s => s.Comments != null ? s.Comments.Count : 0));
    }
}
