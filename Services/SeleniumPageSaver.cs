using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace KartChronoWrapper.Services
{
    public class SeleniumPageSaver : ISaveSessionService
    {
        private string _url;

        public SeleniumPageSaver()
        {
            _url = Environment.GetEnvironmentVariable("TRACK_URL") ?? string.Empty;
        }

        public async Task SaveSession()
        {
            var htmlContent = await SavePageHtml(_url);
            if(htmlContent is not null)
                await new S3FilesService().SaveCurrentSession(htmlContent);
            else
                Log.Warning("empty html page content");
        }

        public static async Task<string?> SavePageHtml(string url)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // Запускаем в режиме без графического интерфейса
            using (var driver = new ChromeDriver(options))
            {
                try
                {
                    await driver.Navigate().GoToUrlAsync(url);
                    /// Ожидание загрузки страницы
                    // await Task.Delay(2000);
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    // Ожидание загрузки элемента (например, <body>)
                    wait.Until(d => d.FindElement(By.TagName("body")));
                    ///
                    string pageSource = driver.PageSource;

                    return pageSource;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    return null;
                }
                finally
                {
                    driver.Quit();
                }
            }
        }
    }
}
