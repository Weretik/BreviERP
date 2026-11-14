using System.Security.Cryptography;
using System.Text.Json;
using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

/// <summary>The Brevi item of an old one and its number of products.</summary>
internal sealed record V133Target(FabricMovementTarget Target, int Quantity);
