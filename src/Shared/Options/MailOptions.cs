namespace FireInvent.Shared.Options;

public class MailOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;

    public string SenderAddress { get; set; } = string.Empty;
}
