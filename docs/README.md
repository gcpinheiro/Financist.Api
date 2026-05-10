# Financist Docs

This folder documents implementation decisions that are important for future
development. The goal is to make the codebase easy to resume for both humans
and Codex sessions.

## Current Documents

- [PDF transaction import](pdf-transaction-import.md)
  End-to-end flow from PDF upload to reviewed transaction creation.
- [Document transaction API contracts](document-transaction-api-contracts.md)
  HTTP endpoints, DTO shapes, statuses, and frontend expectations.
- [Development and verification](development-and-verification.md)
  Commands, Docker notes, migrations, and regression checks for this feature.

## Reading Order

1. Start with `pdf-transaction-import.md` to understand the business flow.
2. Read `document-transaction-api-contracts.md` before changing frontend or API
   contracts.
3. Use `development-and-verification.md` before rebuilding Docker, changing EF
   mappings, or testing the import flow.

## Documentation Rules

- Keep docs close to the code that owns the behavior.
- Update docs in the same change that alters public API behavior, persistence
  shape, candidate status transitions, or installment/fingerprint rules.
- Prefer short examples and exact file paths over broad explanations.
