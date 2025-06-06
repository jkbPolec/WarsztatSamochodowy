using Riok.Mapperly.Abstractions;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Mappers;

[Mapper]
public partial class PartMapper
{
    public partial PartDto ToDto(Part part);
    public partial Part ToEntity(PartDto dto);
    public partial void UpdateEntity(PartDto dto, Part entity);
}