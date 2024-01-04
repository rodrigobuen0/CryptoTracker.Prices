using Newtonsoft.Json;
using StackExchange.Redis;

namespace CryptoTracker.Prices
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ConnectionMultiplexer _redis;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            var configurationOptions = ConfigurationOptions.Parse("localhost:6379,abortConnect=false,connectTimeout=2000,responseTimeout=50");
            _redis = ConnectionMultiplexer.Connect(configurationOptions);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redis.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                string precosAtivos = await ObtemPrecosAtivosPortfolio();
                await db.StringSetAsync("PortforlioAssetsPrices", precosAtivos);

                await Task.Delay(300000, stoppingToken);
            }
        }

        private async Task<string> ObtemPrecosAtivosPortfolio()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest");
            request.Headers.Add("X-CMC_PRO_API_KEY", "SUA CHAVE DE API DA COINMARKETCAP");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            var result = await response.Content.ReadAsStringAsync();
            return result;

        }
    }
}
