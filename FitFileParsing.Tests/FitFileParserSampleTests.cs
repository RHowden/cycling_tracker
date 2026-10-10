using Dynastream.Fit;

namespace FitFileParsing.Tests;

/// <summary>
/// Step 1 of the "HR, power and speed chart" plan: the parser should collect Record
/// messages into column arrays (ActivityData.Samples).
///
/// Assumed API (adjust names to taste):
///   ActivityData.Samples : SampleData?   (null when the file has no records)
///   SampleData.Time      : int[]         elapsed seconds from the first record
///   SampleData.HeartRate : int?[]
///   SampleData.Power     : int?[]
///   SampleData.Speed     : double?[]     m/s, EnhancedSpeed ?? Speed
/// </summary>
public class FitFileParserSampleTests
{
    static readonly System.DateTime start = new(2024, 1, 15, 8, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Parse_WithRampingPower_ReturnsPowerAndTimeColumns()
    {
        using var stream = BuildFitFile(encoder =>
        {
            for (int i = 0; i < 5; i++)
                encoder.Write(Record(i, power: 100 + i * 50));
        });

        var result = new FitFileParser().Parse(stream);

        Assert.NotNull(result.Samples);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, result.Samples!.Time);
        Assert.Equal(new int?[] { 100, 150, 200, 250, 300 }, result.Samples.Power);
    }

    [Fact]
    public void Parse_TimeIsElapsedSecondsFromFirstRecord()
    {
        using var stream = BuildFitFile(encoder =>
        {
            // First record is 60s after session start; time must still begin at 0.
            encoder.Write(Record(60, heartRate: 120));
            encoder.Write(Record(61, heartRate: 121));
            encoder.Write(Record(62, heartRate: 122));
        });

        var result = new FitFileParser().Parse(stream);

        Assert.Equal(new[] { 0, 1, 2 }, result.Samples!.Time);
    }

    [Fact]
    public void Parse_HeartRateOnly_PowerAndSpeedAreAllNull()
    {
        using var stream = BuildFitFile(encoder =>
        {
            for (int i = 0; i < 3; i++)
                encoder.Write(Record(i, heartRate: 140 + i));
        });

        var result = new FitFileParser().Parse(stream);

        Assert.Equal(new int?[] { 140, 141, 142 }, result.Samples!.HeartRate);
        Assert.All(result.Samples.Power, p => Assert.Null(p));
        Assert.All(result.Samples.Speed, s => Assert.Null(s));
    }

    [Fact]
    public void Parse_PowerDropout_IsNullNotZero()
    {
        using var stream = BuildFitFile(encoder =>
        {
            encoder.Write(Record(0, power: 200, heartRate: 130));
            encoder.Write(Record(1, power: null,heartRate: 131)); 
            encoder.Write(Record(2, power: 210, heartRate: 132));
        });

        var result = new FitFileParser().Parse(stream);

        Assert.Equal(new int?[] { 200, null, 210 }, result.Samples!.Power);
        Assert.Equal(new int?[] { 130, 131, 132 }, result.Samples.HeartRate);
    }

    [Fact]
    public void Parse_TimestampGap_PreservesRealElapsedTime()
    {
        using var stream = BuildFitFile(encoder =>
        {
            encoder.Write(Record(0, power: 200));
            encoder.Write(Record(1, power: 200));
            encoder.Write(Record(300, power: 200)); // 5 minute pause
            encoder.Write(Record(301, power: 200));
        });

        var result = new FitFileParser().Parse(stream);

        Assert.Equal(new[] { 0, 1, 300, 301 }, result.Samples!.Time);
    }

    [Fact]
    public void Parse_SpeedIsStoredInMetresPerSecond()
    {
        using var stream = BuildFitFile(encoder =>
        {
            encoder.Write(Record(0, speed: 3.5f));
            encoder.Write(Record(1, speed: 4.0f));
        });

        var result = new FitFileParser().Parse(stream);

        Assert.Equal(2, result.Samples!.Speed.Length);
        Assert.Equal(3.5, result.Samples.Speed[0]!.Value, precision: 2);
        Assert.Equal(4.0, result.Samples.Speed[1]!.Value, precision: 2);
    }

    [Fact]
    public void Parse_PrefersEnhancedSpeedOverSpeed()
    {
        using var stream = BuildFitFile(encoder =>
        {
            encoder.Write(Record(0, speed: 3.0f, enhancedSpeed: 3.456f));
        });

        var result = new FitFileParser().Parse(stream);

        Assert.Equal(3.456, result.Samples!.Speed[0]!.Value, precision: 2);
    }

    [Fact]
    public void Parse_FallsBackToSpeedWhenNoEnhancedSpeed()
    {
        using var stream = BuildFitFile(encoder =>
        {
            encoder.Write(Record(0, speed: 2.5f));
        });

        var result = new FitFileParser().Parse(stream);

        Assert.Equal(2.5, result.Samples!.Speed[0]!.Value, precision: 2);
    }

    [Fact]
    public void Parse_NoRecords_ReturnsNullSamples()
    {
        using var stream = BuildFitFile(_ => { });

        var result = new FitFileParser().Parse(stream);

        Assert.Null(result.Samples);
    }

    [Fact]
    public void Parse_AllColumnsHaveSameLength()
    {
        using var stream = BuildFitFile(encoder =>
        {
            encoder.Write(Record(0, power: 200, heartRate: 130, speed: 8f));
            encoder.Write(Record(1, heartRate: 131));
            encoder.Write(Record(2, power: 210));
        });

        var samples = new FitFileParser().Parse(stream).Samples!;

        Assert.Equal(3, samples.Time.Length);
        Assert.Equal(3, samples.HeartRate.Length);
        Assert.Equal(3, samples.Power.Length);
        Assert.Equal(3, samples.Speed.Length);
    }

    // ---- helpers ----

    private static RecordMesg Record(
        int secondsFromStart,
        int? power = null,
        int? heartRate = null,
        float? speed = null,
        float? enhancedSpeed = null)
    {
        var record = new RecordMesg();
        record.SetTimestamp(new Dynastream.Fit.DateTime(start.AddSeconds(secondsFromStart)));
        if (power.HasValue) record.SetPower((ushort)power.Value);
        if (heartRate.HasValue) record.SetHeartRate((byte)heartRate.Value);
        if (speed.HasValue) record.SetSpeed(speed.Value);
        if (enhancedSpeed.HasValue) record.SetEnhancedSpeed(enhancedSpeed.Value);
        return record;
    }

    /// <summary>
    /// Writes FileId, then the records supplied by the caller, then a single Session.
    /// </summary>
    private static Stream BuildFitFile(Action<Encode> writeRecords)
    {
        var stream = new MemoryStream();
        var encoder = new Encode(ProtocolVersion.V20);
        encoder.Open(stream);

        var fileId = new FileIdMesg();
        fileId.SetType(Dynastream.Fit.File.Activity);
        fileId.SetTimeCreated(new Dynastream.Fit.DateTime(start));
        encoder.Write(fileId);

        writeRecords(encoder);

        var session = new SessionMesg();
        session.SetStartTime(new Dynastream.Fit.DateTime(start));
        session.SetSport(Sport.Cycling);
        session.SetTotalElapsedTime(1800f);
        encoder.Write(session);

        encoder.Close();
        stream.Position = 0;
        return stream;
    }
}