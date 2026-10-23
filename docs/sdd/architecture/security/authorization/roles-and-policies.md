# Authorization

## Baseline rule

`Host.Api` configures a fallback authorization policy requiring an authenticated user. An endpoint is therefore protected unless it explicitly declares `[AllowAnonymous]`. Anonymous access must be explicit in the API contract and verified as part of the feature.

Authentication answers **who is calling**. Authorization answers **whether that caller may perform the operation**.

## Roles

Canonical role constants are in `Identity.Domain/Authorization/RoleNames.cs`; inherited code also contains `Identity.Domain/Constants/AppRoles.cs` with the same set of values.

| Role | Current meaning | Created by seeding |
| --- | --- | --- |
| `Admin` | System administration | yes |
| `Manager` | Content manager access | yes |
| `User` | Regular authenticated user | yes |
| `Guest` | Role constant defined without an agreed business meaning | no |

At startup, `RoleSeeder` currently creates only Admin, Manager, and User. Do not assign `Guest` until its seeding and business meaning are deliberately added.

## Policies

Policy names are in `Identity.Application/Security/Policies/PolicyNames.cs`; their role requirements are composed in `Host.Api/AuthorizationPoliciesRegistrationExtensions`.

| Policy | Allowed roles |
| --- | --- |
| `RequireAdminRole` / `CanManageUsers` | Admin |
| `RequireManagerRole` / `CanManageProducts` / `CanManageOrders` | Manager, Admin |
| `CatalogRead` / `OrderCreate` | User, Manager, Admin |
| `RequireTenantAccess` | Constant is defined; policy is not currently registered |

Do not use `RequireTenantAccess` on an endpoint until it is registered in Host and its policy semantics are agreed.

## Protecting an endpoint

Prefer an existing named policy that expresses the operation. The controller declares access only; the command or query still goes through `ISender`, and business invariants remain in Domain/Application. Do not use a role check in a controller instead of a policy.

For a public endpoint, use `[AllowAnonymous]` and record the reason in the feature contract. This overrides the fallback policy.

## Adding a policy or role

1. Check whether an existing policy already covers the business operation.
2. If not, define the policy name in Identity Application and its role requirement in Host.Api.
3. If a new role is needed, add its constant in Identity Domain and deliberately configure seeding; define which existing and future users receive it.
4. Protect the endpoint with the new policy, update the API contract, and manually verify denied and allowed requests.

Adding a role is an authorization change, not just a string change: it affects seeding, user assignment, documentation, and deployment.
