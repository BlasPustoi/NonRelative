using System;
using System.Threading;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;

namespace DAL
{
    public class DynamoDBDAL
    {
        public static bool operationSucceeded;
        public static bool operationFailed;
        private static readonly string Ip = "localhost";
        private static readonly int Port = 8000;
        private static readonly string EndpointUrl = "http://" + Ip + ":" + Port;
        private static AmazonDynamoDBClient Client;
        public static CancellationTokenSource source = new CancellationTokenSource();
        public static CancellationToken token = source.Token;
        public static Document TableRecord;
        public static async Task<bool> DeletingTable_async(string tableName)
        {
            operationSucceeded = false;
            operationFailed = false;
            Task tblDelete = Client.DeleteTableAsync(tableName);
            try
            {
                await tblDelete;
            }
            catch (Exception ex)
            {
                Console.WriteLine("     ERROR: Failed to delete the table, because:\n            " + ex.Message);
                operationFailed = true;
                return (false);
            }
            Console.WriteLine("     -- Successfully deleted the table!");
            operationSucceeded = true;
            return (true);
        }
        public static async Task<bool> ReadingMovie_async(string year, string title, bool report, Table moviesTable)
        {
            Primitive hash = new Primitive(year, false);
            Primitive range = new Primitive(title, false);

            operationSucceeded = false;
            operationFailed = false;
            try
            {
                Task<Document> readMovie = moviesTable.GetItemAsync(hash, range, token);
                if (report)
                    Console.WriteLine("  -- Reading the {0} movie \"{1}\" from the Movies table...", year, title);
                TableRecord = await readMovie;
                if (TableRecord == null)
                {
                    if (report)
                        Console.WriteLine("     -- Sorry, that movie isn't in the Movies table.");
                    return (false);
                }
                else
                {
                    if (report)
                        Console.WriteLine("     -- Found it!  The movie record looks like this:\n" +
                                            TableRecord.ToJsonPretty());
                    operationSucceeded = true;
                    return (true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("     FAILED to get the movie, because: {0}.", ex.Message);
                operationFailed = true;
            }
            return (false);
        }
        public static async Task WritingNewMovie_async(Document newItem, Table moviesTable)
        {
            operationSucceeded = false;
            operationFailed = false;


            try
            {
                Task<Document> writeNew = moviesTable.PutItemAsync(newItem, token);
                Console.WriteLine("  -- Writing a new movie to the Movies table...");
                await writeNew;
                Console.WriteLine("      -- Wrote the item successfully!");
                operationSucceeded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("      FAILED to write the new movie, because:\n       {0}.", ex.Message);
                operationFailed = true;
            }

        }

        private static bool IsPortInUse()
        {
            bool isAvailable = true;
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();
            foreach (IPEndPoint endpoint in tcpConnInfoArray)
            {
                if (endpoint.Port == Port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;
        }

        public static AmazonDynamoDBClient createClient(bool useDynamoDbLocal)
        {
            if (useDynamoDbLocal)
            {
                var portUsed = IsPortInUse();
                if (portUsed)
                {
                    Console.WriteLine("The local version of DynamoDB is NOT running.");
                    return (null);
                }

                Console.WriteLine("  -- Setting up a DynamoDB-Local client (DynamoDB Local seems to be running)");
                AmazonDynamoDBConfig ddbConfig = new AmazonDynamoDBConfig();
                ddbConfig.ServiceURL = EndpointUrl;
                try
                {
                    Client = new AmazonDynamoDBClient(ddbConfig);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("     FAILED to create a DynamoDBLocal client; " + ex.Message);
                    return null;
                }
            }
            else
            {
                Client = new AmazonDynamoDBClient();
            }

            return Client;
        }
    }
}
