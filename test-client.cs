using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Inserisci la chiave API: ");
        string apiKey = Console.ReadLine();

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Chiave vuota");
            return;
        }

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            try
            {
                HttpResponseMessage response = await client.PostAsync("http://localhost:3000/api/validate-key", null);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Chiave valida!");
                }
                else
                {
                    Console.WriteLine("Chiave non valida");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore: " + ex.Message);
            }
        }
    }
}