namespace ServiceFabricQueuedServices.Clients
{
	using System;
	using System.Collections.Generic;

	using Microsoft.ServiceBus;
	using Microsoft.ServiceBus.Messaging;
	using Microsoft.ServiceFabric.Services.Client;
	using Microsoft.ServiceFabric.Services.Communication.Client;

	/// <summary>
    /// Utility for creating <see cref="ICommunicationClientFactory{TCommunicationClient}"/> instances capable
    /// of sending queued messages via the Azure Service Bus and WCF Service Contracts.
    /// </summary>
    public static class QueuedCommunicationClientUtility
    {
	    /// <summary>
	    /// Creates a new <see cref="QueuedWcfCommunicationClientFactory{TServiceContract}"/> with default parameters
	    /// necessary to connect to an Azure Service Bus queue and publish WCF messages.
	    /// </summary>
	    /// <param name="netMessagingBinding">
	    /// The <see cref="NetMessagingBinding"/> to be used to bind to the WCF endpoint. Must not be null.
	    /// </param>
	    /// <param name="sharedAccessKeyName">
	    /// The name of the shared access key to be used to connect. Must not be null or empty.
	    /// </param>
	    /// <param name="sharedAccessKey">
	    /// The value of the shared access key to be used to connect. Must not be null or empty.
	    /// </param>
	    /// <param name="exceptionHandlers">
	    /// The handlers to be used for handling exceptions. May be null.
	    /// </param>
	    /// <typeparam name="TServiceContract">
	    /// The type of WCF service contract to be used.
	    /// </typeparam>
	    /// <returns>
	    /// A new <see cref="QueuedWcfCommunicationClientFactory{TServiceContract}"/> instance ready for use to publish
	    /// messages via the Service Bus. Guaranteed not to be null.
	    /// </returns>
	    /// <exception cref="ArgumentNullException">
	    /// Thrown if <paramref name="netMessagingBinding"/> is null.
	    /// </exception>
	    /// <exception cref="ArgumentException">
	    /// Thrown if any string parameters are null or empty.
	    /// </exception>
	    public static QueuedWcfCommunicationClientFactory<TServiceContract> CreateCommunicationClientFactory<TServiceContract>(NetMessagingBinding netMessagingBinding, string sharedAccessKeyName, string sharedAccessKey, IEnumerable<IExceptionHandler> exceptionHandlers = null) where TServiceContract : class

		{
			if (netMessagingBinding == null)
			{
				throw new ArgumentNullException(nameof(netMessagingBinding));
			}

			if (String.IsNullOrEmpty(sharedAccessKeyName))
			{
				throw new ArgumentException("Must not be null or empty", nameof(sharedAccessKeyName));
			}

			if (String.IsNullOrWhiteSpace(sharedAccessKey))
			{
				throw new ArgumentException("Must not be null or empty", nameof(sharedAccessKey));
			}

			return CreateCommunicationClientFactory<TServiceContract>(
				netMessagingBinding,
				sharedAccessKeyName,
				sharedAccessKey,
				servicePartitionResolver: ServicePartitionResolver.GetDefault(),
				exceptionHandlers: exceptionHandlers
			);
		}

	    /// <summary>
	    /// Creates a new <see cref="QueuedWcfCommunicationClientFactory{TServiceContract}"/> with default parameters
	    /// necessary to connect to an Azure Service Bus queue and publish WCF messages.
	    /// </summary>
	    /// <param name="netMessagingBinding">
	    /// The <see cref="NetMessagingBinding"/> to be used to bind to the WCF endpoint. Must not be null.
	    /// </param>
	    /// <param name="sharedAccessKeyName">
	    /// The name of the shared access key to be used to connect. Must not be null or empty.
	    /// </param>
	    /// <param name="sharedAccessKey">
	    /// The value of the shared access key to be used to connect. Must not be null or empty.
	    /// </param>
	    /// <param name="servicePartitionResolver">
	    /// The <see cref="IServicePartitionResolver"/> used to resolve the Service Fabric partition
	    /// that contains the target service that's to handle the request.
	    /// </param>
	    /// <param name="exceptionHandlers">
	    /// The handlers to be used for handling exceptions. May be null.
	    /// </param>
	    /// <typeparam name="TServiceContract">
	    /// The type of WCF service contract to be used.
	    /// </typeparam>
	    /// <returns>
	    /// A new <see cref="QueuedWcfCommunicationClientFactory{TServiceContract}"/> instance ready for use to publish
	    /// messages via the Service Bus. Guaranteed not to be null.
	    /// </returns>
	    /// <exception cref="ArgumentNullException">
	    /// Thrown if <paramref name="netMessagingBinding"/> is null.
	    /// </exception>
	    /// <exception cref="ArgumentException">
	    /// Thrown if any string parameters are null or empty.
	    /// </exception>
	    public static QueuedWcfCommunicationClientFactory<TServiceContract> CreateCommunicationClientFactory<TServiceContract>(NetMessagingBinding netMessagingBinding, string sharedAccessKeyName, string sharedAccessKey, ServicePartitionResolver servicePartitionResolver, IEnumerable<IExceptionHandler> exceptionHandlers = null) where TServiceContract : class
		{
			if (netMessagingBinding == null)
			{
				throw new ArgumentNullException(nameof(netMessagingBinding));
			}

			if (String.IsNullOrEmpty(sharedAccessKeyName))
			{
				throw new ArgumentException("Must not be null or empty", nameof(sharedAccessKeyName));
			}

			if (String.IsNullOrWhiteSpace(sharedAccessKey))
			{
				throw new ArgumentException("Must not be null or empty", nameof(sharedAccessKey));
			}

			var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(sharedAccessKeyName, sharedAccessKey);

			return new QueuedWcfCommunicationClientFactory<TServiceContract>(
				clientBinding: netMessagingBinding,
				tokenProvider: tokenProvider,
				servicePartitionResolver: servicePartitionResolver,
				exceptionHandlers: exceptionHandlers,
				traceId: null
			);
		}
    }
}
