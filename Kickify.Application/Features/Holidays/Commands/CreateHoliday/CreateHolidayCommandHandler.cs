using System.Linq;
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
        var startDate = request.StartDate.Date;
        var endDate = request.EndDate.Date;

        if (endDate < startDate)
        {
            return Result.Failure<CreateHolidayResponse>(HolidayErrors.InvalidDateRange(startDate, endDate));
        }

        var datesToCreate = new List<DateTime>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            datesToCreate.Add(date);
        }

        var existingDates = await _holidayRepository.GetExistingDatesAsync(datesToCreate, cancellationToken);
        if (existingDates.Count > 0)
        {
            var conflicts = string.Join(", ", existingDates
                .OrderBy(d => d)
                .Select(d => d.ToString("dd/MM/yyyy")));

            return Result.Failure<CreateHolidayResponse>(HolidayErrors.DatesAlreadyExist(conflicts));
        }

        var newHolidays = new List<Holiday>();
        bool isMultiDay = datesToCreate.Count > 1;
        string baseName = request.Name.Trim();

        for (int index = 0; index < datesToCreate.Count; index++)
        {
            var name = isMultiDay
                ? $"{baseName} (Ngày {index + 1})"
                : baseName;

            newHolidays.Add(new Holiday
            {
                Id = Guid.NewGuid(),
                Date = datesToCreate[index],
                Name = name
            });
        }

        await _holidayRepository.AddRangeAsync(newHolidays);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dtos = newHolidays
            .Select(h => new HolidayDto(h.Id, h.Date, h.Name))
            .ToList();

        return Result.Success(new CreateHolidayResponse(dtos));
    }
}
