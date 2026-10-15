using Dynastream.Fit;

namespace FitFileParsing.Tests;

public class FitFileParserTests
{
    static readonly System.DateTime expectedStartTime = new(2024, 1, 15, 8, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Parse_NullStream_ThrowsArgumentNullException()
    {
        var subject = new FitFileParser();
        Stream? nullStream = null;

        Assert.Throws<ArgumentNullException>(() => subject.Parse(nullStream!));
    }

    [Fact]
    public void Parse_EmptyStream_ReturnsNotAFitFile()
    {
        using var stream = new MemoryStream();

        var result = new FitFileParser().Parse(stream);

        Assert.False(result.Succeeded);
        Assert.Equal(FitParseError.NotAFitFile, result.Error);
    }

    [Fact]
    public void Parse_GarbageBytes_ReturnsNotAFitFile()
    {
        using var stream = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes("this is definitely not a fit file, just text"));

        var result = new FitFileParser().Parse(stream);

        Assert.False(result.Succeeded);
        Assert.Equal(FitParseError.NotAFitFile, result.Error);
    }

    [Fact]
    public void Parse_NoSession_ReturnsNoSession()
    {
        using var stream = BuildFileWithSessions(0);

        var result = new FitFileParser().Parse(stream);

        Assert.False(result.Succeeded);
        Assert.Equal(FitParseError.NoSession, result.Error);
    }

    [Fact]
    public void Parse_MultipleSessions_ReturnsMultipleSessions()
    {
        using var stream = BuildFileWithSessions(2);

        var result = new FitFileParser().Parse(stream);

        Assert.False(result.Succeeded);
        Assert.Equal(FitParseError.MultipleSessions, result.Error);
    }

    [Fact]
    public void Parse_WithValidSession_PopulatesBasicSummaryMetrics()
    {
        var subject = new FitFileParser();

        using var stream = CreateTestFitFile((encoder, session) =>
        {
            var activity = new ActivityMesg();
            encoder.Write(activity);

            session.SetStartTime(new Dynastream.Fit.DateTime(expectedStartTime));
            session.SetSport(Sport.Running);
            session.SetTotalDistance(10000.0f);
            session.SetTotalElapsedTime(1800.0f);
            session.SetAvgHeartRate((byte)155);
            session.SetAvgPower((ushort)250);

            // Simple trackpoint to ensure full file processing
            var record = new RecordMesg();
            record.SetTimestamp(new Dynastream.Fit.DateTime(expectedStartTime));
            encoder.Write(record);
        });

        var result = subject.Parse(stream);

        Assert.True(result.Succeeded);
        var expected = new ActivityData(expectedStartTime, "Running", 1800.0, 155, 250, 10000f);
        Assert.Equal(expected, result.ActivityData with { Samples = null });
    }

    [Fact]
    public void WithRealFile_ParseRealFile_Succeeds()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleFitFile.fit");

        var subject = new FitFileParser();

        using var stream = System.IO.File.OpenRead(filePath);
        var result = subject.Parse(stream);

        Assert.True(result.Succeeded);
    }

    private static MemoryStream CreateTestFitFile(Action<Encode, SessionMesg>? injectTestData = null)
    {
        var memoryStream = new MemoryStream();

        var encoder = new Encode(ProtocolVersion.V20);
        encoder.Open(memoryStream);

        // Mandatory baseline data every test file needs
        var fileId = new FileIdMesg();
        fileId.SetType(Dynastream.Fit.File.Activity);
        fileId.SetTimeCreated(new Dynastream.Fit.DateTime(expectedStartTime));
        encoder.Write(fileId);

        var session = new SessionMesg();
        session.SetStartTime(new Dynastream.Fit.DateTime(expectedStartTime));
        injectTestData?.Invoke(encoder, session);
        encoder.Write(session);

        encoder.Close();
        memoryStream.Position = 0;
        return memoryStream;
    }

    private static MemoryStream BuildFileWithSessions(int sessionCount)
    {
        var stream = new MemoryStream();

        var encoder = new Encode(ProtocolVersion.V20);
        encoder.Open(stream);

        var fileId = new FileIdMesg();
        fileId.SetType(Dynastream.Fit.File.Activity);
        fileId.SetTimeCreated(new Dynastream.Fit.DateTime(expectedStartTime));
        encoder.Write(fileId);

        for (int i = 0; i < sessionCount; i++)
        {
            var session = new SessionMesg();
            session.SetStartTime(new Dynastream.Fit.DateTime(expectedStartTime.AddHours(i)));
            session.SetSport(Sport.Cycling);
            encoder.Write(session);
        }

        encoder.Close();
        stream.Position = 0;
        return stream;
    }
}