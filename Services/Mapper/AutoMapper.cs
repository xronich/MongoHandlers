using AutoMapper;
using .Abstractions.Services.Mapper;

namespace .Domain.Implementation.Services.Mapper
{
    public class AutoMapper : IAutoMapper
    {
        public Dest CreateMapper<Source, Dest>(Source source)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<Source, Dest>(); });

            IMapper mapper = config.CreateMapper();

            return mapper.Map<Source, Dest>(source);
        }
    }
}
