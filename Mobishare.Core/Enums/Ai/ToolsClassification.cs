namespace Mobishare.Core.Enums.Ai;

[AttributeUsage(AttributeTargets.Field)]
public class ToolInfoAttribute : Attribute
{
    public string Description { get; }
    public ToolInfoAttribute(string description) => Description = description;
}

public enum ToolsClassification
{
    [ToolInfo("Book a vehicle for the user")]
    book_vehicle,

    [ToolInfo("Cancel an active booking")]
    cancel_booking,

    [ToolInfo("Start the ride")]
    start_ride,

    [ToolInfo("End the ride and calculate cost")]
    end_ride,

    [ToolInfo("Retrieve user's ride history")]
    get_ride_history,

    [ToolInfo("Get user's account balance")]
    get_account_balance,

    [ToolInfo("Report a problem with a vehicle")]
    report_issue
}
