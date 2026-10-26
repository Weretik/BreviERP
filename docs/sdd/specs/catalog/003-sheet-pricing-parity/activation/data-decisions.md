# Standing local catalogue decisions

Confirmed by the human in this task on 2026-10-26 for TS-008D:

- SKU **724**: preserve missing supplier data and unapproved draft; keep current pricing blocked. Change only after a later verified source/explicit decision.
- SKU **931**: preserve inactive status and its unapproved draft; exclude from activation. Change only after a later explicit decision.
- Use the reviewed 2026-10-25 frozen source for local activation. DR-C01 retains MOD-061's 4-minute manual operation and ordinary backend calculation; [details and recovery](review-decisions.md).

These are persistent exclusions/decisions, not temporary missing tasks and not special formulas in the calculation engine. Local activation is explicitly authorized; production deployment is not included. [Execution specification](execution.md).
