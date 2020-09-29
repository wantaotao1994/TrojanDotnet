namespace Winter.Utility
{
    public static class Helper
    {
        public static string GetPacAddress(int port)
        {
            return  "http://127.0.0.1:" +port + "/";
        }
    }
}