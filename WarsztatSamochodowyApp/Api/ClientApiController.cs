using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;

namespace WarsztatSamochodowyApp.API;

[ApiController]
[Route("api/[controller]")]
public class ClientApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ClientMapper _mapper;

    public ClientApiController(ApplicationDbContext context, ClientMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [Route("all")]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAllClients()
    {
        var clients = await _context.Clients.ToListAsync();
        var dtos = clients.Select(c => _mapper.ToDto(c)).ToList();
        return Ok(dtos);
    }
}