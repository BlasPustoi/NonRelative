using System;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Threading;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;
using DAL;
namespace Mongo
{
    [BsonIgnoreExtraElements]
    public class Post
    {
        public string title { get; set; }
        public string text { get; set; }
        public int likes { get; set; }
        public List<ObjectId> Comment { get; set; }
        public DateTime Date { get; set; }
        [BsonId]
        public ObjectId Id { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class comment
    {
        public string text { get; set; }
        public int like { get; set; }
        public string SentBy { get; set; }
        [BsonId]
        public ObjectId Id { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class user
    {
        public user()
        {
            ;
        }
        public user(string a,int b, string c,string d, List<string> v,List<ObjectId> p,List<ObjectId> f)
        {
            this.Email = a;
            this.Password = b;
            this.FirstName = c;
            this.LastName = d;
            this.Interests = v;
            this.posts = p;
            this.Friends = f;
        }
        public string Email { get; set; }
        public int Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Interests { get; set; }
        public List<ObjectId> posts { get; set; }
        public List<ObjectId> Friends { get; set; }
        [BsonId]
        public ObjectId Id { get; set; }
    }
    class Program
    {
        private static AmazonDynamoDBClient Client;
        public static Table moviesTable;


        static void Main(string[] args)
        {
            Program.Client = DAL.DynamoDBDAL.createClient(true);
            DAL.DynamoDBDAL.createClient(true);
            var request = new CreateTableRequest
            {
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = "S"
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Title",
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Id",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "Title",
                        KeyType = "Range"
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits=5
                },
                TableName="JustCreateTable"
            };
            Program.Client.CreateTable(request);
            moviesTable = Table.LoadTable(Program.Client, "JustCreateTable");
            
            // MONGO PART //

            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("social");
            
            IMongoCollection<BsonDocument> mongo = database.GetCollection<BsonDocument>("Users");
            IMongoCollection<user> mongoUser = database.GetCollection<user>("Users");
            IMongoCollection<Post> mongoPost = database.GetCollection<Post>("Posts");
            IMongoCollection<comment> mongoComment = database.GetCollection<comment>("Comments");
            var filterBuilderPost = Builders<Post>.Filter;
            var filterPost = filterBuilderPost.Empty;
            var resultPost = mongoPost.Find(filterPost).ToList();
            var filterBuilderComment = Builders<comment>.Filter;
            var filterComment = filterBuilderComment.Empty;
            var resultComment = mongoComment.Find(filterComment).ToList();
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Empty;
            //var filterBuilderUser = Builders<user>.Filter;
            //var filterUser = filterBuilderUser.Empty;
            //var resultUser = mongoUser.Find(filterUser).ToList();
            var result = mongo.Find(filter).ToList();
            user CurrentUser=new user();
            bool leave = false;
            
            while (true&&leave==false)
            {
                Console.WriteLine("Type 1 to login, Type 2 to register a new account!");
                int choice = Int32.Parse(Console.ReadLine());
                switch (choice) {
                    case 1:
                        {
                            Console.WriteLine("Type your LastName and Password");
                            string lname = Console.ReadLine();
                            int pas = Int32.Parse(Console.ReadLine());
                            var filterBuilderUser = Builders<user>.Filter;
                            var filterUser = filterBuilderUser.Eq("LastName", lname) & filterBuilderUser.Eq("Password", pas);
                            var resultUser = mongoUser.Find(filterUser).ToList();
                            if (resultUser.Count == 1)
                            {
                                Console.WriteLine("Welcome to Crappy " + resultUser[0].FirstName);
                                CurrentUser = resultUser[0];
                                leave = true;

                            }
                        }
                        break;
                    case 2:
                        Console.WriteLine("Enter your Email");
                        string Email = Console.ReadLine();
                        Console.WriteLine("Create a password");
                        int Password = Int32.Parse(Console.ReadLine());
                        Console.WriteLine("Enter your First Name");
                        string FName = Console.ReadLine();
                        Console.WriteLine("Enter your Last Name");
                        string Lname = Console.ReadLine();
                        Console.WriteLine("Enter your interests");
                        List<string> interests = new List<string>();
                        interests.Add(Console.ReadLine());
                        List<ObjectId> posts = new List<ObjectId>();
                        List<ObjectId> friends = new List<ObjectId>();
                        try
                        {
                            user a = new user(Email, Password, FName, Lname, interests, posts, friends);
                            mongoUser.InsertOneAsync(a);
                            Console.WriteLine("User created!");
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        break;
                }
            }
            while (true)
            {
                Console.WriteLine("Type 1 to add a post");
                Console.WriteLine("Type 2 to see all posts");
                Console.WriteLine("Type 3 to write a comment to a post");
                Console.WriteLine("Type 4 to see all users");
                Console.WriteLine("Type 5 to add/delete friend");
                Console.WriteLine("Type 6 to add all posts to DynamoDB database");
                Console.WriteLine("Type 7 to show all posts in DynamoDB database");
                Console.WriteLine("Type 8 to update a post in DynamoDB database");

                int choice = Int32.Parse(Console.ReadLine());
                Console.Clear();
                switch (choice)
                {
                    case 1:
                        {

                            Console.WriteLine("Enter title");
                            string title = Console.ReadLine();
                            Console.WriteLine("Enter text");
                            string text = Console.ReadLine();
                            List<ObjectId> coments = new List<ObjectId>();
                            Post a = new Post
                            {
                                title = title,
                                text = text,
                                likes = 0,
                                Comment = coments,
                                Date = DateTime.Now
                            };
                            var filterBuilderUser = Builders<user>.Filter;
                            var filterUser = filterBuilderUser.Eq("Email", CurrentUser.Email);
                            var resultUser = mongoUser.Find(filterUser).ToList();
                            if (resultUser.Count == 1)
                            {
                                mongoPost.InsertOne(a);
                                filterUser = Builders<user>.Filter.Eq("Email", CurrentUser.Email);
                                var update = Builders<user>.Update.Push("posts", a.Id);
                                mongoUser.UpdateOne(filterUser, update);
                            }
                        }
                        break;
                    case 2:
                        
                        filterBuilderPost = Builders<Post>.Filter;
                        filterPost = filterBuilderPost.Empty;
                        resultPost = mongoPost.Find(filterPost).ToList();
                        for (int i = 0; i < resultPost.Count; i++)
                        {
                            Console.WriteLine(resultPost[i].title);
                            Console.WriteLine(resultPost[i].text);
                            Console.WriteLine(resultPost[i].likes);
                            Console.WriteLine("///");
                        }
                        
                        break;
                    case 3:
                        {
                            Console.WriteLine("Enter title of the Post");
                            string Inputtitle = Console.ReadLine();
                            filterPost = filterBuilderPost.Eq("title", Inputtitle);
                            resultPost = mongoPost.Find(filterPost).ToList();
                            if (resultPost.Count == 1)
                            {
                                Console.WriteLine("Write your comment now");
                                string InputComment = Console.ReadLine();
                                comment b = new comment { text = InputComment, SentBy = (CurrentUser.FirstName + " " + CurrentUser.LastName) };
                                mongoComment.InsertOne(b);
                                ObjectId id = resultPost[0].Id;
                                filterPost = filterBuilderPost.Eq("_id", id);
                                var update1 = Builders<Post>.Update.Push("Comment", id);
                                mongoPost.UpdateOne(filterPost, update1);
                            }
                            break;
                        }
                    case 4:
                        {
                            var filterBuilderUser = Builders<user>.Filter;
                            var filterUser = filterBuilderUser.Empty;
                            var resultUser = mongoUser.Find(filterUser).ToList();
                            for (int i = 0; i < resultUser.Count; i++)
                            {
                                Console.WriteLine(resultUser[i].FirstName + " " + resultUser[i].LastName);
                                Console.WriteLine();
                            }
                        }
                        break;
                    case 5:
                        { 
                        Console.WriteLine("Type user First name");
                        string InputName = Console.ReadLine();
                        Console.WriteLine("Type user Last name");
                        string InputLname = Console.ReadLine();
                        var filterBuilderUser = Builders<user>.Filter;
                        var filterUser = filterBuilderUser.Eq("FirstName", InputName) & filterBuilderUser.Eq("LastName", InputLname);
                        var resultUser = mongoUser.Find(filterUser).ToList();
                        var filterCurrUser = filterBuilderUser.Eq("LastName", CurrentUser.LastName) & filterBuilderUser.Eq("Password", CurrentUser.Password);
                        var resultCurrUser = mongoUser.Find(filterCurrUser).ToList();
                        CurrentUser = resultCurrUser[0];
                            if (resultUser.Count == 1)
                            {
                                Console.WriteLine("first");
                                if (CurrentUser.Friends.Count == 0)
                                {
                                    Console.WriteLine("fifth");
                                    filterUser = filterBuilderUser.Eq("_id", CurrentUser.Id);
                                    var update2 = Builders<user>.Update.Push("Friends", resultUser[0].Id);
                                    mongoUser.UpdateOne(filterUser, update2);
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("second");
                                    for (int i = 0; i < CurrentUser.Friends.Count; i++)
                                    {
                                        if (CurrentUser.Friends[i] == resultUser[0].Id)
                                        {
                                            Console.WriteLine("third");
                                            filterUser = filterBuilderUser.Eq("_id", CurrentUser.Id);
                                            var update = Builders<user>.Update.Pull("Friends", resultUser[0].Id);
                                            mongoUser.UpdateOne(filterUser, update);
                                            break;
                                        }
                                        else if (i == CurrentUser.Friends.Count - 1)
                                        {
                                            Console.WriteLine("fourth");
                                            filterUser = filterBuilderUser.Eq("_id", CurrentUser.Id);
                                            var update2 = Builders<user>.Update.Push("Friends", resultUser[0].Id);
                                            mongoUser.UpdateOne(filterUser, update2);
                                            break;
                                        }
                                    }
                                }
                            }

                        }
                        break;
                    case 6:
                        {
                            filterBuilderPost = Builders<Post>.Filter;
                            filterPost = filterBuilderPost.Empty;
                            resultPost = mongoPost.Find(filterPost).ToList();
                            for (int i = 0; i < resultPost.Count; i++)
                            {
                                Document newItemDocument = new Document();
                                newItemDocument["Id"] = resultPost[i].Id.ToString();
                                newItemDocument["Title"] = resultPost[i].title;
                                newItemDocument["Text"] = resultPost[i].text;
                                newItemDocument["Date"] = resultPost[i].Date.ToString();
                                newItemDocument["Likes"] = resultPost[i].likes;
                                newItemDocument["Comments"] = resultPost[i].Comment.ToJson();
                                DAL.DynamoDBDAL.WritingNewMovie_async(newItemDocument,moviesTable).Wait();
                                DAL.DynamoDBDAL.ReadingMovie_async(resultPost[i].Id.ToString(), resultPost[i].title, true,moviesTable).Wait();
                            }
                            break;
                        }
                    case 7:
                        string tableName = "JustCreateTable";
                        Table ThreadTable = Table.LoadTable(Program.Client, tableName);
                        ScanFilter scanFilter = new ScanFilter();
                        Search search = ThreadTable.Scan(scanFilter);
                        List<Document> documentList = new List<Document>();
                        documentList = search.GetNextSet();
                        foreach(var document in documentList)
                        {
                            Console.WriteLine(document.ToJsonPretty());
                        }


                        break;
                    case 8:
                        {
                            Console.WriteLine("Enter title of the Post");
                            string Inputtitle = Console.ReadLine();
                            filterPost = filterBuilderPost.Eq("title", Inputtitle);
                            resultPost = mongoPost.Find(filterPost).ToList();
                            if (resultPost.Count == 1)
                            {
                                Dictionary<string, AttributeValue> key = new Dictionary<string, AttributeValue>
                                {
                                    { "Id", new AttributeValue { S = resultPost[0].Id.ToString() } },
                                    { "Title", new AttributeValue { S = resultPost[0].title } }
                                };
                                Console.WriteLine("Press 1 to update Text");
                                Console.WriteLine("Press 2 to update Date");
                                Console.WriteLine("Press 3 to update Likes");
                                Console.WriteLine("Press 4 to update Comments");
                                string c = Console.ReadLine();
                                switch (c)
                                {
                                    case "1":
                                        {
                                            Console.WriteLine("Enter new text now");
                                            string input = Console.ReadLine();
                                            Dictionary<string, AttributeValueUpdate> updates = new Dictionary<string, AttributeValueUpdate>();
                                            updates["Text"] = new AttributeValueUpdate()
                                            {
                                                Action = AttributeAction.PUT,
                                                Value = new AttributeValue { S = input }
                                            };
                                            UpdateItemRequest req = new UpdateItemRequest
                                            {
                                                TableName = "JustCreateTable",
                                                Key = key,
                                                AttributeUpdates = updates
                                            };

                                            Program.Client.UpdateItem(req);
                                            DAL.DynamoDBDAL.ReadingMovie_async(resultPost[0].Id.ToString(), resultPost[0].title, true, moviesTable).Wait();
                                            break;
                                        }
                                    case "2":
                                        {
                                            Console.WriteLine("Enter new Date now");
                                            string input = Console.ReadLine();
                                            Dictionary<string, AttributeValueUpdate> updates = new Dictionary<string, AttributeValueUpdate>();
                                            updates["Date"] = new AttributeValueUpdate()
                                            {
                                                Action = AttributeAction.PUT,
                                                Value = new AttributeValue { S = input }
                                            };
                                            UpdateItemRequest req = new UpdateItemRequest
                                            {
                                                TableName = "JustCreateTable",
                                                Key = key,
                                                AttributeUpdates = updates
                                            };

                                            Program.Client.UpdateItem(req);
                                            DAL.DynamoDBDAL.ReadingMovie_async(resultPost[0].Id.ToString(), resultPost[0].title, true, moviesTable).Wait();
                                            break;
                                        }
                                    case "3":
                                        {
                                            Console.WriteLine("Enter new Like count now");
                                            string input = Console.ReadLine();
                                            Dictionary<string, AttributeValueUpdate> updates = new Dictionary<string, AttributeValueUpdate>();
                                            updates["Likes"] = new AttributeValueUpdate()
                                            {
                                                Action = AttributeAction.PUT,
                                                Value = new AttributeValue { S = input }
                                            };
                                            UpdateItemRequest req = new UpdateItemRequest
                                            {
                                                TableName = "JustCreateTable",
                                                Key = key,
                                                AttributeUpdates = updates
                                            };

                                            Program.Client.UpdateItem(req);
                                            DAL.DynamoDBDAL.ReadingMovie_async(resultPost[0].Id.ToString(), resultPost[0].title, true, moviesTable).Wait();
                                            break;
                                        }
                                    case "4":
                                        {
                                            Console.WriteLine("Enter new Comments now");
                                            string input = Console.ReadLine();
                                            Dictionary<string, AttributeValueUpdate> updates = new Dictionary<string, AttributeValueUpdate>();
                                            updates["Comments"] = new AttributeValueUpdate()
                                            {
                                                Action = AttributeAction.PUT,
                                                Value = new AttributeValue { S = input }
                                            };
                                            UpdateItemRequest req = new UpdateItemRequest
                                            {
                                                TableName = "JustCreateTable",
                                                Key = key,
                                                AttributeUpdates = updates
                                            };

                                            Program.Client.UpdateItem(req);
                                            DAL.DynamoDBDAL.ReadingMovie_async(resultPost[0].Id.ToString(), resultPost[0].title, true, moviesTable).Wait();
                                            break;
                                        }
                                }
                                
                            }
                            break;
                        }

                }
            }
            //*/
        }
        //
    }
}
