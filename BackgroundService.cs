namespace ExcelBotCs;

/// Copyright(c) .NET Foundation.Licensed under the Apache License, Version 2.0.
/// <summary>
/// Base class for implementing a long running <see cref="IHostedService"/>.
/// </summary>
public abstract class BackgroundService : IHostedService, IDisposable
{
	protected readonly IServiceScopeFactory _scopeFactory;
	private Task _executingTask;
	private readonly CancellationTokenSource _stoppingCts =
		new CancellationTokenSource();

	public BackgroundService(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

	public virtual Task StartAsync(CancellationToken cancellationToken)
	{
		// Store the task we're executing
		_executingTask = ExecuteAsync(_stoppingCts.Token);

		// If the task is completed then return it,
		// this will bubble cancellation and failure to the caller
		if (_executingTask.IsCompleted)
		{
			return _executingTask;
		}

		// Otherwise it's running
		return Task.CompletedTask;
	}

	public virtual async Task StopAsync(CancellationToken cancellationToken)
	{
		// Stop called without start
		if (_executingTask == null)
		{
			return;
		}

		try
		{
			// Signal cancellation to the executing method
			_stoppingCts.Cancel();
		}
		finally
		{
			// Wait until the task completes or the stop token triggers
			await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite,
				cancellationToken));
		}
	}

	public virtual void Dispose()
	{
		_stoppingCts.Cancel();
	}
}