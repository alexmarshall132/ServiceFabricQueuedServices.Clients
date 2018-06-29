using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace ServiceFabricQueuedServices.Clients
{
	/// <summary>
	/// Utility methods for <see cref="WcfCommunicationClient{TServiceContract}"/> instances to be used
	/// to connect to stateless services.
	/// </summary>
	public static class WcfCommunicationClientExtensions
	{
		/// <summary>
		/// Creates a new <see cref="WcfCommunicationClient{TServiceContract}"/> capable of connection
		/// to a stateless service with a singleton partition and default replica selection.
		/// </summary>
		/// <param name="wcfClientFactory">
		/// The <see cref="WcfCommunicationClientFactory{TServiceContract}"/> to be used to create the
		/// client. Must not be null.
		/// </param>
		/// <param name="fabricServiceUri">
		/// The name of the Service Fabric service to which we're connecting. This is the URI of the service,
		/// e.g. <code>fabric:/MyApplication/MyQueuedService</code>
		/// </param>
		/// <param name="retrySettings">
		/// The settings used for retry operations. May be null, in which case default settings will be used.
		/// </param>
		/// <param name="cancellationToken">
		/// The <see cref="CancellationToken"/> used to abort the call if necessary.
		/// </param>
		/// <typeparam name="TServiceContract">
		/// The type of WCF Service Contract in use.
		/// </typeparam>
		/// <returns>
		/// A <see cref="Task{TResult}"/> yielding a <see cref="WcfCommunicationClient{TServiceContract}"/>
		/// ready for use with a default, queued, stateless service with a single partition. Guaranteed
		/// not to be null.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="wcfClientFactory"/> is null
		/// -or-
		/// Thrown if <paramref name="fabricServiceUri"/> is null.
		/// </exception>
		public static Task<WcfCommunicationClient<TServiceContract>> CreateDefaultClientAsync<TServiceContract>(
			this WcfCommunicationClientFactory<TServiceContract> wcfClientFactory,
			Uri fabricServiceUri,
			OperationRetrySettings retrySettings = null,
			CancellationToken? cancellationToken = null) where TServiceContract : class
		{
			if (wcfClientFactory == null)
			{
				throw new ArgumentNullException(nameof(wcfClientFactory));
			}

			if (fabricServiceUri == null)
			{
				throw new ArgumentNullException(nameof(fabricServiceUri));
			}

			return wcfClientFactory.GetClientAsync(
				serviceUri: fabricServiceUri,
				partitionKey: ServicePartitionKey.Singleton,
				targetReplicaSelector: TargetReplicaSelector.Default,
				listenerName: String.Empty,
				retrySettings: retrySettings ?? new OperationRetrySettings(),
				cancellationToken: cancellationToken ?? CancellationToken.None
			);
		}
	}
}
