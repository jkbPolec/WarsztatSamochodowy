using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Mappers;

public class ServicesMapper
{
    public ServiceTaskDto ToDto(ServiceTask task)
    {
        return new ServiceTaskDto
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Price = task.Price,
            UsedParts = task.UsedParts.Select(up => new UsedPartDto
            {
                PartId = up.PartId,
                Quantity = up.Quantity,
                Part = new PartDto
                {
                    Id = up.Part.Id,
                    Name = up.Part.Name,
                    Price = up.Part.Price
                }
            }).ToList()
        };
    }

    public ServiceTask ToEntity(ServiceTaskDto dto)
    {
        return new ServiceTask
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            UsedParts = dto.UsedParts.Select(up => new UsedPart
            {
                PartId = up.PartId,
                Quantity = up.Quantity
            }).ToList()
        };
    }

    public void UpdateEntity(ServiceTaskDto dto, ServiceTask entity)
    {
        entity.Description = dto.Description;
        entity.Name = dto.Name;
        entity.Price = dto.Price;

        entity.UsedParts.Clear();
        foreach (var id in dto.UsedPartIds) entity.UsedParts.Add(new UsedPart { PartId = id });
    }
}