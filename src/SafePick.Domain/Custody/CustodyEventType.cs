namespace SafePick.Domain.Custody;

/// <summary>
/// The kinds of custody events that can be recorded.
///
/// Every custody event is immutable. Corrections are recorded as a new
/// event of type Correction that references the original — the original
/// is never modified or deleted (ADR-0004 and ADR-0011).
/// </summary>
public enum CustodyEventType
{
    /// <summary>The learner was confirmed to have entered the vehicle.</summary>
    PickedUp = 0,

    /// <summary>
    /// The driver marked the learner as "not in the car" — the learner
    /// was expected but was not collected. A reason is required.
    /// </summary>
    NotInCar = 1,

    /// <summary>
    /// The school marked the learner as absent before the pickup window
    /// opened. This suppresses the expectation for that day.
    /// </summary>
    SchoolMarkedAbsent = 2,

    /// <summary>
    /// A correction to a prior custody event. The correction references
    /// the original event by id. The original remains unchanged.
    /// </summary>
    Correction = 3
}