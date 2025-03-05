namespace BoardGameBackend.Settings
{
    public class MongoDbSettings 
    {
        public string? Host {get; set;}
        public int? Port {get; set;}
        public string? User{get; set;}
        public string? Password{get; set;}
        public string? ConnectionString {get; set;}
       
        public string ConnectionStringSetup  {
            get
            {
                Console.WriteLine($"mongodb://{ConnectionString}");
                return $"{ConnectionString}";
            } 
        }

    }
}