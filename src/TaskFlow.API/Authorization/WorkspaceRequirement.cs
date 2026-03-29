using Microsoft.AspNetCore.Authorization;

namespace TaskFlow.API.Authorization;

public class WorkspaceRequirement(WorkspaceRole minimumRole)
    : IAuthorizationRequirement
{
    public WorkspaceRole MinimumRole { get; } = minimumRole;
}