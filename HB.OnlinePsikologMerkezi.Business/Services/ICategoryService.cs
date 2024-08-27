using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Services
{
    public interface ICategoryService
    {
        Task<Response<List<CategoryListDto>>> GetCategories();
        Task<Response<CategoryAddDto>> AddCategory(CategoryAddDto dto);
        Task<Response<CategoryListDto>> RemoveCategory(int id);

    }
}
