# Document Transaction API Contracts

Base path:

```text
/api/v1
```

All document endpoints require JWT authentication.

## List Document Imports

```http
GET /api/v1/documents
```

Returns the authenticated user's document imports.

Response item shape:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "storedFileName": "stored.pdf",
  "originalFileName": "statement.pdf",
  "contentType": "application/pdf",
  "sizeBytes": 12345,
  "status": "Completed",
  "uploadedAtUtc": "2026-04-29T21:00:00Z"
}
```

`status` values:

- `Pending`
- `Processing`
- `Completed`
- `Failed`

## Upload Document

```http
POST /api/v1/documents/upload
Content-Type: multipart/form-data
```

Form field:

- `File`: non-empty PDF or supported document file

Returns `201 Created` with a `DocumentImportDto`.

Important behavior:

- The API stores the file first.
- The same upload request extracts text, chunks content, extracts transaction
  candidates, and marks the import as completed or failed.
- Candidate creation is skipped when a matching fingerprint already exists for
  the user.

## List Transaction Candidates

```http
GET /api/v1/documents/{documentImportId}/transaction-candidates
```

Returns all candidates extracted for one document import.

Response item shape:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "documentImportId": "00000000-0000-0000-0000-000000000000",
  "description": "Loja Exemplo 03/10",
  "amount": 123.45,
  "currency": "BRL",
  "type": "Expense",
  "occurredOn": "2026-04-12",
  "rawText": "12/04 Loja Exemplo 03/10 R$ 123,45",
  "confidence": 0.92,
  "installmentNumber": 3,
  "installmentCount": 10,
  "installmentGroupKey": "hash",
  "importFingerprint": "hash",
  "status": "PendingReview",
  "transactionId": null
}
```

`status` values:

- `PendingReview`
- `Imported`
- `Rejected`

`type` values:

- `Income`
- `Expense`

## Import Transaction Candidate

```http
POST /api/v1/documents/{documentImportId}/transaction-candidates/{candidateId}/import
Content-Type: application/json
```

Request body:

```json
{
  "description": "Loja Exemplo",
  "amount": 123.45,
  "currency": "BRL",
  "type": "Expense",
  "occurredOn": "2026-04-12"
}
```

Returns `200 OK` with the updated candidate.

Important behavior:

- Only `PendingReview` candidates can be imported.
- The request body is treated as the user's reviewed version.
- The backend updates the candidate values before creating the transaction.
- The backend creates one `Transaction`.
- The candidate is marked as `Imported`.
- `transactionId` is set to the created transaction id.

Expected errors:

- `401`: missing or invalid JWT.
- `404`: document import or candidate does not belong to the user.
- `409`: candidate is already imported or rejected.
- Validation error: missing description, missing currency, or amount <= 0.

## Reject Transaction Candidate

```http
POST /api/v1/documents/{documentImportId}/transaction-candidates/{candidateId}/reject
```

Returns `200 OK` with the updated candidate.

Important behavior:

- Only `PendingReview` candidates can be rejected.
- No transaction is created.
- The candidate remains stored with `Rejected` status.

Expected errors:

- `401`: missing or invalid JWT.
- `404`: document import or candidate does not belong to the user.
- `409`: candidate is already imported or rejected.

## Frontend Contract Expectations

The frontend should:

- Upload PDFs through `/documents/upload`.
- Select a document and call `/transaction-candidates`.
- Show pending candidates with edit, import, and reject actions.
- Send edited values in the import request body.
- Refresh transaction state after successful import.
- Keep imported and rejected rows visible as reviewed history.

Relevant frontend files:

- `../Financist.Web/src/app/features/documents/documents.service.ts`
- `../Financist.Web/src/app/features/documents/pages/documents-page.component.ts`
- `../Financist.Web/src/app/shared/models/document-import.model.ts`
- `../Financist.Web/src/app/features/transactions/transactions.service.ts`
