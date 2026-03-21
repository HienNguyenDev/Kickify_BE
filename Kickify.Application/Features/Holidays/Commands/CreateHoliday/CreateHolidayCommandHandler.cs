using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Holidays.Commands.CreateHoliday;

public class CreateHolidayCommandHandler : ICommandHandler<CreateHolidayCommand, CreateHolidayResponse>
{
    private readonly IHolidayRepository _holidayRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateHolidayCommandHandler(
        IHolidayRepository holidayRepository,
        IUnitOfWork unitOfWork)
    {
        _holidayRepository = holidayRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateHolidayResponse>> Handle(CreateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holidayDate = request.Date.Date;

        var exists = await _holidayRepository.ExistsByDateAsync(holidayDate, cancellationToken: cancellationToken);
        if (exists)
        {
            return Result.Failure<CreateHolidayResponse>(HolidayErrors.AlreadyExistsOnDate(holidayDate));
        }

        var holiday = new Holiday
        {
            Id = Guid.NewGuid(),
            Date = holidayDate,
            Name = request.Name.Trim()
        };

        await _holidayRepository.AddAsync(holiday);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateHolidayResponse(
            holiday.Id,
            holiday.Date,
            holiday.Name,
            holiday.CreatedAt
        ));
    }
}
