using AutoMapper;
using Outhink.Db.Models;
using Outhink.ResponseModels.QueryResponseModels;

namespace Outhink.AutoMapper
{
    public class ItemProfile : Profile
    {
        public ItemProfile()
        {
            CreateMap<Item, GetItemsResponseModel>();
        }
    }
}
