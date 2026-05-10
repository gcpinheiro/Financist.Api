# PDF Transaction Import

## Purpose

The document import feature turns readable PDF statements into transaction
candidates. A candidate is not a real transaction until the user reviews it and
imports it.

This two-step flow is intentional:

- PDF parsing can be imperfect.
- Users need to edit dates, descriptions, values, currency, or type.
- Users need to reject false positives.
- Installment purchases must not overwrite or hide future installments from
  later statements.

## End-to-End Flow

1. The frontend uploads one or more PDFs with `POST /api/v1/documents/upload`.
2. `DocumentService.UploadAsync` stores the file and creates a `DocumentImport`.
3. `IndexDocumentAsync` extracts readable PDF text.
4. The extracted text is split into `DocumentChunk` records for RAG.
5. `DocumentTransactionExtractionService` scans the same text for transaction
   lines.
6. Extracted lines become `DocumentTransactionCandidate` records with
   `PendingReview` status.
7. The frontend lists candidates for the selected document.
8. The user can edit, import, or reject each pending candidate.
9. Importing creates a `Transaction` and marks the candidate as `Imported`.
10. Rejecting marks the candidate as `Rejected` and does not create a
    transaction.

## Main Code Map

Backend:

- `src/Financist.Api/Controllers/DocumentsController.cs`
- `src/Financist.Application/Features/Documents/DocumentService.cs`
- `src/Financist.Application/Features/Documents/DocumentTransactionExtractionService.cs`
- `src/Financist.Domain/Entities/DocumentTransactionCandidate.cs`
- `src/Financist.Domain/Enums/DocumentTransactionCandidateStatus.cs`
- `src/Financist.Infrastructure/Persistence/Configurations/DocumentTransactionCandidateConfiguration.cs`
- `src/Financist.Infrastructure/Persistence/Repositories/DocumentTransactionCandidateRepository.cs`
- `tests/Financist.UnitTests/Application/DocumentTransactionExtractionServiceTests.cs`

Frontend:

- `../Financist.Web/src/app/features/documents/documents.service.ts`
- `../Financist.Web/src/app/features/documents/pages/documents-page.component.ts`
- `../Financist.Web/src/app/features/documents/pages/documents-page.component.html`
- `../Financist.Web/src/app/shared/models/document-import.model.ts`
- `../Financist.Web/src/app/features/transactions/transactions.service.ts`

## Candidate Lifecycle

`DocumentTransactionCandidateStatus` has three states:

- `PendingReview`: candidate was extracted and is waiting for user action.
- `Imported`: candidate was converted into a `Transaction`.
- `Rejected`: candidate was reviewed and intentionally ignored.

Only `PendingReview` candidates can be edited, imported, or rejected. Trying to
import or reject a non-pending candidate returns a conflict from the application
service.

## Import Behavior

Importing a candidate accepts a review payload:

- `description`
- `amount`
- `currency`
- `type`
- `occurredOn`

The backend applies the review to the candidate first, then creates a
`Transaction` with the reviewed values. The created transaction receives notes
that include:

- source document import id
- installment number/count when present
- raw PDF line used as source

This gives future debugging a trace back to the document candidate.

## Installment Rules

PDF statements may contain installment purchases, for example:

```text
12/04 Loja Exemplo 02/10 R$ 123,45
12/05 Loja Exemplo 03/10 R$ 123,45
```

These must be treated as different candidates and eventually different
transactions. The implementation therefore stores both:

- `InstallmentNumber`: current installment number, such as `2`.
- `InstallmentCount`: total installments, such as `10`.
- `InstallmentGroupKey`: stable grouping key for installments from the same
  purchase.
- `ImportFingerprint`: unique key for the specific imported candidate.

The fingerprint includes the installment number and count. That means `02/10`
and `03/10` do not collide even when description, amount, currency, and type are
the same.

## Deduplication Rules

During extraction, the API checks candidate fingerprints already stored for the
same user. If a fingerprint already exists, the API skips creating a duplicate
candidate.

Fingerprint inputs include:

- version marker: `transaction-candidate-v1`
- transaction type
- occurred date
- currency
- absolute amount
- normalized description with installment marker removed
- installment number/count, or `single`
- occurrence number inside the same PDF

The occurrence number protects repeated equal lines in the same statement. For
example, two identical purchases on the same date can still become separate
candidates.

## Extraction Heuristics

`DocumentTransactionExtractionService` currently uses regular expressions and
line-based parsing. It expects a line with:

- a date near the beginning
- a description after the date
- an amount at the end

Supported examples include Brazilian formats such as:

```text
10/04 Mercado Central R$ 184,90
12/04 Loja Exemplo 03/10 R$ 123,45
15/04 Cashback 10,00 CR
```

Type detection:

- negative sign means `Expense`
- `CR` suffix means `Income`
- terms such as cashback, credito, estorno, pagamento, and reembolso mean
  `Income`
- otherwise the line defaults to `Expense`

Confidence:

- installment candidates currently receive higher confidence than plain
  candidates because the line has an extra recognized structure.

## Known Limitations

- Scanned PDFs without embedded text still need OCR before this parser can read
  them.
- Bank-specific layouts may require adapters or stronger parsing rules.
- Categories and cards are not assigned during import yet.
- The transaction is created only when the user imports the candidate.
- Rejected candidates remain stored as audit/history, not deleted.

## Safe Extension Points

- Add bank-specific parsers behind `IDocumentTransactionExtractionService`.
- Add category/card suggestion before import without changing the candidate
  lifecycle.
- Add OCR inside `IDocumentTextExtractionService`.
- Add richer duplicate review UI by grouping on `InstallmentGroupKey`.

When changing fingerprint rules, add tests first. Fingerprint behavior is part
of the data contract and affects idempotency.
