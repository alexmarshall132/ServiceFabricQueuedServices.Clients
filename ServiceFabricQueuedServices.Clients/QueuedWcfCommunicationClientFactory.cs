namespace ServiceFabricQueuedServices.Clients
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.ServiceModel;
	using System.Threading;
	using System.Threading.Tasks;

	using Microsoft.ServiceBus;
	using Microsoft.ServiceBus.Messaging;
	using Microsoft.ServiceFabric.Services.Client;
	using Microsoft.ServiceFabric.Services.Communication.Client;
	using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

	/// <summary>
	/// Implementation of <see cref="WcfCommunicationClientFactory{TServiceContract}"/> that supports queued services
	/// via Azure Service Bus.
	/// </summary>
	/// <typeparam name="TServiceContract">
	/// The type of the Service contract.
	/// </typeparam>
	public class QueuedWcfCommunicationClientFactory<TServiceContract> : WcfCommunicationClientFactory<TServiceContract> where TServiceContract : class
	{
		private readonly ChannelFactory<TServiceContract> factory;

		public QueuedWcfCommunicationClientFactory(NetMessagingBinding clientBinding, TokenProvider tokenProvider, IEnumerable<IExceptionHandler> exceptionHandlers = null, IServicePartitionResolver servicePartitionResolver = null, string traceId = null) : 
			base(clientBinding, exceptionHandlers, servicePartitionResolver, traceId, null)
		{
			this.factory = new ChannelFactory<TServiceContract>(clientBinding ?? throw new ArgumentNullException(nameof(clientBinding)));
			this.factory.Endpoint.Behaviors.Add(new TransportClientEndpointBehavior(tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider))));
		}

		/// <summary>
		/// Creates a communication client for the given endpoint address.
		/// </summary>
		/// <param name="endpoint">Endpoint address where the service is listening</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>The communication client that was created</returns>
		protected override async Task<WcfCommunicationClient<TServiceContract>> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
		{
			EndpointAddress endpointAddress = new EndpointAddress(endpoint);
			TServiceContract channel = this.factory.CreateChannel(endpointAddress);
			IClientChannel clientChannel = (IClientChannel)channel;
			Exception connectionTimeoutException = null;
			try
			{
				Task openTask = Task.Factory.FromAsync(clientChannel.BeginOpen(this.factory.Endpoint.Binding.OpenTimeout, null, null), clientChannel.EndOpen);
				if (await Task.WhenAny(openTask, Task.Delay(this.factory.Endpoint.Binding.OpenTimeout, cancellationToken)) == openTask)
				{
					if (openTask.Exception != null)
						throw openTask.Exception;
					openTask = (Task)null;
				}
				else
				{
					clientChannel.Abort();
					throw new TimeoutException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Failed to open communication client after period '{0}'", new object[1]
					{
			(object) this.factory.Endpoint.Binding.OpenTimeout
					}));
				}
			}
			catch (AggregateException ex)
			{
				ex.Handle((Func<Exception, bool>)(x => x is TimeoutException));
				connectionTimeoutException = (Exception)ex;
			}
			catch (TimeoutException ex)
			{
				connectionTimeoutException = (Exception)ex;
			}
			if (connectionTimeoutException != null)
				throw new EndpointNotFoundException(connectionTimeoutException.Message, connectionTimeoutException);
			clientChannel.OperationTimeout = this.factory.Endpoint.Binding.ReceiveTimeout;
			return this.CreateWcfCommunicationClient(channel);
		}
	}
}