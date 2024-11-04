using ICD_integration;

namespace IcdApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("enter a disease or condition to search");
            string? query = Console.ReadLine();

            IcdSearchService searchService = new IcdSearchService();
            try
            {
                string result = await searchService.SearchDiseasesAsync(query!);
                searchService.DisplaySearchResults(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}