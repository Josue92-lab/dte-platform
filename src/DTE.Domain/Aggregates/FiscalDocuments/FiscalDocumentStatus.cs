namespace DTE.Domain.Aggregates.FiscalDocuments;

public enum FiscalDocumentStatus
{
    Draft = 1,
    Validated = 2,
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Signed is the correct fiscal document lifecycle state name.")]
    Signed = 3,
    Processed = 4,
    Rejected = 5
}
