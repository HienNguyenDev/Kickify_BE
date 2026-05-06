using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Holidays.Commands.CreateHoliday;
using Kickify.Application.Features.Holidays.Commands.DeleteHoliday;
using Kickify.Application.Features.Holidays.Commands.UpdateHoliday;
using Kickify.Application.Features.Holidays.Queries.GetAllHolidays;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IResult> GetAll(
        [FromQuery] string? keyword,
        [FromQuery] int? year,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllHolidaysQuery(keyword, year, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Create a holiday (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateHolidayRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateHolidayCommand(request.StartDate, request.EndDate, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Update a holiday (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{holidayId:guid}")]
    public async Task<IResult> Update(Guid holidayId, [FromBody] UpdateHolidayRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateHolidayCommand(holidayId, request.Date, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Delete a holiday (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{holidayId:guid}")]
    public async Task<IResult> Delete(Guid holidayId, CancellationToken cancellationToken)
    {
        var command = new DeleteHolidayCommand(holidayId);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}