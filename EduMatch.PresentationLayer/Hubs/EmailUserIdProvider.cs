using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class EmailUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Tells SignalR that the user's ID is their Email claim
        return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}