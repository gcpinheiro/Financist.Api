namespace Financist.Application.Features.Categories;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken = default);
}
