using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Application.Features.Tanks.DTOs;

namespace AquaControl.Application.Features.Tanks.Queries.GetTanks;

public sealed record GetTanksQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? TankType = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false
) : IQuery<PagedResult<TankDto>>;

