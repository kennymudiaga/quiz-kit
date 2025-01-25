using AutoMapper;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Core.Entities;
using QuizKit.Core.Mappers;

namespace QuizKit.Tests.Mappers;

public class CategoryMappingProfileTests
{
    private readonly IMapper _mapper;

    public CategoryMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CategoryMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void CategoryToModel_MapsCorrectly()
    {
        // Arrange
        var category = new Category
        {
            Id = "programming",
            Description = "Programming related questions",
            CreationTime = DateTime.UtcNow,
            LastUpdateTime = DateTime.UtcNow
        };

        // Act
        var model = _mapper.Map<CategoryModel>(category);

        // Assert
        Assert.Equal(category.Id, model.Id);
        Assert.Equal(category.Description, model.Description);
        Assert.Equal(category.CreationTime, model.CreationTime);
        Assert.Equal(category.LastUpdateTime, model.LastUpdateTime);
    }

    [Fact]
    public void CategoryToModel_WithNullValues_MapsCorrectly()
    {
        // Arrange
        var category = new Category
        {
            Id = "programming",
            Description = null,
            CreationTime = DateTime.UtcNow,
            LastUpdateTime = null
        };

        // Act
        var model = _mapper.Map<CategoryModel>(category);

        // Assert
        Assert.Equal(category.Id, model.Id);
        Assert.Null(model.Description);
        Assert.Equal(category.CreationTime, model.CreationTime);
        Assert.Null(model.LastUpdateTime);
    }
}
