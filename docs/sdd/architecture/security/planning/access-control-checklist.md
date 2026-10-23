# Access Control Feature Checklist

Use this checklist in the feature folder alongside the [project specifications](../../../specs/README.md).

## Decision Path

```text
Does an existing policy cover the operation?
  | yes → protect the endpoint with it; do not add a role or new user type
  |
  no
  ▼
Is this a new business permission or a new user type?
  | permission → add a named policy and map it to existing roles
  | user type → add a role, deliberately create and assign it, then add a policy if needed
  |
  ▼
Can an anonymous caller create this user?
  | no  → an admin creation, invitation, or import feature
  | yes → a registration feature with verification and abuse protection
```

## Specification Checklist

- [ ] Name the protected operation in business terms.
- [ ] Reuse an existing policy or document why a new one is needed.
- [ ] For a new policy: name it in `PolicyNames` and define role requirements in Host.Api.
- [ ] For a new role: add its constant, seeding and deployment plan, and assignment rules; do not leave the role declared but unavailable.
- [ ] State who can call the endpoint, and deliberately mark anonymous endpoints with `AllowAnonymous`.
- [ ] If users are created or assigned roles, define the functionality as an Identity module use case.
- [ ] Decide whether existing refresh sessions must be revoked after a role, password, or account status change.
- [ ] Add rate limits and safe error responses for anonymous endpoints or credential operations.
- [ ] Include `401 Unauthorized`, `403 Forbidden`, and success cases in the API contract and Swagger test plan.

## Manual Test Matrix

| Case | Expected result |
| --- | --- |
| No Bearer token on a protected route | `401` |
| Authenticated user without the required role | `403` |
| Required role and valid request | Success according to the contract |
| Explicitly anonymous endpoint | Works without a Bearer token only as allowed by its validation |
| Refresh without a CSRF value or with a mismatched value | `400` |

After changing roles or policies, verify a new login. Tokens contain role claims from the time they were issued, so separately check sessions created before the change if deployment requires an immediate effect.
