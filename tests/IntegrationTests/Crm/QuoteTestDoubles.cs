using Crm.Application.Quotes.Pdf;
using Crm.Application.Quotes.Pdf.Introduction;
using Crm.Application.Quotes.Staff;

namespace IntegrationTests.Crm;

/// <summary>Director (1, Admin) and the manager Олена (17).</summary>
internal sealed class QuoteTestStaff : IQuoteStaffReader
{
    private static readonly QuoteStaffMember[] Members =
        [new(1, "Директор", "director@brevi.test"), new(17, "Олена", "o@brevi.test", "+380")];

    public Task<IReadOnlyList<QuoteStaffMember>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<QuoteStaffMember>>(Members);

    public Task<QuoteStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
        Task.FromResult(Members.FirstOrDefault(x => x.UserId == userId));
}

internal sealed class QuoteTestRenderer : IQuotePdfRenderer
{
    public QuotePdfModel? Model { get; private set; }

    public QuoteIntroPdfModel? IntroModel { get; private set; }

    public Task<byte[]> RenderAsync(QuotePdfModel model, CancellationToken cancellationToken)
    {
        Model = model;
        return Task.FromResult("%PDF-"u8.ToArray());
    }

    public Task<byte[]> RenderIntroductionAsync(QuoteIntroPdfModel model, CancellationToken cancellationToken)
    {
        IntroModel = model;
        return Task.FromResult("%PDF-"u8.ToArray());
    }
}

internal sealed class QuoteTestClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
