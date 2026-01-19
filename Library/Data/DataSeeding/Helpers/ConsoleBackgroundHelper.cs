namespace LibraryApi.Data.DataSeeding.Helpers
{
    public static class ConsoleBackgroundHelper
    {
        public static void WriteGreen(string message)
        {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = previous;
        }
    }
}
