using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Mappers;

public class ServicesMapper
{
    // public ServiceTaskDto ToDto(ServiceTask task)
    // {
    //     return new ServiceTaskDto
    //     {
    //         Id = task.Id,
    //         Name = task.Name,
    //         Description = task.Description,
    //         Price = task.Price,
    //         UsedParts = task.UsedParts.Select(up => new UsedPartDto
    //         {
    //             PartId = up.PartId,
    //             Quantity = up.Quantity,
    //             Part = new PartDto
    //             {
    //                 Id = up.Part.Id,
    //                 Name = up.Part.Name,
    //                 Price = up.Part.Price
    //             }
    //         }).ToList()
    //     };
    // }
    //
    // public ServiceTask ToEntity(ServiceTaskDto dto)
    // {
    //     return new ServiceTask
    //     {
    //         Id = dto.Id,
    //         Name = dto.Name,
    //         Description = dto.Description,
    //         Price = dto.Price,
    //         UsedParts = dto.UsedParts.Select(up => new UsedPart
    //         {
    //             PartId = up.PartId,
    //             Quantity = up.Quantity
    //         }).ToList()
    //     };
    // }
    //
    // public void UpdateEntity(ServiceTaskDto dto, ServiceTask entity)
    // {
    //     entity.Description = dto.Description;
    //     entity.Name = dto.Name;
    //     entity.Price = dto.Price;
    //
    //     entity.UsedParts.Clear();
    //     foreach (var id in dto.UsedPartIds) entity.UsedParts.Add(new UsedPart { PartId = id });
    // }

    public ServiceOrderDto ToDto(ServiceOrder order)
    {
        return new ServiceOrderDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            FinishedDate = order.FinishedDate,
            VehicleId = order.VehicleId,
            Vehicle = order.Vehicle == null
                ? null
                : new VehicleDto
                {
                    Id = order.Vehicle.Id,
                    Vin = order.Vehicle.Vin,
                    RegistrationNumber = order.Vehicle.RegistrationNumber,
                    ImageUrl = order.Vehicle.ImageUrl,
                    ClientFullName = order.Vehicle.Client == null
                        ? "Brak klienta"
                        : $"{order.Vehicle.Client.FirstName} {order.Vehicle.Client.LastName}",
                    ClientId = order.Vehicle.ClientId
                },
            ServiceTasks = order.ServiceTasks?.Select(st => new ServiceTaskDto
            {
                Id = st.Id,
                Name = st.Name,
                Description = st.Description,
                Price = st.Price,
                UsedParts = st.UsedParts?.Select(up => new UsedPartDto
                {
                    Part = new PartDto
                    {
                        Id = up.Part?.Id ?? 0,
                        Name = up.Part?.Name ?? "Brak",
                        Price = up.Part?.Price ?? 0
                    },
                    Quantity = up.Quantity
                }).ToList() ?? new List<UsedPartDto>()
            }).ToList() ?? new List<ServiceTaskDto>(),
            SelectedTaskIds = order.ServiceTasks.Select(t => t.Id).ToList(),
            Comments = order.Comments?.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                Author = c.Author,
                CreatedAt = c.CreatedAt,
                ServiceOrderId = c.ServiceOrderId
            }).ToList() ?? new List<CommentDto>(),
            MechanicId = order.MechanicId,
            MechanicName = order.MechanicName
        };
    }

    public ServiceOrder ToEntity(ServiceOrderDto dto)
    {
        var order = new ServiceOrder
        {
            Id = dto.Id,
            OrderDate = dto.OrderDate,
            Status = dto.Status,
            FinishedDate = dto.FinishedDate,
            VehicleId = dto.VehicleId,
            MechanicId = dto.MechanicId,
            MechanicName = dto.MechanicName
        };

        // ServiceTasks trzeba pobrać z bazy na podstawie Id, żeby EF nie robił problemów z trackingiem
        // To w kontrolerze

        return order;
    }

    public void UpdateEntity(ServiceOrder existingOrder, ServiceOrder updatedOrder,
        List<int> selectedTaskIds)
    {
        existingOrder.Status = updatedOrder.Status;
        existingOrder.VehicleId = updatedOrder.VehicleId;
        existingOrder.MechanicId = updatedOrder.MechanicId;

        if (updatedOrder.Status == ServiceOrderStatus.Zakonczone && existingOrder.FinishedDate == null)
            existingOrder.FinishedDate = DateTime.Now;
        else if (updatedOrder.Status != ServiceOrderStatus.Zakonczone)
            existingOrder.FinishedDate = null;

        // Wyczyść listę zadań i dodaj nowe z bazy na podstawie selectedTaskIds
        existingOrder.ServiceTasks.Clear();
    }
}