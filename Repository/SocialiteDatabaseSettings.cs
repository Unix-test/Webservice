namespace Webserver.Repository
{
    public class SocialiteDatabaseSettings: ISocialiteDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}