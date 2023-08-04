namespace cobra.service.mail.listener.communications.Services;

public interface IEmailListenerService
{
    Task ListenForIncomingAsync(DateTime? dateTime, int retryNumber = 0);
}
