using CrawlLib.Data;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace CrawlLib
{
    public class Crawler
    {
        const string Path = $"C:\\browserdriver";
        const int WAIT_TIME = 1;
        const string PathLogFile = @"D:\log_file.txt";
        const string URL = "https://www.dhl.com/vn-en/home.html";
        public Crawler()
        {

        }
        void Sleep(int second)
        {
            Thread.Sleep(second * 1100);
        }
        (string, string) GetMileStoneReturnDay(IWebElement firstMile, List<MileStone> mileStones)
        {
            // firstMile = c-tracking-result--checkpoint
            var spans = firstMile.FindElement(By.ClassName("c-tracking-result--checkpoint-left")).FindElements(By.TagName("span"));
            var right = firstMile.FindElement(By.ClassName("c-tracking-result--checkpoint-right"));
            var status = right.FindElement(By.TagName("p")).Text;// status
            var serviceArea = right.FindElement(By.ClassName("c-tracking-result--checkpoint--more")).Text;
            var description = right.FindElement(By.ClassName("c-tracking-result-pieceid")).Text;
            mileStones.Add(new MileStone
            {
                Day = spans[0].Text,
                Date = spans[1].Text,
                LocalTime = spans[2].Text,
                Status = status,
                ServiceArea = serviceArea,
                Description = description
            });

            return (spans[0].Text, spans[1].Text); // day
        }
        bool HasDescriptionField(IWebElement description)
        {
            try
            {
                description.FindElement(By.ClassName("c-tracking-result-pieceid"));
                return true;
            }
            catch
            {
                return false;
            }
        }
        void AddAnotherMileStone((string, string) dateTime, List<MileStone> mileStones, IReadOnlyCollection<IWebElement> lis)
        {

            for (int i = 1; i < lis.Count; i++)
            {
                var local = lis.ElementAt(i).FindElement(By.ClassName("c-tracking-result--checkpoint-time")).Text;
                var serviceArea = lis.ElementAt(i).FindElement(By.ClassName("c-tracking-result--checkpoint--more")).Text;
                var status = lis.ElementAt(i).FindElement(By.TagName("p")).Text;
                var description = HasDescriptionField(lis.ElementAt(i)) ? lis.ElementAt(i).FindElement(By.ClassName("c-tracking-result-pieceid")).Text : string.Empty;
                mileStones.Add(new MileStone
                {
                    Day = dateTime.Item1,
                    Date = dateTime.Item2,
                    LocalTime = local,
                    ServiceArea = serviceArea,
                    Status = status,
                    Description = description
                });
            }
        }
        // element is div
        void AddOrderHistories(IReadOnlyCollection<IWebElement> elements, List<MileStone> mileStones)
        {
            foreach (var element in elements)
            {
                var lis = element.FindElements(By.TagName("li"));
                var dateTime = GetMileStoneReturnDay(lis[0], mileStones);
                AddAnotherMileStone(dateTime, mileStones, lis);
            }
        }
        string WriteToJson(List<MileStone> mileStones)
        {
            HandleMileStone(mileStones);
            var json = JsonConvert.SerializeObject(mileStones, Formatting.Indented);
            File.WriteAllText(@"D:\path.json", json);
            return json;
        }
        string GetValidDescription(string description)
        {
            if (description.Trim().Length == 0) return string.Empty;
            var index = description.LastIndexOf("\n");
            var first = description.Substring(0, index);
            var second = description.Substring(index).Replace(" ", "");
            return first + second;
        }
        void HandleMileStone(List<MileStone> mileStones)
        {
            for (int i = 0; i < mileStones.Count; i++)
            {
                mileStones[i].Description = GetValidDescription(mileStones[i].Description);
            }
        }
        bool CheckIfExist(ChromeDriver driver, By by, string element)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch
            {
                LogFile($"Can not find out {element}");
                var all = driver.FindElement(By.XPath("//*"));
                LogFile(all.GetAttribute("outerHTML"));
                return false;
            }
        }
        void LogFile(string status)
        {
            using (StreamWriter sw = File.CreateText(PathLogFile))
            {
                sw.WriteLine(status);
            }
        }
        ChromeOptions GetOptions()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--incognito");
            options.AddArgument("--excludeSwitches");
            options.AddArgument("headless");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddExcludedArgument("--enable-automation");
            options.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
            options.AddArgument("--Accept-Language=en-GB,en,q=0.5");

            return options;
        }
        public string Crawl(string trackingNumber)
        {
            var mileStones = new List<MileStone>();
            var options = GetOptions();

            var driver = new ChromeDriver(Path, options);
            // settin driver
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);

            driver.Navigate().GoToUrl(URL);

            Sleep(WAIT_TIME);

            // accpet all
            if(CheckIfExist(driver, By.Id("accept-recommended-btn-handler"), "button accept cookie"))
            {
                driver.FindElement(By.Id("accept-recommended-btn-handler")).Click();
            }

            Sleep(WAIT_TIME);

            // fill in data
            if(CheckIfExist(driver, By.XPath("/html/body/div[5]/div[2]/div[2]/div[1]/div[1]/form/div/div/input"), "fill in data"))
            {
                driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div[2]/div[1]/div[1]/form/div/div/input")).SendKeys(trackingNumber);
            }

            Sleep(WAIT_TIME);

            // tracking
            if(CheckIfExist(driver, By.XPath("/html/body/div[5]/div[2]/div[2]/div[1]/div[1]/form/div/div/button"), "button tracking"))
            {
                driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div[2]/div[1]/div[1]/form/div/div/button")).Click();
            }

            Sleep(WAIT_TIME);


            //
            if(CheckIfExist(driver, By.XPath("//*[@id=\"c-tracking-result--checkpoints-dropdown-button\"]"), "button show history"))
            {
                driver.FindElement(By.XPath("//*[@id=\"c-tracking-result--checkpoints-dropdown-button\"]")).Click();
            }

            // Get all element 
            var historiesContainer = driver.FindElement(By.XPath("//*[@id=\"wcag-main-content\"]/div[1]/div[2]/div[4]/div/div/div[5]/div/div[2]"));

            var histories = historiesContainer.FindElements(By.ClassName("c-tracking-result--checkpoint"));

            var watch = System.Diagnostics.Stopwatch.StartNew();
            AddOrderHistories(histories, mileStones);
            watch.Stop();
            var result = WriteToJson(mileStones);
            LogFile($"Time excute: {watch.ElapsedMilliseconds / 1000} seconds");
            driver.Quit();
            return result;
        }
    }
}
