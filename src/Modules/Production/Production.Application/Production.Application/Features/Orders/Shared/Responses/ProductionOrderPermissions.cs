namespace Production.Application.Features.Orders.Shared.Responses;

/// <summary>What the signed-in person may do with the order and which prices they see.</summary>
/// <param name="CanTrack">Tick етапи and record shipments: Admin and the head of the workshop on a «Нове» order.</param>
/// <param name="CanClose">«Закрити»: may track and everything is shipped.</param>
/// <param name="CanReopen">«Повернути в роботу»: Admin, on a closed or cancelled order.</param>
public sealed record ProductionOrderPermissions(
    bool CanEdit, bool CanCopy, bool CanDelete, bool SeesSewingRate, bool SeesReportPrice,
    bool CanTrack, bool CanClose, bool CanCancel, bool CanReopen, bool CanManageFiles, bool CanSendTelegram);
