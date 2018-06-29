# ServiceFabricQueuedServices.Clients
Helper library for setting up `WcfCommunicationClient`s for queued calls using Azure Service Bus ***queues***. This functionality *WILL NOT* work with Azure Service Bus ***topics + subscriptions***. All that's required is a  `NetMessagingBinding` of your own configuration, and an Azure Service Bus connection string with `send` privileges.
