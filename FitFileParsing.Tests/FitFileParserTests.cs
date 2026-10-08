using System.Security.Cryptography.X509Certificates;
using Dynastream.Fit;

namespace FitFileParsing.Tests;

public class FitFileParserTests
{
    static readonly System.DateTime expectedStartTime = new System.DateTime(2024, 1, 15, 8, 30, 0, DateTimeKind.Utc);


    [Fact]
    public void Parse_SessionWithNullStrem_ThrowsException()
    {
        var subject = new FitFileParser();
        Stream? nullStream = null;

        var exception = Assert.Throws<ArgumentNullException>(() => subject.Parse(nullStream!));
    }


    [Fact]
    public void Parse_SessionWithInvalidStream_ThrowsException()
    {
        var subject = new FitFileParser();
        using var invalidStream = new MemoryStream();

        Assert.Throws<FormatException>(() => subject.Parse(invalidStream));
    }

    [Fact]
    public void Parse_WithValidSession_PopulatesBasicSummaryMetrics()
    {
        // Arrange
        var subject = new FitFileParser();

        using var stream = CreateTestFitFile((encoder, session) =>
        {
            // 1. Set the Activity/Sport Type to "Run"
            var activity = new ActivityMesg();
            encoder.Write(activity);

            // 2. Set the summary fields inside the Session Message
            session.SetStartTime(new Dynastream.Fit.DateTime(expectedStartTime));
            session.SetSport(Sport.Running);
            session.SetTotalDistance((float)10000.0);
            session.SetTotalElapsedTime((float)1800.0);
            session.SetAvgHeartRate((byte)155);
            session.SetAvgPower((ushort)250);
            encoder.Write(session);

            // 3. Simple trackpoint to ensure full file processing
            var record = new RecordMesg();
            record.SetTimestamp(new Dynastream.Fit.DateTime(expectedStartTime));
            encoder.Write(record);
        });

        // Act
        var result = subject.Parse(stream);

        // Assert
        var expected = new ActivityData(expectedStartTime, "Running", 1800.0, 155, 250, 10000f);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Sandbox_ParseRealFile_ForInspection()
    {
        // 1. Path to your actual downloaded .fit file
        // (Use an absolute path to make it easy)
        string filePath = @"C:\Users\rhowd\Downloads\2020-05-22-152940-ELEMNT BOLT E9E6-170-47.fit";

        var subject = new FitFileParser();

        // 2. Stream the real file into your production code [index]
        using (var stream = System.IO.File.OpenRead(filePath))
        {
            var result = subject.Parse(stream);

            // 3. WHACK A BREAKPOINT ON THIS LINE BELOW! 🛑
            var inspectionTarget = result;
        }
    }

    private Stream CreateTestFitFile(Action<Encode, SessionMesg>? injectTestData = null)
    {
        var memoryStream = new MemoryStream();

        var encoder = new Encode(ProtocolVersion.V20);
        encoder.Open(memoryStream);

        // 1. Write the mandatory baseline data every test file needs
        var fileId = new FileIdMesg();
        fileId.SetType(Dynastream.Fit.File.Activity);
        fileId.SetTimeCreated(new Dynastream.Fit.DateTime(expectedStartTime));
        encoder.Write(fileId);

        var session = new SessionMesg();
        session.SetStartTime(new Dynastream.Fit.DateTime(expectedStartTime));
        injectTestData?.Invoke(encoder, session);
        encoder.Write(session);

        // 3. Close and package the byte array
        encoder.Close();
        return memoryStream;

    }
}
