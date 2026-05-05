namespace HrPlatform.Models;

public class InviteResult
{
    public bool Success { get; private init; }
    public string? Error { get; private init; }

    /// <summary>True = invited an existing user. False = sent registration invite.</summary>
    public bool ExistingUser { get; private init; }

    public static InviteResult Ok(bool existing) =>
        new() { Success = true, ExistingUser = existing };

    public static InviteResult Fail(string error) =>
        new() { Success = false, Error = error };
}