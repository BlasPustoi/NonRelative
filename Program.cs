using System;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

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
        static void Main(string[] args)
        {
            
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
            var filterBuilderUser = Builders<user>.Filter;
            var filterUser = filterBuilderUser.Empty;
            var resultUser = mongoUser.Find(filterUser).ToList();
            var result = mongo.Find(filter).ToList();
            user CurrentUser=new user();
            bool leave = false;
            
            while (true&&leave==false)
            {
                Console.WriteLine("Type 1 to login, Type 2 to register a new account!");
                int choice = Int32.Parse(Console.ReadLine());
                switch (choice) {
                    case 1:
                        Console.WriteLine("Type your LastName and Password");
                        string lname = Console.ReadLine();
                        int pas = Int32.Parse(Console.ReadLine());
                        filterUser = filterBuilderUser.Eq("LastName", lname) & filterBuilderUser.Eq("Password", pas);
                        resultUser = mongoUser.Find(filterUser).ToList();
                        if (resultUser.Count == 1)
                        {
                            Console.WriteLine("Welcome to Crappy " + resultUser[0].FirstName);
                            CurrentUser = resultUser[0];
                            leave =true;
                            
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

                int choice = Int32.Parse(Console.ReadLine());
                switch (choice)
                {
                    case 1:
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
                        filterBuilderUser = Builders<user>.Filter;
                        filterUser = filterBuilderUser.Eq("Email",CurrentUser.Email);
                        resultUser = mongoUser.Find(filterUser).ToList();
                        if (resultUser.Count == 1)
                        {
                            mongoPost.InsertOne(a);
                            filterUser = Builders<user>.Filter.Eq("Email", CurrentUser.Email);
                            var update = Builders<user>.Update.Push("posts", a.Id);
                            mongoUser.UpdateOne(filterUser, update);
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
                        Console.WriteLine("Enter title of the Post");
                        string Inputtitle = Console.ReadLine();
                        filterPost = filterBuilderPost.Eq("title", Inputtitle);
                        resultPost = mongoPost.Find(filterPost).ToList();
                        if (resultPost.Count == 1)
                        {
                            Console.WriteLine("Write your comment now");
                            string InputComment = Console.ReadLine();
                            comment b = new comment { text = InputComment, SentBy = (CurrentUser.FirstName +" "+ CurrentUser.LastName) };
                            mongoComment.InsertOne(b);
                            ObjectId id = resultPost[0].Id;
                            filterPost = filterBuilderPost.Eq("_id", id);
                            var update1 = Builders<Post>.Update.Push("Comment", id);
                            mongoPost.UpdateOne(filterPost, update1);
                        }
                        break;
                    case 4:
                        filterUser = filterBuilderUser.Empty;
                        resultUser = mongoUser.Find(filterUser).ToList();
                        for(int i = 0; i < resultUser.Count; i++)
                        {
                            Console.WriteLine(resultUser[i].FirstName +" "+ resultUser[i].LastName);
                            Console.WriteLine();
                        }
                        break;
                    case 5:
                        Console.WriteLine("Type user First name");
                        string InputName = Console.ReadLine();
                        Console.WriteLine("Type user Last name");
                        string InputLname = Console.ReadLine();
                        filterUser = filterBuilderUser.Eq("FirstName", InputName) & filterBuilderUser.Eq("LastName", InputLname);
                        resultUser = mongoUser.Find(filterUser).ToList();
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
                        break;
                }
            }
        }
    }
}
