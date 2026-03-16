using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Holidays.Commands.UpdateHoliday;

public class UpdateHolidayCommandHandler : ICommandHandler<UpdateHolidayCommand, UpdateHolidayResponse>
{
    private readonly IHolidayRepository _holidayRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHolidayCommandHandler(
        IHolidayRepository holidayRepository,
        IUnitOfWork unitOfWork)
    {
        _holidayRepository = holidayRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateHolidayResponse>> Handle(UpdateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _holidayRepository.GetByIdAsync(request.HolidayId);
        if (holiday is null)
        {
            return Result.Failure<UpdateHolidayResponse>(HolidayErrors.NotFound(request.HolidayId));
        }

        var holidayDate = request.Date.Date;
        var exists = await _holidayRepository.ExistsByDateAsync(holidayDate, request.HolidayId, cancellationToken);
        if (exists)
        {
            return Result.Failure<UpdateHolidayResponse>(HolidayErrors.AlreadyExistsOnDate(holidayDate));
        }

        holiday.Date = holidayDate;
        holiday.Name = request.Name.Trim();

        _holidayRepository.Update(holiday);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateHolidayResponse(
            holiday.Id,
            holiday.Date,
            holiday.Name,
            holiday.UpdatedAt
        ));
    }
}
