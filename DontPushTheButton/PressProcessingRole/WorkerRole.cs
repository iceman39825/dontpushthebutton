using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Data.SqlClient;

namespace PressProcessingRole
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "dontpushthebutton";
        private string DBConnectionString = System.Configuration.ConfigurationManager.AppSettings.Keys[0];

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            Client.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        string queryString = "INSERT INTO ButtonPresses (DatePressed) VALUES (@date)";

                        using (SqlConnection connection =
                            new SqlConnection(DBConnectionString))
                        {
                            // Create the Command and Parameter objects.
                            SqlCommand command = new SqlCommand(queryString, connection);
                            command.Parameters.AddWithValue("@date", receivedMessage.GetBody<DateTime>());

                            connection.Open();
                            command.ExecuteNonQuery();
                        }

                        receivedMessage.Complete();
                    }
                    catch
                    {
                        // Handle any message processing specific exceptions here
                    }
                });

            CompletedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string connectionString =
                "Endpoint=sb://dontpushthebutton.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/K6+b3UrHgftN1J//TK+SjbPcRkz1MwLM1fDvef/yCc=";

            // Initialize the connection to Service Bus Queue
            Client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            Client.Close();
            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
