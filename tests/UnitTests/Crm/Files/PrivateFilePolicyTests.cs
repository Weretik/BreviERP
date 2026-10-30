using Crm.Application.Files.Validation;
namespace UnitTests.Crm.Files;
public class PrivateFilePolicyTests
{
    [TestCase("application/pdf", new byte[] { 37, 80, 68, 70, 45, 49 })]
    [TestCase("image/jpeg", new byte[] { 255, 216, 255, 224 })]
    [TestCase("image/png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })]
    public void Allowed_actual_signature_matches_declared_mime(string mime, byte[] bytes) => Assert.That(PrivateFilePolicy.MatchesAllowedContent(bytes, mime), Is.True);
    [TestCase("application/pdf")]
    [TestCase("text/html")]
    [TestCase("image/png")]
    public void Arbitrary_text_and_mime_spoof_are_rejected(string mime) => Assert.That(PrivateFilePolicy.MatchesAllowedContent("<script>unsafe</script>"u8, mime), Is.False);
}
