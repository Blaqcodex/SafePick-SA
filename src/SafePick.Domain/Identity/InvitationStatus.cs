namespace SafePick.Domain.Identity;

/// <summary>
/// The lifecycle of an invitation.
///
/// An invitation is a pending grant of authority. It is single-use and
/// time-bounded. Once it leaves Pending, it can never return to Pending.
/// </summary>
public enum InvitationStatus
{
    /// <summary>The invitation has been issued and awaits redemption.</summary>
    Pending = 0,

    /// <summary>The invitation was redeemed and the user account was created.</summary>
    Accepted = 1,

    /// <summary>The invitation passed its expiry window without being redeemed.</summary>
    Expired = 2,

    /// <summary>The invitation was revoked before it was redeemed.</summary>
    Revoked = 3
}