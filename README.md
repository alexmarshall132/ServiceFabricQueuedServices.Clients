# ServiceFabricQueuedServices.Clients
Helper library for setting up `WcfCommunicationClient`s for queued calls using Azure Service Bus ***queues***. This functionality *WILL NOT* work with Azure Service Bus ***topics + subscriptions***. All that's required is a  `NetMessagingBinding` of your own configuration, and an Azure Service Bus connection string with `send` privileges.

## Installation

The Nuget package can be downloaded [here](https://www.nuget.org/packages/ServiceFabricQueuedServices.Clients), or can be installed within the Visual Studio Package Manager Console with the command:
```
PM> Install-Package ServiceFabricQueuedServices.Clients -Prerelease
```

## Usage

To use this package, you'll need to execute the following steps:
  1. Create a setting in your `Settings.xml` file for the connection string with `listen` permissions:
```
<?xml version="1.0" encoding="utf-8" ?>
<Settings xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2011/01/fabric">
	<!-- Add your custom configuration sections and parameters here -->
	<Section Name="ServiceBus">
		<Parameter Name="ListenerConnectionString" Value="Endpoint=sb://mysbnamespace.servicebus.windows.net/;SharedAccessKeyName=ListenAccessKey;SharedAccessKey=abc/5yj8/yMm+123j456gbbbb4bbbbbZtsvWjHLp+hM=" />
	</Section>
</Settings>
``` 
  2. In your `StatelessService`, `StatefulService` or `Actor` service implementation as a private instance member, or as a dependency-injected value using a dependency injection library, you'll need to create a single instance of a `QueuedWcfCommunicationClientFactory<IMyService>`. You'll use this factory to create client channels that can be used to communicate with your queued service via Azure Service Fabric:
```
	CodePackageActivationContext activationContext = FabricRuntime.GetActivationContext();

	ConfigurationSection eventPublishingConfigSection = activationContext.GetConfigurationPackageObject("Config").Settings.Sections["EventPublishing"];

	var connectionStringBuilder = new ServiceBusConnectionStringBuilder(eventPublishingConfigSection.Parameters["EventServiceBusSendConnectionString"].Value);

	string keyName = connectionStringBuilder.SharedAccessKeyName;
	string key = connectionStringBuilder.SharedAccessKey;

	var myFactory = QueuedCommunicationClientUtility.CreateCommunicationClientFactory<INotificationEvents>(new NetMessagingBinding(), keyName, key);
```   
  3. In your service implementation method(s), create a `WcfCommunicationClient` and use it to enqueue a message to your service:
```
    var myClient = myFactory.CreateDefaultClientAsync(new Uri("fabric:/MyServiceFabricApp/MyQueuedService"));
```
  In this example, `CreateDefaultClientAsync` is an extension method that comes with this Nuget package to simplify creating `WcfCommunicationClient` instances. It's there for convenience, but you don't have to use it, and you have the code available to you for your examination and modification.

  4. Call your queued service through the proxy:
```
	await myClient.DoMyActionAsync(new MyComplexType { MyProperty = "Something" });
```

These examples are just one way to use this package, not the only way. Please experiment and find the best way for your particular solutions.