using AutoMapper;
using BoardGameBackend.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserModel, Player>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username));
        CreateMap<UserModel, PlayerInLobby>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.IsConnected, opt => opt.MapFrom(src => true));
        CreateMap<UserModel, UserModelDto>();
        CreateMap<PlayerInGame, PlayerViewModelData>();
    }


}
namespace BoardGameBackend.Mappers
{
    public static class GameMapper
    {
        // Static instance of the IMapper
        private static IMapper _mapper;

        // Static constructor to initialize the mapper with the game-specific profiles
        static GameMapper()
        {
            // Define the mapping configuration
           var config = new MapperConfiguration(cfg =>
            {
            //    cfg.ConstructServicesUsing(type => new TileTypeResolver(TileTypes.Tiles));
                cfg.AddProfile<GameMappingProfile>();
            }); 
 
            _mapper = config.CreateMapper();
        }


        public static IMapper Instance => _mapper;
    }

    public class GameMappingProfile : Profile
    {
        public GameMappingProfile()
        {
            CreateMap<PlayerInGame, PlayerViewModelData>();
            CreateMap<PlayerInGame, Player>();
        }
    }


}
