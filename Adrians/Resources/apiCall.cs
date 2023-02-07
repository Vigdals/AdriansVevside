using System.Diagnostics;
using System.Net.Http.Headers;

namespace Adrians.Resources
{
    public class ApiCall
    {
        public static string DoApiCall(string apiURL)
        {
            using var client = new HttpClient();
            // Get data response
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(apiURL).Result;
            var stringResponse = response.Content.ReadAsStringAsync().Result;
            return stringResponse;
        }

        public static void CheckIfSuccess(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body
                //Console.WriteLine(apiResultAsModel.StringResponse);
                Debug.WriteLine(response.StatusCode);
            }
            else
            {
                Debug.WriteLine("Feilkode: {0} og grunnen til dette er: ({1})", (int)response.StatusCode,
                    response.ReasonPhrase);
            }
        }
    }
}
