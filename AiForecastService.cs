using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RegionForecastSystem
{
    public static class AiForecastService
    {
        private const string ApiKey = "rc-02b7066a2bb74cf0422ef6d686940a67b4100957d210c5b1";
        private const string BaseUrl = "https://developer.amd.com.cn/radeon/api/v1/chat/completions";
        private const string ModelName = "DeepSeek-V4-Flash";

        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<AiForecastResult> GetForecastAsync(string regionSummary)
        {
            var requestBody = new
            {
                model = ModelName,
                messages = new[]
                {
                    new {
                        role = "system",
                        content = "Ты - аналитик данных. Твоя задача - на основе предоставленных цифр вычислить точный среднегодовой темп прироста (CAGR) и спрогнозировать будущий объем ВРП. Выводи только конкретные цифры, проценты и факты. Избегай общих фраз."
                    },
                    new {
                        role = "user",
                        content = $"Данные: {regionSummary}\n\n" +
                                  "Вычисли среднегодовой темп прироста (CAGR) на основе исторических данных о ВРП. " +
                                  "Спрогнозируй объем ВРП на следующий год с учетом этого темпа. " +
                                  "Дай 3 конкретные рекомендации с указанием ожидаемого прироста в процентах. " +
                                  "Укажи 2 измеримых риска (например, падение на X%).\n\n" +
                                  "Ответ верни строго в JSON:\n" +
                                  "{\n" +
                                  "  \"forecast_text\": \"краткий текст с математическим обоснованием\",\n" +
                                  "  \"projected_grp_growth_percent\": 5.4,\n" +
                                  "  \"projected_grp_absolute\": \"1.55 трлн руб.\",\n" +
                                  "  \"recommendations\": [\"Рекомендация 1 (+2.1% к ВРП)\", \"Рекомендация 2 (+1.5%)\", \"Рекомендация 3\"],\n" +
                                  "  \"risks\": [\"Риск 1 (вероятность 30%)\", \"Риск 2\"]\n" +
                                  "}"
                    }
                },
                response_format = new { type = "json_object" },
                temperature = 0.1
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            try
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(60));
                HttpResponseMessage response = await _httpClient.PostAsync(BaseUrl, content, cts.Token);
                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new AiForecastResult { IsSuccess = false, ErrorMessage = $"Ошибка API: {responseString}" };

                JsonDocument doc = JsonDocument.Parse(responseString);
                string aiText = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                if (aiText.Contains("```json"))
                    aiText = aiText.Replace("```json", "").Replace("```", "").Trim();

                var result = JsonSerializer.Deserialize<AiForecastResult>(aiText);
                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                return new AiForecastResult { IsSuccess = false, ErrorMessage = "Ошибка: " + ex.Message };
            }
        }
    }
}