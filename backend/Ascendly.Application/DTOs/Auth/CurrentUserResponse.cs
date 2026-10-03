namespace Ascendly.Application.DTOs.Auth;

public sealed class CurrentUserResponse
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public int InterviewSessionCount { get; init; }
    public int CompletedInterviewSessionCount { get; init; }
}
