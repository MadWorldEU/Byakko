namespace MadWorldEU.Byakko.Configurations;

public static class BlazorErrors
{
    public static readonly BlazorError FileLoadFailed = BlazorError.Create("Blazor.File.LoadFailed", "The file could not be loaded. Please try again.");
    public static readonly BlazorError FilesLoadFailed = BlazorError.Create("Blazor.Files.LoadFailed", "The files could not be loaded. Please try again.");
    public static readonly BlazorError AuditLogsLoadFailed = BlazorError.Create("Blazor.AuditLogs.LoadFailed", "The audit logs could not be loaded. Please try again.");
    public static readonly BlazorError TriggerFailed = BlazorError.Create("Blazor.Trigger.Failed", "The request could not be processed. Please try again.");
    public static readonly BlazorError FileDeleteFailed = BlazorError.Create("Blazor.File.DeleteFailed", "Something went wrong while deleting the file. Please try again.");
    public static readonly BlazorError FileUploadFailed = BlazorError.Create("Blazor.File.UploadFailed", "Something went wrong while uploading the file. Please try again.");
    public static readonly BlazorError FileInvalidId = BlazorError.Create("Blazor.File.InvalidId", "The file link is not valid. Please check the link and try again.");
    public static readonly BlazorError FeedbackSendFailed = BlazorError.Create("Blazor.Feedback.SendFailed", "Something went wrong while sending your message. Please try again.");
    public static readonly BlazorError CorrespondenceTooManyRequests = BlazorError.Create("Blazor.Correspondence.TooManyRequests", "Too many requests");
}