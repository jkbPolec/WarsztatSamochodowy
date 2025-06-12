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


    public partial PartTypeDto ToDto(PartType partType);
    public partial PartType ToEntity(PartTypeDto dto);
    public partial void UpdateEntity(PartTypeDto dto, PartType entity);
}