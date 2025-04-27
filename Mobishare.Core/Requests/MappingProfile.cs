using AutoMapper;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Commands;

namespace Mobishare.Core.Requests;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateVehicle, Vehicle>().ReverseMap();
        // CreateMap<UpdateVehicle, Vehicle>().ReverseMap();
        // CreateMap<DeleteVehicle, Vehicle>().ReverseMap();

        CreateMap<CreateVehicleType, VehicleType>().ReverseMap();
        CreateMap<UpdateVehicleType, VehicleType>().ReverseMap();
        CreateMap<DeleteVehicleType, VehicleType>().ReverseMap();
    }
}
