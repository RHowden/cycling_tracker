using Dynastream.Fit;

namespace FitFileParsing;

public class FitFileParser
{
    public ActivityData Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (stream.Length == 0)
        {
            throw new FormatException("The provided stream is empty and is not a valid FIT file format.");
        }

        Decode decodeDemo = new();

        // Use a FitListener to capture all decoded messages in a FitMessages object
        FitListener fitListener = new();
        decodeDemo.MesgEvent += fitListener.OnMesg;

        //reset to start of stream
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        if (decodeDemo.Read(stream) is not true)
        {
            throw new FormatException("The provided stream is not a valid FIT file format.");
        }

        FitMessages fitMessages = fitListener.FitMessages;

        var session = fitMessages.SessionMesgs.FirstOrDefault();

        if (session == null)
        {
            throw new FormatException("FIT file is missing a valid summary Session message.");
        }

        var startTime = session.GetStartTime().GetDateTime();
        Sport sport = session.GetSport() ?? Sport.Generic;
        double duration = session.GetTotalElapsedTime() ?? 0f;
        int? avgHr = session.GetAvgHeartRate();
        int? avgPower = session.GetAvgPower();
        float distance = session.GetTotalDistance() ?? 0f;

        var samples = BuildSamples(fitMessages.RecordMesgs);

        return new ActivityData(startTime, sport.ToString(), duration, avgHr, avgPower, distance, samples);
    }

    private static SampleData? BuildSamples(IReadOnlyList<RecordMesg> records)
    {
        // Records without a timestamp can't be placed on the time axis, so skip them.
        var timed = records
            .Where(r => r.GetTimestamp() != null)
            .ToList();

        if (timed.Count == 0)
        {
            return null;
        }

        var first = timed[0].GetTimestamp().GetDateTime();

        var time = new int[timed.Count];
        var heartRate = new int?[timed.Count];
        var power = new int?[timed.Count];
        var speed = new double?[timed.Count];

        for (int i = 0; i < timed.Count; i++)
        {
            var record = timed[i];

            time[i] = (int)(record.GetTimestamp().GetDateTime() - first).TotalSeconds;
            heartRate[i] = NullIfInvalid(record.GetHeartRate(), byte.MaxValue);
            power[i] = NullIfInvalid(record.GetPower(), ushort.MaxValue);
            speed[i] = ValidSpeed(record.GetEnhancedSpeed()) ?? ValidSpeed(record.GetSpeed());
        }



        return new SampleData(time, heartRate, power, speed);
    }

    // FIT marks "no value" with the max value of the field's base type
    // (0xFF for uint8, 0xFFFF for uint16). Normalise those to null.
    private static int? NullIfInvalid(byte? value, byte invalid) =>
        value is null || value == invalid ? null : value;

    private static int? NullIfInvalid(ushort? value, ushort invalid) =>
        value is null || value == invalid ? null : value;

    private static double? ValidSpeed(float? value) =>
        value is null || value >= 65.5f ? null : value;
}
