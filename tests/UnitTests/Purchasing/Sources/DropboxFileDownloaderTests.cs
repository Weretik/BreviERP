using Purchasing.Infrastructure.Integrations.Dropbox;

namespace UnitTests.Purchasing.Sources;

public sealed class DropboxFileDownloaderTests
{
    [Test]
    public void The_shared_link_is_asked_as_a_download()
    {
        var link = DropboxFileDownloader.DirectLink(new Uri("https://www.dropbox.com/scl/fi/rei5/file_749.pdf?rlkey=7pbv&dl=0"));
        Assert.That(link.ToString(), Is.EqualTo("https://www.dropbox.com/scl/fi/rei5/file_749.pdf?rlkey=7pbv&dl=1"));
    }
}
