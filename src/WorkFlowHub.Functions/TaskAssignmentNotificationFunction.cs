using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace WorkFlowHub.Functions;

public class TaskAssignmentNotificationFunction
{
    private readonly ILogger<TaskAssignmentNotificationFunction> _logger;

    public TaskAssignmentNotificationFunction(ILogger<TaskAssignmentNotificationFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Queue-triggered function. Fires when a task is assigned to a user.
    /// Queue message format: { "taskId": "...", "taskTitle": "...", "assignedUserEmail": "..." }
    /// Replace the log stub with a real email provider (SendGrid, SMTP, etc.) in production.
    /// </summary>
    [Function("TaskAssignmentNotification")]
    public async Task Run(
        [QueueTrigger("task-assignments", Connection = "AzureWebJobsStorage")] string message)
    {
        _logger.LogInformation("Task assignment message received: {Message}", message);

        TaskAssignmentMessage? payload;

        try
        {
            payload = JsonSerializer.Deserialize<TaskAssignmentMessage>(message,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize task assignment message.");
            return;
        }

        if (payload is null)
        {
            _logger.LogWarning("Received null payload in task assignment queue.");
            return;
        }

        // Stub — swap in SendGrid / SMTP / Azure Communication Services here
        _logger.LogInformation(
            "Sending email notification to {Email} for task '{Title}' (ID: {TaskId})",
            payload.AssignedUserEmail, payload.TaskTitle, payload.TaskId);

        await Task.CompletedTask;
    }
}

public record TaskAssignmentMessage(
    Guid TaskId,
    string TaskTitle,
    string AssignedUserEmail);
