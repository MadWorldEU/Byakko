namespace MadWorldEU.Byakko.Accounts;

/// <summary>Represents the lifecycle state of an account.</summary>
public enum AccountStatus
{
    /// <summary>The account is active and in good standing.</summary>
    Active,

    /// <summary>The user has submitted a GDPR deletion request; awaiting administrator action.</summary>
    DeletionRequested,

    /// <summary>An administrator has confirmed the deletion request; data removal is pending.</summary>
    DeletionConfirmed,

    /// <summary>All user data has been permanently deleted.</summary>
    Deleted
}