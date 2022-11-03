using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

const string Path = @"C:\";
void PauseFor(int second)
{
    Thread.Sleep(second * 1000);
}
bool IsCheckedHuman(IWebDriver driver)
{
    try
    {
        driver.FindElement(By.ClassName("evkjyb01"));
        Console.WriteLine("Do Not Checking Human");

        return false;
    }
    catch
    {
        Console.WriteLine("Checking Human");
        return true;
    }
}
bool CheckExistElementByXPath(IWebDriver driver, string xPath)
{
    try
    {
        driver.FindElement(By.XPath(xPath));
        return false;
    }
    catch
    {
        return true;
    }

}
void SolveHuman(IWebDriver driver)
{
    const string path = "\"/html/body/div/div/div[2]/div[2]/p[contains(., 'Press')]\"";
    if (CheckExistElementByXPath(driver, path))
    {
        Actions action = new Actions(driver);
        var press = driver.FindElement(By.XPath("//*[contains(text(),'Press')]"));
        action.ClickAndHold(press).Perform();
        PauseFor(10);
        action.Release();
    }
    else
    {
        Console.WriteLine("Do Not Found Div");
    }
}
void Crawl()
{
    Console.WriteLine("Start Crawl");
    var options = new ChromeOptions();

    options.AddArgument("--start-maximized");
    options.AddArgument("--ignore-certificate-errors");
    options.AddArgument("--disable-popup-blocking");
    options.AddArgument("--incognito");
    options.AddArgument("--excludeSwitches");
    options.AddArgument("--disable-blink-features=AutomationControlled");
    options.AddExcludedArgument("--enable-automation");

    var driver = new ChromeDriver(Path, options);
    // settin driver
    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

    driver.Navigate().GoToUrl("https://www.walmart.ca/en");
    if (IsCheckedHuman(driver))
    {
        SolveHuman(driver);
    }
    else
    {

    }
    Thread.Sleep(3000);
   

}
Crawl();    