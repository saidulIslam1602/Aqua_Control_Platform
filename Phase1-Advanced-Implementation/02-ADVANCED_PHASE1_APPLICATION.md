# Phase 1 - Step 2: Application Layer (CQRS) - Business Logic Layer

## 🎯 Application Layer Architecture

The Application Layer implements **CQRS (Command Query Responsibility Segregation)**, **MediatR** for decoupling, and **Clean Architecture** principles used by companies like Microsoft, Amazon, and Netflix.

### CQRS Pattern
```
┌─────────────────────────────────────────────────────────────┐
│                    Command Side (Write)                      │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │    Commands     │→ │   Handlers      │→ │  Domain      │ │
│  │   (Intent)      │  │  (Business)     │  │  Models      │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Query Side (Read)                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │     Queries     │→ │   Handlers      │→ │  Read        │ │
│  │   (Request)     │  │  (Data Access)  │  │  Models      │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 📁 Application Layer Structure

> **Note:** All files are in the **backend** (`backend/src/AquaControl.Application/`).  
> Files are listed in **dependency order** - create them in this sequence to avoid compilation errors.

---

### File 1: Application Project File (Create First)
**File:** `backend/src/AquaControl.Application/AquaControl.Application.csproj`  
**Dependencies:** None - Must be created first

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="FluentValidation" Version="11.9.0" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\AquaControl.Domain\AquaControl.Domain.csproj" />
  </ItemGroup>

</Project>
```

---

### File 2: Error Model (No Dependencies)
**File:** `backend/src/AquaControl.Application/Common/Models/Error.cs`  
**Dependencies:** None - Create this first among models

```csharp
namespace AquaControl.Application.Common.Models;

public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Validation);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);
}

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Failure = 4,
    Unauthorized = 5,
    Forbidden = 6
}
```

---

### File 3: Result Pattern (Depends on Error)
**File:** `backend/src/AquaControl.Application/Common/Models/Result.cs`  
**Dependencies:** Error.cs (File 2) - Create after Error.cs

```csharp
using System.Diagnostics.CodeAnalysis;

namespace AquaControl.Application.Common.Models;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
```

---

### File 4: Paged Result Model (No Dependencies)
**File:** `backend/src/AquaControl.Application/Common/Models/PagedResult.cs`  
**Dependencies:** None - Standalone class

```csharp
namespace AquaControl.Application.Common.Models;

public sealed class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

---

### File 5: Unit of Work Interface (No Dependencies)
**File:** `backend/src/AquaControl.Application/Common/Interfaces/IUnitOfWork.cs`  
**Dependencies:** None

```csharp
namespace AquaControl.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

---

### File 6: Command Interface (Depends on Result)
**File:** `backend/src/AquaControl.Application/Common/Interfaces/ICommand.cs`  
**Dependencies:** Result.cs (File 3), MediatR

```csharp
using MediatR;

namespace AquaControl.Application.Common.Interfaces;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
```

---

### File 7: Query Interface (Depends on Result)
**File:** `backend/src/AquaControl.Application/Common/Interfaces/IQuery.cs`  
**Dependencies:** Result.cs (File 3), MediatR

```csharp
using MediatR;

namespace AquaControl.Application.Common.Interfaces;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
```

---

### File 8: DTOs (No Dependencies)
**File:** `backend/src/AquaControl.Application/Features/Tanks/DTOs/TankDto.cs`  
**Dependencies:** None - Standalone DTOs

```csharp
namespace AquaControl.Application.Features.Tanks.DTOs;

public sealed class TankDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Capacity { get; init; }
    public string CapacityUnit { get; init; } = string.Empty;
    public LocationDto Location { get; init; } = new();
    public string TankType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int SensorCount { get; init; }
    public int ActiveSensorCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? LastMaintenanceDate { get; init; }
    public DateTime? NextMaintenanceDate { get; init; }
    public bool IsMaintenanceDue { get; init; }
    public List<SensorDto> Sensors { get; init; } = new();
}

public sealed class LocationDto
{
    public string Building { get; init; } = string.Empty;
    public string Room { get; init; } = string.Empty;
    public string? Zone { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string FullAddress { get; init; } = string.Empty;
}

public sealed class SensorDto
{
    public Guid Id { get; init; }
    public string SensorType { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Manufacturer { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public decimal Accuracy { get; init; }
    public DateTime InstallationDate { get; init; }
    public DateTime? CalibrationDate { get; init; }
    public DateTime? NextCalibrationDate { get; init; }
    public bool IsCalibrationDue { get; init; }
}
```

---

### File 9: Repository Interface (Depends on Domain Layer)
**File:** `backend/src/AquaControl.Application/Common/Interfaces/ITankRepository.cs`  
**Dependencies:** Domain Layer (Tank, TankId, ISpecification) - **Note:** Create after Domain layer exists

```csharp
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.Common;

namespace AquaControl.Application.Common.Interfaces;

public interface ITankRepository
{
    Task<Tank?> GetByIdAsync(TankId id, CancellationToken cancellationToken = default);
    Task<Tank?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Tank> Tanks, int TotalCount)> GetPagedAsync(
        ISpecification<Tank> specification,
        int page,
        int pageSize,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
    Task AddAsync(Tank tank, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tank tank, CancellationToken cancellationToken = default);
    Task DeleteAsync(Tank tank, CancellationToken cancellationToken = default);
}
```

---

### File 10: Specification Pattern (Depends on Domain Layer)
**File:** `backend/src/AquaControl.Application/Common/Specifications/TankFilterSpecification.cs`  
**Dependencies:** Domain Layer, ISpecification - **Note:** Create after Domain layer exists

```csharp
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.Common;
using AquaControl.Domain.Enums;
using System.Linq.Expressions;

namespace AquaControl.Application.Common.Specifications;

public sealed class TankFilterSpecification : ISpecification<Tank>
{
    private readonly string? _searchTerm;
    private readonly TankType? _tankType;
    private readonly bool? _isActive;

    public TankFilterSpecification(string? searchTerm, TankType? tankType, bool? isActive)
    {
        _searchTerm = searchTerm;
        _tankType = tankType;
        _isActive = isActive;
    }

    public Expression<Func<Tank, bool>> ToExpression()
    {
        Expression<Func<Tank, bool>> expression = tank => true;

        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            var searchLower = _searchTerm.ToLowerInvariant();
            expression = expression.And(tank =>
                tank.Name.ToLower().Contains(searchLower) ||
                tank.Location.Building.ToLower().Contains(searchLower) ||
                tank.Location.Room.ToLower().Contains(searchLower) ||
                (tank.Location.Zone != null && tank.Location.Zone.ToLower().Contains(searchLower)));
        }

        if (_tankType.HasValue)
        {
            expression = expression.And(tank => tank.TankType == _tankType.Value);
        }

        if (_isActive.HasValue)
        {
            var targetStatus = _isActive.Value ? TankStatus.Active : TankStatus.Inactive;
            expression = expression.And(tank => tank.Status == targetStatus);
        }

        return expression;
    }
}
```

---

### File 11: Validation Behavior (Depends on Result & FluentValidation)
**File:** `backend/src/AquaControl.Application/Common/Behaviors/ValidationBehavior.cs`  
**Dependencies:** Result.cs (File 3), FluentValidation, MediatR

```csharp
using FluentValidation;
using MediatR;
using AquaControl.Application.Common.Models;

namespace AquaControl.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var error = Error.Validation(
                "Validation.Failed",
                string.Join("; ", failures.Select(f => f.ErrorMessage)));

            return CreateValidationResult<TResponse>(error);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(Error error)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Failure(error) as TResult)!;
        }

        object validationResult = typeof(Result<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
            .GetMethod(nameof(Result.Failure))!
            .Invoke(null, new object[] { error })!;

        return (TResult)validationResult;
    }
}
```

---

### File 12: Logging Behavior (Depends on MediatR)
**File:** `backend/src/AquaControl.Application/Common/Behaviors/LoggingBehavior.cs`  
**Dependencies:** MediatR, ILogger

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AquaControl.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();

        _logger.LogInformation(
            "Starting request {RequestName} with ID {RequestId}",
            requestName,
            requestId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Completed request {RequestName} with ID {RequestId} in {ElapsedMs}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Request {RequestName} with ID {RequestId} failed after {ElapsedMs}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
```

---

### File 13: Create Tank Command (Depends on ICommand & Domain)
**File:** `backend/src/AquaControl.Application/Features/Tanks/Commands/CreateTank/CreateTankCommand.cs`  
**Dependencies:** ICommand (File 6), Domain.Enums

```csharp
using AquaControl.Application.Common.Interfaces;
using AquaControl.Domain.Enums;

namespace AquaControl.Application.Features.Tanks.Commands.CreateTank;

public sealed record CreateTankCommand(
    string Name,
    decimal Capacity,
    string CapacityUnit,
    string Building,
    string Room,
    string? Zone,
    decimal? Latitude,
    decimal? Longitude,
    TankType TankType
) : ICommand<Guid>;
```

**File:** `src/AquaControl.Application/Features/Tanks/Commands/CreateTank/CreateTankCommandHandler.cs`
```csharp
using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.ValueObjects;
using AquaControl.Domain.Common;

namespace AquaControl.Application.Features.Tanks.Commands.CreateTank;

public sealed class CreateTankCommandHandler : ICommandHandler<CreateTankCommand, Guid>
{
    private readonly ITankRepository _tankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTankCommandHandler> _logger;

    public CreateTankCommandHandler(
        ITankRepository tankRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateTankCommandHandler> logger)
    {
        _tankRepository = tankRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTankCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating tank with name: {TankName}", request.Name);

        try
        {
            // Check if tank with same name already exists
            var existingTank = await _tankRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingTank != null)
            {
                return Error.Conflict(
                    "Tank.NameAlreadyExists",
                    $"Tank with name '{request.Name}' already exists");
            }

            // Create value objects
            var capacity = TankCapacity.Create(request.Capacity, request.CapacityUnit);
            var location = Location.Create(
                request.Building,
                request.Room,
                request.Zone,
                request.Latitude,
                request.Longitude);

            // Create tank aggregate
            var tank = Tank.Create(request.Name, capacity, location, request.TankType);

            // Save to repository
            await _tankRepository.AddAsync(tank, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tank created successfully with ID: {TankId}", tank.Id.Value);

            return Result.Success(tank.Id.Value);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating tank: {Error}", ex.Message);
            return Error.Validation("Tank.ValidationError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tank with name: {TankName}", request.Name);
            return Error.Failure("Tank.CreationFailed", "An error occurred while creating the tank");
        }
    }
}
```

**File:** `src/AquaControl.Application/Features/Tanks/Commands/CreateTank/CreateTankCommandValidator.cs`
```csharp
using FluentValidation;

namespace AquaControl.Application.Features.Tanks.Commands.CreateTank;

public sealed class CreateTankCommandValidator : AbstractValidator<CreateTankCommand>
{
    public CreateTankCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tank name is required")
            .MaximumLength(100).WithMessage("Tank name must not exceed 100 characters")
            .Matches("^[a-zA-Z0-9\\s\\-_]+$").WithMessage("Tank name contains invalid characters");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero")
            .LessThanOrEqualTo(1000000).WithMessage("Capacity cannot exceed 1,000,000 liters");

        RuleFor(x => x.CapacityUnit)
            .NotEmpty().WithMessage("Capacity unit is required")
            .Must(BeValidUnit).WithMessage("Invalid capacity unit. Allowed: L, ML, GAL");

        RuleFor(x => x.Building)
            .NotEmpty().WithMessage("Building is required")
            .MaximumLength(50).WithMessage("Building name must not exceed 50 characters");

        RuleFor(x => x.Room)
            .NotEmpty().WithMessage("Room is required")
            .MaximumLength(50).WithMessage("Room name must not exceed 50 characters");

        RuleFor(x => x.Zone)
            .MaximumLength(50).WithMessage("Zone name must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Zone));

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.TankType)
            .IsInEnum().WithMessage("Invalid tank type");
    }

    private static bool BeValidUnit(string unit)
    {
        var validUnits = new[] { "L", "ML", "GAL" };
        return validUnits.Contains(unit.ToUpperInvariant());
    }
}
```

---

### File 14: Create Tank Command Validator (Depends on CreateTankCommand)
**File:** `backend/src/AquaControl.Application/Features/Tanks/Commands/CreateTank/CreateTankCommandValidator.cs`  
**Dependencies:** FluentValidation, CreateTankCommand (File 13)

```csharp
using FluentValidation;

namespace AquaControl.Application.Features.Tanks.Commands.CreateTank;

public sealed class CreateTankCommandValidator : AbstractValidator<CreateTankCommand>
{
    public CreateTankCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tank name is required")
            .MaximumLength(100).WithMessage("Tank name must not exceed 100 characters")
            .Matches("^[a-zA-Z0-9\\s\\-_]+$").WithMessage("Tank name contains invalid characters");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero")
            .LessThanOrEqualTo(1000000).WithMessage("Capacity cannot exceed 1,000,000 liters");

        RuleFor(x => x.CapacityUnit)
            .NotEmpty().WithMessage("Capacity unit is required")
            .Must(BeValidUnit).WithMessage("Invalid capacity unit. Allowed: L, ML, GAL");

        RuleFor(x => x.Building)
            .NotEmpty().WithMessage("Building is required")
            .MaximumLength(50).WithMessage("Building name must not exceed 50 characters");

        RuleFor(x => x.Room)
            .NotEmpty().WithMessage("Room is required")
            .MaximumLength(50).WithMessage("Room name must not exceed 50 characters");

        RuleFor(x => x.Zone)
            .MaximumLength(50).WithMessage("Zone name must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Zone));

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.TankType)
            .IsInEnum().WithMessage("Invalid tank type");
    }

    private static bool BeValidUnit(string unit)
    {
        var validUnits = new[] { "L", "ML", "GAL" };
        return validUnits.Contains(unit.ToUpperInvariant());
    }
}
```

---

### File 15: Create Tank Command Handler (Depends on Everything)
**File:** `backend/src/AquaControl.Application/Features/Tanks/Commands/CreateTank/CreateTankCommandHandler.cs`  
**Dependencies:** ICommandHandler, Result, Domain, ITankRepository, IUnitOfWork

```csharp
using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.ValueObjects;
using AquaControl.Domain.Common;

namespace AquaControl.Application.Features.Tanks.Commands.CreateTank;

public sealed class CreateTankCommandHandler : ICommandHandler<CreateTankCommand, Guid>
{
    private readonly ITankRepository _tankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTankCommandHandler> _logger;

    public CreateTankCommandHandler(
        ITankRepository tankRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateTankCommandHandler> logger)
    {
        _tankRepository = tankRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTankCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating tank with name: {TankName}", request.Name);

        try
        {
            // Check if tank with same name already exists
            var existingTank = await _tankRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingTank != null)
            {
                return Error.Conflict(
                    "Tank.NameAlreadyExists",
                    $"Tank with name '{request.Name}' already exists");
            }

            // Create value objects
            var capacity = TankCapacity.Create(request.Capacity, request.CapacityUnit);
            var location = Location.Create(
                request.Building,
                request.Room,
                request.Zone,
                request.Latitude,
                request.Longitude);

            // Create tank aggregate
            var tank = Tank.Create(request.Name, capacity, location, request.TankType);

            // Save to repository
            await _tankRepository.AddAsync(tank, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tank created successfully with ID: {TankId}", tank.Id.Value);

            return Result.Success(tank.Id.Value);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating tank: {Error}", ex.Message);
            return Error.Validation("Tank.ValidationError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tank with name: {TankName}", request.Name);
            return Error.Failure("Tank.CreationFailed", "An error occurred while creating the tank");
        }
    }
}
```

---

### File 16: Update Tank Command (Depends on ICommand & Domain)
**File:** `backend/src/AquaControl.Application/Features/Tanks/Commands/UpdateTank/UpdateTankCommand.cs`  
**Dependencies:** ICommand (File 6), Domain.Enums

```csharp
using AquaControl.Application.Common.Interfaces;
using AquaControl.Domain.Enums;

namespace AquaControl.Application.Features.Tanks.Commands.UpdateTank;

public sealed record UpdateTankCommand(
    Guid TankId,
    string Name,
    decimal Capacity,
    string CapacityUnit,
    string Building,
    string Room,
    string? Zone,
    decimal? Latitude,
    decimal? Longitude,
    TankType TankType
) : ICommand;
```

---

### File 17: Update Tank Command Handler (Depends on Everything)
**File:** `backend/src/AquaControl.Application/Features/Tanks/Commands/UpdateTank/UpdateTankCommandHandler.cs`  
**Dependencies:** ICommandHandler, Result, Domain, ITankRepository, IUnitOfWork

```csharp
using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.ValueObjects;
using AquaControl.Domain.Common;

namespace AquaControl.Application.Features.Tanks.Commands.UpdateTank;

public sealed class UpdateTankCommandHandler : ICommandHandler<UpdateTankCommand>
{
    private readonly ITankRepository _tankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTankCommandHandler> _logger;

    public UpdateTankCommandHandler(
        ITankRepository tankRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateTankCommandHandler> logger)
    {
        _tankRepository = tankRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTankCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating tank with ID: {TankId}", request.TankId);

        try
        {
            // Get existing tank
            var tank = await _tankRepository.GetByIdAsync(TankId.Create(request.TankId), cancellationToken);
            if (tank == null)
            {
                return Error.NotFound("Tank.NotFound", $"Tank with ID '{request.TankId}' was not found");
            }

            // Check if another tank with same name exists
            var existingTankWithName = await _tankRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingTankWithName != null && existingTankWithName.Id != tank.Id)
            {
                return Error.Conflict(
                    "Tank.NameAlreadyExists",
                    $"Another tank with name '{request.Name}' already exists");
            }

            // Create value objects
            var capacity = TankCapacity.Create(request.Capacity, request.CapacityUnit);
            var location = Location.Create(
                request.Building,
                request.Room,
                request.Zone,
                request.Latitude,
                request.Longitude);

            // Update tank properties
            tank.UpdateName(request.Name);
            tank.UpdateCapacity(capacity);
            tank.Relocate(location);

            // Save changes
            await _tankRepository.UpdateAsync(tank, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tank updated successfully: {TankId}", request.TankId);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating tank: {Error}", ex.Message);
            return Error.Validation("Tank.ValidationError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tank: {TankId}", request.TankId);
            return Error.Failure("Tank.UpdateFailed", "An error occurred while updating the tank");
        }
    }
}
```

---

### File 18: Get Tanks Query (Depends on IQuery, PagedResult, TankDto)
**File:** `backend/src/AquaControl.Application/Features/Tanks/Queries/GetTanks/GetTanksQuery.cs`  
**Dependencies:** IQuery (File 7), PagedResult (File 4), TankDto (File 8)

```csharp
using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;

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
```

---

### File 19: Get Tanks Query Handler (Depends on Everything)
**File:** `backend/src/AquaControl.Application/Features/Tanks/Queries/GetTanks/GetTanksQueryHandler.cs`  
**Dependencies:** IQueryHandler, Result, Domain, ITankRepository, TankFilterSpecification, TankDto, PagedResult

```csharp
using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Application.Common.Specifications;
using AquaControl.Application.Features.Tanks.DTOs;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.Enums;

namespace AquaControl.Application.Features.Tanks.Queries.GetTanks;

public sealed class GetTanksQueryHandler : IQueryHandler<GetTanksQuery, PagedResult<TankDto>>
{
    private readonly ITankRepository _tankRepository;
    private readonly ILogger<GetTanksQueryHandler> _logger;

    public GetTanksQueryHandler(ITankRepository tankRepository, ILogger<GetTanksQueryHandler> logger)
    {
        _tankRepository = tankRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<TankDto>>> Handle(GetTanksQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tanks with filters: {@Filters}", request);

        try
        {
            var specification = new TankFilterSpecification(
                request.SearchTerm,
                ParseTankType(request.TankType),
                request.IsActive);

            var (tanks, totalCount) = await _tankRepository.GetPagedAsync(
                specification,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortDescending,
                cancellationToken);

            var tankDtos = tanks.Select(tank => new TankDto
            {
                Id = tank.Id.Value,
                Name = tank.Name,
                Capacity = tank.Capacity.Value,
                CapacityUnit = tank.Capacity.Unit,
                Location = new LocationDto
                {
                    Building = tank.Location.Building,
                    Room = tank.Location.Room,
                    Zone = tank.Location.Zone,
                    Latitude = tank.Location.Latitude,
                    Longitude = tank.Location.Longitude,
                    FullAddress = tank.Location.GetFullAddress()
                },
                TankType = tank.TankType.ToString(),
                Status = tank.Status.ToString(),
                IsActive = tank.Status == TankStatus.Active,
                SensorCount = tank.Sensors.Count,
                ActiveSensorCount = tank.GetActiveSensors().Count(),
                CreatedAt = tank.CreatedAt,
                UpdatedAt = tank.UpdatedAt,
                LastMaintenanceDate = tank.LastMaintenanceDate,
                NextMaintenanceDate = tank.NextMaintenanceDate,
                IsMaintenanceDue = tank.IsMaintenanceDue()
            }).ToList();

            var result = new PagedResult<TankDto>
            {
                Items = tankDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tanks");
            return Error.Failure("Tank.GetFailed", "An error occurred while retrieving tanks");
        }
    }

    private static TankType? ParseTankType(string? tankType)
    {
        if (string.IsNullOrEmpty(tankType))
            return null;

        return Enum.TryParse<TankType>(tankType, true, out var result) ? result : null;
    }
}
```

---

### File 20: Domain Event Handler (Depends on MediatR & Domain Events)
**File:** `backend/src/AquaControl.Application/Features/Tanks/EventHandlers/TankCreatedEventHandler.cs`  
**Dependencies:** MediatR, Domain.Events

```csharp
using MediatR;
using AquaControl.Domain.Events;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.EventHandlers;

public sealed class TankCreatedEventHandler : INotificationHandler<TankCreatedEvent>
{
    private readonly ILogger<TankCreatedEventHandler> _logger;

    public TankCreatedEventHandler(ILogger<TankCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TankCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Tank created: {TankId} - {TankName} at {Location}",
            notification.TankId,
            notification.Name,
            notification.Location.GetFullAddress());

        // Additional business logic:
        // - Send notifications
        // - Update read models
        // - Trigger integrations
        // - Audit logging

        await Task.CompletedTask;
    }
}
```

---

## 📋 Summary

This completes the sophisticated Application Layer with:

✅ **CQRS Pattern** - Separate commands and queries  
✅ **MediatR Integration** - Decoupled request/response handling  
✅ **Result Pattern** - Functional error handling  
✅ **Validation Pipeline** - FluentValidation with MediatR behaviors  
✅ **Specification Pattern** - Flexible query building  
✅ **Domain Event Handling** - Event-driven architecture  
✅ **Logging & Monitoring** - Comprehensive request logging  
✅ **Clean Architecture** - Proper dependency management  

Next, I'll create the Infrastructure Layer with Entity Framework, Event Store, and AWS integrations.
