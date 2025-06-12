using Riok.Mapperly.Abstractions;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Models;

[Mapper]
public partial class CommentMapper
{
    public partial CommentDto ToDto(Comment entity);
    public partial Comment ToEntity(CommentDto dto);
}