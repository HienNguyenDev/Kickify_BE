using Kickify.Api.Extensions;
using Kickify.Application.Features.Holidays.Queries.GetAllHolidays;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/holidays")]
public class HolidaysController : ControllerBase
{
    private readonly ISender _sender;

    public HolidaysController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all system holidays.
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAllHolidaysQuery(), cancellationToken);
        return result.MatchOk();
    }
}