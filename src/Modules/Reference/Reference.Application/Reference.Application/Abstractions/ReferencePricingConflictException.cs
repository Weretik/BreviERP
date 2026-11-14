namespace Reference.Application.Abstractions;

/// <summary>
/// A lost race found while saving: the price changed meanwhile or has a future revision. Like
/// <c>DbUpdateConcurrencyException</c> it comes from <c>SaveChanges</c>, so it stays an exception; the handler answers
/// 409 with its Ukrainian message.
/// </summary>
public sealed class ReferencePricingConflictException(string message) : Exception(message);
