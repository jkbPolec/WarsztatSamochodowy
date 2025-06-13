using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Mappers;

public class ServiceOrderMapper
{
    private readonly CommentMapper _commentMapper;
    private readonly ServiceTaskMapper _serviceTaskMapper;
    private readonly VehicleMapper _vehicleMapper;

    public ServiceOrderMapper(VehicleMapper vehicleMapper, ServiceTaskMapper serviceTaskMapper,
        CommentMapper commentMapper)
    {
        _vehicleMapper = vehicleMapper;
        _serviceTaskMapper = serviceTaskMapper;
        _commentMapper = commentMapper;
    }

    public ServiceOrderDto ToDto(ServiceOrder order)
    {
        if (order == null) return null;

        return new ServiceOrderDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            FinishedDate = order.FinishedDate,
            VehicleId = order.VehicleId,
            MechanicId = order.MechanicId,
            MechanicName = order.MechanicName,

            // Reużywamy wstrzykniętych mapperów
            Vehicle = order.Vehicle != null ? _vehicleMapper.ToDto(order.Vehicle) : null,
            ServiceTasks = _serviceTaskMapper.ToDtoList(order.ServiceTasks),
            Comments = order.Comments?.Select(_commentMapper.ToDto).ToList() ?? new List<CommentDto>(),

            // Kluczowe dla formularza edycji
            SelectedTaskIds = order.ServiceTasks?.Select(t => t.Id).ToList() ?? new List<int>()
        };
    }

    public List<ServiceOrderDto> ToDtoList(IEnumerable<ServiceOrder> orders)
    {
        return orders?.Select(ToDto).ToList() ?? new List<ServiceOrderDto>();
    }

    // Ta metoda jest bardzo prosta - tworzy tylko szkielet do zapisu
    public ServiceOrder ToEntity(ServiceOrderDto dto)
    {
        if (dto == null) return null;

        return new ServiceOrder
        {
            Id = dto.Id,
            Status = dto.Status,
            VehicleId = dto.VehicleId,
            MechanicId = dto.MechanicId
        };
    }
}