using Riok.Mapperly.Abstractions;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Mappers;

[Mapper]
public partial class VehicleMapper
{
    public VehicleDto ToDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            Vin = vehicle.Vin,
            RegistrationNumber = vehicle.RegistrationNumber,
            ImageUrl = vehicle.ImageUrl,
            ClientId = vehicle.ClientId,
            ClientFullName = vehicle.Client != null
                ? $"{vehicle.Client.FirstName} {vehicle.Client.LastName}"
                : string.Empty
        };
    }

    public Vehicle FromDto(VehicleDto dto)
    {
        return new Vehicle
        {
            Id = dto.Id,
            Vin = dto.Vin,
            RegistrationNumber = dto.RegistrationNumber,
            ImageUrl = dto.ImageUrl,
            ClientId = dto.ClientId,
            Client = null // bo nie masz danych na temat Clienta z DTO
        };
    }
}