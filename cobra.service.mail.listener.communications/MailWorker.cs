using cobra.service.mail.listener.communications.Services;
using cobra.service.mail.listener.communications.Utils;

namespace cobra.service.mail.listener.communications
{
    public class MailWorker : BackgroundService
    {
        private readonly ILogger<MailWorker> _logger;
        private readonly IEmailListenerService _emailListenerService;
        private string WorkerName { get;}

        public MailWorker(ILogger<MailWorker> logger, IEmailListenerService emailListener)
        {
            _logger = logger;
            _emailListenerService = emailListener;
            WorkerName = GetType().Name;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Await right away so Host Startup can continue.
            await Task.Yield();

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation("Calling EmailListeningService");

                        var dateTime = LocalDateTime.GetDateTimeNow();
                        await _emailListenerService.ListenForIncomingAsync(dateTime, 0).ConfigureAwait(false);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Unhandled exception occurred in the {worker}. Worker will retry after the normal interval.",
                            WorkerName);
                    }

                    const int secondsInterval = 3;
                    await Task.Delay(secondsInterval * 1000, stoppingToken).ConfigureAwait(false);
                }

                _logger.LogInformation(
                    "Execution ended. Cancellation token cancelled = {IsCancellationRequested}",
                    stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Execution Cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception. Execution Stopping");
            }
        }
    }

}