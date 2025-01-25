using AutoMapper;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Mappers;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryModel>();
    }
}
