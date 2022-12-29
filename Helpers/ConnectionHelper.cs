using Npgsql;

namespace ContactPro.Helpers
{
    public static class ConnectionHelper //static means I don't have to create an instance. It is only called once.
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            //var connectionString = configuration.GetSection("pgSettings")["pgConnection"];
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL"); //Environment is different on each platform. both the configuration and the environment can not be true. One or the other will run.
           
            return String.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl); //ternary operator!
        }
        //build a connection string from the environment.
        private static string BuildConnectionString(string databaseUrl)
        //Universal Resource Identity = Identifies a resource. Can also be a url. URL is used to find a resource.
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(';');
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }
    }
}
