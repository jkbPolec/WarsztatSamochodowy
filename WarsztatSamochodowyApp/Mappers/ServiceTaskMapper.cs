using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Mappers;
using WarsztatSamochodowyApp.Models;

public class ServiceTaskMapper
{
    private readonly PartMapper _partMapper;

    public ServiceTaskMapper(PartMapper partMapper)
    {
        _partMapper = partMapper;
    }

    public ServiceTaskDto ToDto(ServiceTask task)
    {
        if (task == null) return null;
        return new ServiceTaskDto
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Price = task.Price,
            UsedParts = task.UsedParts?.Select(up => new UsedPartDto
            {
                PartId = up.PartId,
                Quantity = up.Quantity,
                Part = up.Part != null ? _partMapper.ToDto(up.Part) : null
            }).ToList() ?? new List<UsedPartDto>()
        };
    }

    public List<ServiceTaskDto> ToDtoList(IEnumerable<ServiceTask> tasks)
    {
        return tasks?.Select(ToDto).ToList() ?? new List<ServiceTaskDto>();
    }

    public void UpdateEntityFromDto(ServiceTaskDto dto, ServiceTask entity)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
    }
}