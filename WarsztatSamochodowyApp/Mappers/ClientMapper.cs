using Riok.Mapperly.Abstractions;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Models;

[Mapper]
public partial class ClientMapper
{
    public partial ClientDto ToDto(Client entity);
    public partial Client ToEntity(ClientDto dto);
    public partial void UpdateEntity(ClientDto dto, Client entity);
}