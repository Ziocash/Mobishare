using AutoMapper;
using Mobishare.Core.Models.Chats;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Models.UserRelated;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Commands;
using Mobishare.Core.Requests.Chats.ConversationRequests.Commands;
using Mobishare.Core.Requests.Chats.MessagePairRequests.Commands;
using Mobishare.Core.Requests.Maps.CityRequests.Commands;
using Mobishare.Core.Requests.Maps.ParkingSlotRequests.Commands;
using Mobishare.Core.Requests.Users.BalanceRequest.Commands;
using Mobishare.Core.Requests.Users.HistoryCreditRequest.Commands;
using Mobishare.Core.Requests.Vehicles.PositionRequests.Commands;
using Mobishare.Core.Requests.Vehicles.ReportRequests.Commands;
using Mobishare.Core.Requests.Vehicles.RideRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Commands;

namespace Mobishare.Core.Requests;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map
        //------------------------------
        // City requests
        CreateMap<CreateCity, City>().ReverseMap();
        CreateMap<UpdateCity, City>().ReverseMap();
        CreateMap<DeleteCity, City>().ReverseMap();

        // ParkingSlot Requests
        CreateMap<CreateParkingSlot, ParkingSlot>().ReverseMap();
        CreateMap<UpdateParkingSlot, ParkingSlot>().ReverseMap();
        CreateMap<DeleteParkingSlot, ParkingSlot>().ReverseMap();
        //------------------------------


        //vehicle
        //------------------------------
        //Position requests
        CreateMap<CreatePosition, Position>().ReverseMap();

        // Vehicle requests
        CreateMap<CreateVehicle, Vehicle>().ReverseMap();
        CreateMap<UpdateVehicle, Vehicle>().ReverseMap();
        CreateMap<DeleteVehicle, Vehicle>().ReverseMap();

        // VehicleType requests
        CreateMap<CreateVehicleType, VehicleType>().ReverseMap();
        CreateMap<UpdateVehicleType, VehicleType>().ReverseMap();
        CreateMap<DeleteVehicleType, VehicleType>().ReverseMap();

        //Ride requests
        CreateMap<CreateRide, Ride>().ReverseMap();
        CreateMap<UpdateRide, Ride>().ReverseMap();
        CreateMap<DeleteRide, Ride>().ReverseMap();

        CreateMap<UpdateReport, Report>().ReverseMap();

        //-------------------------------

        // User
        //-------------------------------
        // Balance requests
        CreateMap<CreateBalance, Balance>().ReverseMap();
        CreateMap<UpdateBalance, Balance>().ReverseMap();

        // HistoryCredit requests
        CreateMap<CreateHistoryCredit, HistoryCredit>().ReverseMap();
        
        //-------------------------------

        CreateMap<CreateConversation, Conversation>().ReverseMap();
        
        CreateMap<CreateChatMessage, ChatMessage>().ReverseMap();

        CreateMap<CreateMessagePair, MessagePair>().ReverseMap();
    }
}
