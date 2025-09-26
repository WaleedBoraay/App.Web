namespace App.Web.Api.DTOs;

public record StatusLogDto(
    int OldStatusId,
    int NewStatusId,
    int ChangedByUserId,
    DateTime ChangedOnUtc,
    string? Remarks
);
