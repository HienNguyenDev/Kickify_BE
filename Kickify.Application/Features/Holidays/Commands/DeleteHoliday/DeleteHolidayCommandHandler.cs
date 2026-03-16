using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Holidays.Commands.DeleteHoliday;

public class DeleteHolidayCommandHandler : ICommandHandler<DeleteHolidayCommand, DeleteHolidayResponse>
{
    private readonly IHolidayRepository _holidayRepository;

    public DeleteHolidayCommandHandler(
        IHolidayRepository holidayRepository)
    {
        _holidayRepository = holidayRepository;
    }

    public async Task<Result<DeleteHolidayResponse>> Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        var isDeleted = await _holidayRepository.HardDeleteByIdAsync(request.HolidayId, cancellationToken);
        if (!isDeleted)
        {
            return Result.Failure<DeleteHolidayResponse>(HolidayErrors.NotFound(request.HolidayId));
        }

        return Result.Success(new DeleteHolidayResponse(request.HolidayId, true));
    }
}
