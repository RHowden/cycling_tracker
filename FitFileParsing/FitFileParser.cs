using System;
using System.Collections.Generic;
using System.IO;
using Dynastream.Fit;

namespace FitFileParsing;

public class FitFileParser
{
    public ActivityData Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // 1. Guard against empty streams immediately (Fixes the sad path test)
        if (stream.Length == 0)
        {
            throw new FormatException("The provided stream is empty and is not a valid FIT file format.");
        }

        Decode decodeDemo = new();

        if (decodeDemo.IsFIT(stream) is not true)
        {
            //throw new FormatException("The provided stream is not a valid FIT file format.");
        }

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

        // Convert Garmin's Sport enum to a clean string format matching your requirements
        Sport sport = session.GetSport() ?? Sport.Generic;

        string sportType = sport switch
        {
            Sport.Running => "Run",
            Sport.Cycling => "Bike",
            _ => sport.ToString()
        };

        double duration = session.GetTotalElapsedTime() ?? 0f;
        int? avgHr = session.GetAvgHeartRate();
        int? avgPower = session.GetAvgPower();
        float distance = session.GetTotalDistance() ?? 0f;

        var startTime = fitMessages.SessionMesgs[0].GetStartTime().GetDateTime();

        return new ActivityData(startTime, sportType, duration, avgHr, avgPower, distance);
    }
}
