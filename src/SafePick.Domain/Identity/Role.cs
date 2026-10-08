namespace SafePick.Domain.Identity;

/// <summary>
/// The roles a user may hold in SafePick SA.
///
/// Note: there is deliberately no "Parent" role. Parents are represented
/// as ParentContact value objects on learners (see Transport context),
/// and receive notifications without ever logging in. This is a decision,
/// not an omission.
/// </summary>
public enum Role
{
    /// <summary>
    /// The root of trust. Seeded on first boot. May only invite School Admins.
    /// Deliberately blind to operational data.
    /// </summary>
    Operator = 0,

    /// <summary>
    /// Governs one school. Invited by the Operator. Approves drivers,
    /// assigns learners, monitors custody.
    /// </summary>
    SchoolAdmin = 1,

    /// <summary>
    /// School staff (e.g. gate monitor). Invited by a School Admin.
    /// Bound to the same school.
    /// </summary>
    SchoolStaff = 2,

    /// <summary>
    /// A transport provider. Self-registers, but gains no custody powers
    /// until approved by a School Admin for a specific school.
    /// </summary>
    Driver = 3
}