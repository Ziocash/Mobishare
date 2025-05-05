using AutoMapper;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Maps.CityRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Commands;

namespace Mobishare.Core.Requests;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateCity, City>().ReverseMap();
        // CreateMap<UpdateCity, City>().ReverseMap();
        CreateMap<DeleteCity, City>().ReverseMap();

        CreateMap<CreateVehicle, Vehicle>().ReverseMap();
        // CreateMap<UpdateVehicle, Vehicle>().ReverseMap();
        // CreateMap<DeleteVehicle, Vehicle>().ReverseMap();

        CreateMap<CreateVehicleType, VehicleType>().ReverseMap();
        CreateMap<UpdateVehicleType, VehicleType>().ReverseMap();
        CreateMap<DeleteVehicleType, VehicleType>().ReverseMap();
    }
}
