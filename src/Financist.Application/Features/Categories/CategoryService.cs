using Financist.Application.Abstractions.Persistence;
using Financist.Application.Abstractions.Services;
using Financist.Application.Common.Exceptions;
using Financist.Domain.Entities;

namespace Financist.Application.Features.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["Category name is required."]
            });
        }

        var userId = _currentUserService.GetRequiredUserId();
        var normalizedName = request.Name.Trim();

        if (await _categoryRepository.ExistsByNameAsync(userId, normalizedName, request.Type, cancellationToken))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["A category with the same name already exists for this transaction type."]
            });
        }

        var category = Category.Create(userId, normalizedName, request.Type);
        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(category);
    }

    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var categories = await _categoryRepository.ListByUserAsync(userId, cancellationToken);
        return categories.Select(Map).ToList();
    }

    private static CategoryDto Map(Category category)
    {
        return new CategoryDto(category.Id, category.Name, category.Type, category.IsSystem);
    }
}
