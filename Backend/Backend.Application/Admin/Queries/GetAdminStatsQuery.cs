using MediatR;
using Backend.Application.Admin.DTOs;

namespace Backend.Application.Admin.Queries;

public class GetAdminStatsQuery : IRequest<AdminStatsDto>
{
}