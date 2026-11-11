using Application.DTOs;
using AutoMapper;
using Domain.Models;

namespace Application.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<Article, ArticleDto>();
        CreateMap<ArticleDto, Article>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.PostId, options => options.Ignore())
            .ForMember(destination => destination.Post, options => options.Ignore());

        CreateMap<CreatePostDto, Post>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Author, options => options.Ignore())
            .ForMember(destination => destination.Articles, options => options.Ignore())
            .ForMember(destination => destination.PostCategories, options => options.Ignore())
            .ForMember(destination => destination.Comments, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.PublishedAt, options => options.Ignore());

        CreateMap<Post, PostDto>()
            .ForCtorParam("Articles", options => options.MapFrom(source =>
                source.Articles.OrderBy(article => article.Order).ThenBy(article => article.Id)))
            .ForCtorParam("AuthorName", options => options.MapFrom(source => source.Author.FullName))
            .ForCtorParam("CategoryIds", options => options.MapFrom(source =>
                source.PostCategories.Select(postCategory => postCategory.CategoryId)))
            .ForCtorParam("CommentCount", options => options.MapFrom(source => source.Comments.Count));

        CreateMap<UpdatePostDto, Post>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.AuthorId, options => options.Ignore())
            .ForMember(destination => destination.Author, options => options.Ignore())
            .ForMember(destination => destination.Articles, options => options.Ignore())
            .ForMember(destination => destination.PostCategories, options => options.Ignore())
            .ForMember(destination => destination.Comments, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.PublishedAt, options => options.Ignore())
            .ForAllMembers(options =>
                options.Condition((_, _, sourceMember) => sourceMember is not null));

        CreateMap<CreateCommentDto, Comment>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.Post, options => options.Ignore())
            .ForMember(destination => destination.Author, options => options.Ignore())
            .ForMember(destination => destination.ParentComment, options => options.Ignore())
            .ForMember(destination => destination.Replies, options => options.Ignore());

        CreateMap<Comment, CommentDto>()
            .ForCtorParam("AuthorName", options => options.MapFrom(source => source.Author.FullName));

        CreateMap<UpdateCommentDto, Comment>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.PostId, options => options.Ignore())
            .ForMember(destination => destination.Post, options => options.Ignore())
            .ForMember(destination => destination.AuthorId, options => options.Ignore())
            .ForMember(destination => destination.Author, options => options.Ignore())
            .ForMember(destination => destination.ParentCommentId, options => options.Ignore())
            .ForMember(destination => destination.ParentComment, options => options.Ignore())
            .ForMember(destination => destination.Replies, options => options.Ignore())
            .ForAllMembers(options =>
                options.Condition((_, _, sourceMember) => sourceMember is not null));
    }
}
