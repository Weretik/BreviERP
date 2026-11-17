# ADR-0018: Production deploys on every merge to the main repository

**Status:** accepted · **Date:** 2026-11-17 · **Decided by:** the owner (architecture audit of 2026-11-17, item OPS-01)

## Context

Until now production deployed from the deploy fork `pnevman-creator/brevi-api` ([ADR-0004](0004-keyless-cloud-run-delivery.md)): a change merged to `Weretik/BreviERP` reached Cloud Run only after someone pressed «Sync fork». The audit of 2026-11-17 found merged fixes (traces, the outbox fix, end-to-end test changes) waiting days for that step, so production differed from `master`. KedrStore already deploys on every merge.

## Options

1. **Keep the fork and sync it by hand.** Rejected: the step is easy to forget, and production drifts from `master`.
2. **Sync the fork automatically from a workflow.** Rejected: it needs a long-lived token with write access to another account's repository, and still has two places to look for deploys.
3. **Deploy from `Weretik/BreviERP` on every merge to `master`.** Chosen: the same scheme as KedrStore, one repository, no stored credential.

## Decision

- The `deploy` job of `.github/workflows/deploy-cloudrun.yml` runs on a push to `master` of `Weretik/BreviERP` after `verify` passes. In any other repository it is skipped.
- The Workload Identity Federation provider `github-oidc` (pool `github`, project `brevistoreapi`) accepts only tokens of `Weretik/BreviERP`, and that repository's principal set holds `roles/iam.workloadIdentityUser` on `github-deploy@brevistoreapi.iam.gserviceaccount.com`.
- The merge of a pull request is the production gate: branch protection on `master` requires a green CI before the merge.

## Consequences

- Every merge deploys: migrations in `brevierp-seed` run first, then the API. Migrations keep the previous release working (database rules), as before.
- The fork `pnevman-creator/brevi-api` no longer deploys and may stay as an archive; its Actions runs stop after `verify`.
- Rollback is a revert pull request, or redeploying the previous image in Cloud Run.

## Rollout

1. The owner changes the provider condition and adds the binding in Cloud Shell ([deploy guide](../../operations/deployment/cloud-run-deploy.md#switching-from-the-deploy-fork)).
2. Then this change is merged; its own merge is the first deploy from `Weretik/BreviERP`.
3. Check the Actions run, the `brevierp-seed` execution, the API revision and `GET /health/ready`.
