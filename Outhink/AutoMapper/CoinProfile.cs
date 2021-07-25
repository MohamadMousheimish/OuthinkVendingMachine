using AutoMapper;
using Outhink.Db.Enums;
using Outhink.Db.Models;
using Outhink.ResponseModels.QueryResponseModels;

namespace Outhink.AutoMapper
{
    public class CoinProfile : Profile
    {
        public CoinProfile()
        {
            CreateMap<Coin, GetCoinsResponseModel>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => MapEnumToString(src.Type)));
        }

        private string MapEnumToString(CoinType coinType)
        {
            return coinType switch
            {
                CoinType.TenCent => "Ten Cent",
                CoinType.TwentyCent => "Twenty Cent",
                CoinType.FiftyCent => "Fifty Cent",
                CoinType.OneEuro => "One Euro",
                _ => "",
            };
        }
    }
}
