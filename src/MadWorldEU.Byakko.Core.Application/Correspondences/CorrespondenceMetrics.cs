using System.Diagnostics.Metrics;

namespace MadWorldEU.Byakko.Correspondences;

/// <summary>Tracks correspondence-related metrics.</summary>
public sealed class CorrespondenceMetrics : ICorrespondenceMetrics
{
    public const string MeterName = "Byakko.Correspondences";

    private readonly Counter<long> _feedbackSentTotal;

    public CorrespondenceMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _feedbackSentTotal = meter.CreateCounter<long>(
            "correspondences.feedback.sent.total",
            description: "Total number of feedback messages sent.");
    }

    public void RecordFeedbackSent() => _feedbackSentTotal.Add(1);
}