using AutoMapper;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;

namespace HB.OnlinePsikologMerkezi.Business.Managers
{
    public class CategoryManager : ICategoryService
    {
        private readonly IUow uow;
        private readonly IMapper mapper;

        public CategoryManager(IUow uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public Task<Response<CategoryAddDto>> AddCategory(CategoryAddDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<Response<List<CategoryListDto>>> GetCategories()
        {

            var data = await uow.GetRepository<Category>().GetAllAsync(false);

            var mappedData = mapper.Map<List<CategoryListDto>>(data);


            return new Response<List<CategoryListDto>>(ResponseType.Success, mappedData);
        }

        public Task<Response<CategoryListDto>> RemoveCategory(int id)
        {
            throw new NotImplementedException();
        }
    }
}
