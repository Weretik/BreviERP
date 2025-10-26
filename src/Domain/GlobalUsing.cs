// System namespaces
global using System;
global using System.Linq.Expressions;
global using Microsoft.EntityFrameworkCore;

global using Ardalis.GuardClauses;
global using NodaMoney;
global using Ardalis.SmartEnum;
global using StronglyTypedIds;

// Domain namespaces
global using Domain.Common.Abstractions;
global using Domain.Common.Entity;
global using Domain.Common.ValueObject;
global using Domain.Accounting.ValueObjects;

[assembly:StronglyTypedIdDefaults(Template.Int, "int-efcore")]

