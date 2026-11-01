using BuildingBlocks.Api.Result;
using Mediator;
using Reminders.Api.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reminders.Application.Calendar.AddNonWorkingDay;
using Reminders.Application.Calendar.GetNonWorkingDays;
using Reminders.Application.Calendar.RemoveNonWorkingDay;
using Reminders.Application.Contracts;

namespace Reminders.Api.Calendar;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/reminders/non-working-days")]
public sealed class NonWorkingDaysController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getReminderNonWorkingDays")]
    public async Task<ActionResult<IReadOnlyList<NonWorkingDayResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetNonWorkingDaysQuery(), cancellationToken));

    [HttpPost(Name = "addReminderNonWorkingDay")]
    public async Task<ActionResult<NonWorkingDayResponse>> Add(NonWorkingDayInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new AddNonWorkingDayCommand(input), cancellationToken));

    [HttpDelete("{date}", Name = "removeReminderNonWorkingDay")]
    public async Task<ActionResult> Remove(DateOnly date, CancellationToken cancellationToken)
    =>
        this.ToNoContentResult(await sender.Send(new RemoveNonWorkingDayCommand(date), cancellationToken));
}
