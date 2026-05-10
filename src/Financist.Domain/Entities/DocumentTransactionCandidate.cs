using Financist.Domain.Common;
using Financist.Domain.Enums;
using Financist.Domain.ValueObjects;

namespace Financist.Domain.Entities;

public sealed class DocumentTransactionCandidate : AuditableEntity
{
    private DocumentTransactionCandidate()
    {
        Description = string.Empty;
        Amount = Money.Zero();
        Currency = string.Empty;
        RawText = string.Empty;
        ImportFingerprint = string.Empty;
    }

    private DocumentTransactionCandidate(
        Guid documentImportId,
        Guid userId,
        string description,
        Money amount,
        TransactionType type,
        DateOnly occurredOn,
        string rawText,
        decimal confidence,
        string importFingerprint,
        int? installmentNumber,
        int? installmentCount,
        string? installmentGroupKey)
    {
        if (documentImportId == Guid.Empty)
        {
            throw new DomainException("Document import is required.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("Document transaction candidate user is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Candidate description is required.");
        }

        if (!amount.GreaterThanZero())
        {
            throw new DomainException("Candidate amount must be greater than zero.");
        }

        if (type is not TransactionType.Income and not TransactionType.Expense)
        {
            throw new DomainException("Candidate transaction type must be Income or Expense.");
        }

        if (string.IsNullOrWhiteSpace(rawText))
        {
            throw new DomainException("Candidate raw text is required.");
        }

        if (confidence is < 0 or > 1)
        {
            throw new DomainException("Candidate confidence must be between zero and one.");
        }

        if (string.IsNullOrWhiteSpace(importFingerprint))
        {
            throw new DomainException("Candidate import fingerprint is required.");
        }

        if (installmentNumber.HasValue != installmentCount.HasValue)
        {
            throw new DomainException("Candidate installment number and count must be provided together.");
        }

        if (installmentNumber.HasValue &&
            (installmentNumber <= 0 || installmentCount <= 0 || installmentNumber > installmentCount))
        {
            throw new DomainException("Candidate installment information is invalid.");
        }

        DocumentImportId = documentImportId;
        UserId = userId;
        Description = description.Trim();
        Amount = amount;
        Currency = amount.Currency;
        Type = type;
        OccurredOn = occurredOn;
        RawText = rawText.Trim();
        Confidence = confidence;
        ImportFingerprint = importFingerprint.Trim();
        InstallmentNumber = installmentNumber;
        InstallmentCount = installmentCount;
        InstallmentGroupKey = string.IsNullOrWhiteSpace(installmentGroupKey) ? null : installmentGroupKey.Trim();
        Status = DocumentTransactionCandidateStatus.PendingReview;
    }

    public Guid DocumentImportId { get; private set; }

    public Guid UserId { get; private set; }

    public string Description { get; private set; }

    public Money Amount { get; private set; }

    public string Currency { get; private set; }

    public TransactionType Type { get; private set; }

    public DateOnly OccurredOn { get; private set; }

    public string RawText { get; private set; }

    public decimal Confidence { get; private set; }

    public int? InstallmentNumber { get; private set; }

    public int? InstallmentCount { get; private set; }

    public string? InstallmentGroupKey { get; private set; }

    public string ImportFingerprint { get; private set; }

    public DocumentTransactionCandidateStatus Status { get; private set; }

    public Guid? TransactionId { get; private set; }

    public DateTime? ImportedAtUtc { get; private set; }

    public DocumentImport? DocumentImport { get; private set; }

    public User? User { get; private set; }

    public Transaction? Transaction { get; private set; }

    public static DocumentTransactionCandidate Create(
        Guid documentImportId,
        Guid userId,
        string description,
        Money amount,
        TransactionType type,
        DateOnly occurredOn,
        string rawText,
        decimal confidence,
        string importFingerprint,
        int? installmentNumber,
        int? installmentCount,
        string? installmentGroupKey)
    {
        return new DocumentTransactionCandidate(
            documentImportId,
            userId,
            description,
            amount,
            type,
            occurredOn,
            rawText,
            confidence,
            importFingerprint,
            installmentNumber,
            installmentCount,
            installmentGroupKey);
    }

    public void ApplyReview(
        string description,
        Money amount,
        TransactionType type,
        DateOnly occurredOn)
    {
        if (Status != DocumentTransactionCandidateStatus.PendingReview)
        {
            throw new DomainException($"Only pending candidates can be edited. Current status: {Status}.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Candidate description is required.");
        }

        if (!amount.GreaterThanZero())
        {
            throw new DomainException("Candidate amount must be greater than zero.");
        }

        if (type is not TransactionType.Income and not TransactionType.Expense)
        {
            throw new DomainException("Candidate transaction type must be Income or Expense.");
        }

        Description = description.Trim();
        Amount = amount;
        Currency = amount.Currency;
        Type = type;
        OccurredOn = occurredOn;
        Touch();
    }

    public void MarkImported(Guid transactionId)
    {
        if (transactionId == Guid.Empty)
        {
            throw new DomainException("Imported transaction is required.");
        }

        if (Status != DocumentTransactionCandidateStatus.PendingReview)
        {
            throw new DomainException($"Only pending candidates can be imported. Current status: {Status}.");
        }

        Status = DocumentTransactionCandidateStatus.Imported;
        TransactionId = transactionId;
        ImportedAtUtc = DateTime.UtcNow;
        Touch();
    }

    public void Reject()
    {
        if (Status != DocumentTransactionCandidateStatus.PendingReview)
        {
            throw new DomainException($"Only pending candidates can be rejected. Current status: {Status}.");
        }

        Status = DocumentTransactionCandidateStatus.Rejected;
        Touch();
    }
}
