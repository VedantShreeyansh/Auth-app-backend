namespace auth_app_backend
{
    internal class MyCouchClient
    {
        private string dbUrl;
        private string dbName;
        private string username;
        private string password;

        public MyCouchClient(string dbUrl, string dbName, string username, string password)
        {
            this.dbUrl = dbUrl;
            this.dbName = dbName;
            this.username = username;
            this.password = password;
        }
    }
}