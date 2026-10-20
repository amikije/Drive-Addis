using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Domain.Entities;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public record RegisterCommand(
    string PhoneNumber,
    string Password,
    string FullName,
    UserRole Role,
    decimal? HourlyPrice,
    double? Latitude,
    double? Longitude,
    VehicleType? VehicleType,
    string? VehicleModel,
    string? PlateNumber,
    string? LicensePhotoUrl
) : IRequest<Result<AuthResponseDto>>;