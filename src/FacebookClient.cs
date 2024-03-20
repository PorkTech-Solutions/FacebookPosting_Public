using System.Diagnostics.CodeAnalysis;
using FacebookPosting.SMTP;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace FacebookPosting;

public class FacebookClient : IDisposable
{
    internal const string SUCCESSFUL_LOGIN_AFTERMATH_URL = "https://www.facebook.com";

    private readonly WebDriver _webDriver;
    private readonly string _login;
    private readonly string _password;
    private FacebookSMTPClient? _facebookSMTPClient;

    private readonly Queue<IPostEntity> _postQueue = new Queue<IPostEntity>();
    public event Func<Task> OnQueueUpdateEnd;

    public FacebookClient(string login, string password, string? smtpPassword = null)
    {
        FirefoxOptions options = new FirefoxOptions();
        options.AddArguments("--verbose");
        options.AddArguments("--disable-notifications");

        if(smtpPassword is not null)
        {
            _facebookSMTPClient = new(smtpEmail: login, smtpPassword: smtpPassword);
        }

        FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true; // Optional
        ;

        if(options is not null)
            _webDriver = new FirefoxDriver(service, options); 
        else
            _webDriver = new FirefoxDriver(service, options); 


        //_webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(25);
        //_webDriver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(25);
        //_webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            

        _login = login;
        _password = password;
    }

    public async static Task<FacebookClient[]> LoadClientsWithLogin(string accountsDataPath)
    {
        string[] fileOutput = File.ReadAllLines(accountsDataPath);
        FacebookClient[] resultClients = new FacebookClient[fileOutput.Length];

        int clientsIndex = 0;
        foreach (var accountDataString in fileOutput)
        {
            string[] splittedAccountData = accountDataString.Split(' ');
            System.Console.WriteLine(splittedAccountData.Count());
            System.Console.WriteLine(splittedAccountData[0]);

            string yieldLogin = splittedAccountData[0];
            string yieldPassword = splittedAccountData[1];
            string? yieldProxyString = null;
            if(splittedAccountData.Length > 2)
            {
                yieldProxyString = splittedAccountData[2];
            }

            resultClients[clientsIndex] = new FacebookClient(yieldLogin, yieldPassword, yieldProxyString);
            await resultClients[clientsIndex].LoginIntoAccountAsync();

            clientsIndex++;
        }

        return resultClients;
    }

    
    public async static Task<FacebookClient[]> LoadClientsWithLogin(string[] accountStrings)
    {
        Stack<FacebookClient> resultClients = new();

        int clientsIndex = 0;
        foreach (var accountDataString in accountStrings)
        {
            string[] splittedAccountData = accountDataString.Split(' ');
            System.Console.WriteLine(splittedAccountData.Count());
            System.Console.WriteLine(splittedAccountData[0]);

            string yieldLogin = splittedAccountData[0];
            string yieldPassword = splittedAccountData[1];
            string? yieldSMTPString = null;
            if(splittedAccountData.Length > 2)
            {
                yieldSMTPString = splittedAccountData[2];
            }

            resultClients.Push(new FacebookClient(yieldLogin, yieldPassword, yieldSMTPString));
            
            bool loginResult = await resultClients.Peek().LoginIntoAccountAsync();
            if(loginResult == false)
            {
                resultClients.Pop().Dispose();
            }

            clientsIndex++;
        }

        return resultClients.ToArray();
    }

    public Task<bool> LoginIntoAccountAsync()
    {
        _webDriver.Manage().Window.Maximize();

        _webDriver.Navigate().GoToUrl("https://www.facebook.com");

        var acceptCookieButtons = _webDriver.FindElements(By.TagName("button")).Where(b => b.GetAttribute("data-testid") == "cookie-policy-manage-dialog-accept-button").ToList();

        System.Console.WriteLine($"accept cookie buttons: {acceptCookieButtons.Count}");

        acceptCookieButtons.FirstOrDefault()?.Click();


        var loginField = _webDriver.FindElements(
                By.Name("email")
            );
        System.Console.WriteLine("logins:" + loginField.Count);
        var passField = _webDriver.FindElements(
                By.Name("pass")
            );

        var submitButton = _webDriver.FindElements(By.TagName("button")).First(b => b.GetAttribute("name") == "login");
            //.FindElement(By.ClassName("x1i10hfl"));

        loginField.Last().SendKeys(_login);
        passField.Last().SendKeys(_password);

        submitButton.Click();

        if (IsAccountEmailTypeBlocked(_webDriver))
        {
            if (_facebookSMTPClient is null)
            {
                return Task.FromResult(false);
            }
            
            EnterFacebookCode();
        }

        Thread.Sleep(4000);

        return Task.FromResult(IsSuccessfullyLoggedIn(_webDriver));

        //return Task.FromResult(true);
    }

    private void EnterFacebookCode()
    {
        

        for(int i = 0; i < 4; i++)
        {
            _webDriver
                .FindElements(By.CssSelector("div.xjbqb8w"))
                .Last()
                .Click();

            Thread.Sleep(3000);
        }

        _webDriver
            .FindElements(By.CssSelector("input.x1i10hfl"))
            .Last()
            .SendKeys(_facebookSMTPClient!.GetFacebookCodeGuarantee());
        
        Thread.Sleep(2000);

            
        _webDriver
            .FindElements(By.CssSelector("div.xjbqb8w"))
            .Last()
            .Click();
    }

    public async Task PostToGroup(FacebookGroupData facebookGroup, FacebookPostData postData)
    {
        await facebookGroup.PostSubmitMessageExecutor.Execute(_webDriver, postData);
    }

    public Task QueueAddAsync([NotNull]IPostEntity entity)
    {
        System.Console.WriteLine(entity is null);
        _postQueue.Enqueue(entity);

        return Task.CompletedTask;
        
        //await OnQueueUpdateEnd();
    }

    public virtual async Task ExecuteQueue()
    {
        while(_postQueue.Count > 0)
        {
            IPostEntity postEntity = _postQueue.Dequeue();

            _webDriver.Navigate().GoToUrl(postEntity.FacebookGroupData.FullURL);
            await Task.Delay(20000);

            await postEntity.FacebookGroupData.PostSubmitMessageExecutor.Execute(_webDriver, postEntity.PostData);
            //await Task.Delay(20000);
        }
    }

    public void Dispose()
    {
        _webDriver.Dispose();
    }

    
    public bool IsAccountEmailTypeBlocked(WebDriver driver)
    {
        var resultCollection = driver.FindElements(By.CssSelector(".x2bj2ny"));

        return resultCollection.Count == 1;
    }

    public bool IsSuccessfullyLoggedIn(WebDriver driver)
    {
        Uri currentUrl = new Uri(driver.Url);

        Console.WriteLine(currentUrl.LocalPath);

        return currentUrl.LocalPath == "/" || string.IsNullOrWhiteSpace(currentUrl.LocalPath);
    }
}