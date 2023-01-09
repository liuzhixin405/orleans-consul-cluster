using ServiceStack.Redis;

namespace eapi.RedisHelper
{
    public class ConnectionHelper
    {
        public RedisClient Conn()
        {
            return new RedisClient("127.0.0.1", 6379);
        }

        public static string GetTimeStamp()
        {
            //TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //return Convert.ToInt64(ts.TotalSeconds).ToString();
           return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        }
    }
}
