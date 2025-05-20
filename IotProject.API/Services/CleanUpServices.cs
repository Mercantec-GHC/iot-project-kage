using IotProject.API.Data;

namespace IotProject.API.Services
{
	public class CleanUpServices : BackgroundService
	{
		private readonly ILogger<CleanUpServices> logger;
		private readonly IServiceProvider serviceProvider;

		public CleanUpServices(ILogger<CleanUpServices> _logger, IServiceProvider _serviceProvider)
		{ 
			logger = _logger;
			serviceProvider = _serviceProvider;
		}

		// Execution of the service. Designed to execute on a certain day, on a certain time.
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			logger.LogInformation("Timed hosted service running.");

			// Runs while the stoppingToken isn't cancelled. 
			while (!stoppingToken.IsCancellationRequested)
			{
				// Gets the next runTime.
				var nextRunTime = GetNextRunTime();
				var delayTime = nextRunTime - DateTime.UtcNow;

				logger.LogInformation("Next cleanup scheduled at: {NextRunTime}", nextRunTime);

				// Waits for the next runTime.
				await Task.Delay(delayTime, stoppingToken);
				
				// Runs the cleanup logic.
				await CleanupOldData();
			}
		}

		// Execution of the service. Designed to execute every 24 hours 
		//protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		//{
		//	logger.LogInformation("Timed hosted service running.");

		//	await CleanupOldData();

		//	using PeriodicTimer timer = new(TimeSpan.FromHours(24));

		//	try
		//	{
		//		while (await timer.WaitForNextTickAsync(stoppingToken))
		//		{
		//			await CleanupOldData();
		//		}
		//	}
		//	catch (OperationCanceledException)
		//	{
		//		logger.LogInformation("Timed hosted service is stopping.");
		//	}
		//}

		// The logic for removal of old data form the database. 
		// Designed to delete DeviceData older the 14 days.
		// Designed to delete RefreshToken. Where the ExpiryDate is less the Utc.Now, or the IsRevoked is true. 
		private async Task CleanupOldData()
		{
			// Delete old data from database.
			using (var scope = serviceProvider.CreateScope())
			{
				// Gets the dbContext service.
				var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
				var cutoffDate = DateTime.UtcNow.AddDays(-14);

				// The removal query's for the DeviceData and RefreshToken.  
				dbContext.DeviceData.RemoveRange(dbContext.DeviceData.Where(x => x.Timestamp < cutoffDate));
				dbContext.RefreshTokens.RemoveRange(dbContext.RefreshTokens.Where(x => x.IsRevoked == true || x.ExpiryDate < DateTime.UtcNow));
				await dbContext.SaveChangesAsync();
			}
		}

		// Calculates the next run time. 
		private DateTime GetNextRunTime()
		{
			// Get today at midnight. 
			var now = DateTime.UtcNow;
			var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

			// Adds a day until DayOfWeek is Sunday. 
			while (scheduledTime.DayOfWeek == DayOfWeek.Sunday || scheduledTime < now)
			{
				scheduledTime = scheduledTime.AddDays(1);
			}
			return scheduledTime;
		}
	}
}