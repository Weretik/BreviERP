using Reference.Domain.Suppliers;

namespace Reference.Application.Abstractions;

/// <summary>KeepinCRM is not configured or its answer cannot be used; nothing was changed.</summary>
public sealed class KeepinCrmUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
