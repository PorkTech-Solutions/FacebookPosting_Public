using System.Runtime.Serialization;
using OpenQA.Selenium;

namespace FacebookPosting;

[DataContract]
[KnownType(typeof(JoinFormDialog))]
public class JoinFormDialog
{
    [IgnoreDataMember]
    protected readonly IWebDriver _driver;
    [DataMember]
    protected readonly SetGroupBlockMacross[] _joinGroupBlockMacrosses;
    [IgnoreDataMember]
    private readonly IWebElement _element;
    [DataMember]
    private JoinGroupInputBlock[] _joinGroupInputs;
    [DataMember]
    public bool AgreeToTermsRequired => _agreeToTermsElement is not null;
    [IgnoreDataMember]
    private IWebElement? _agreeToTermsElement;
    public static JoinFormDialog GetJoinFormDialog(IWebDriver driver, SetGroupBlockMacross[] setMacrosses)
    {
        System.Console.WriteLine("GetJoinFormDialog");
        var outWebElementsFinded = driver
            .FindElements(By.ClassName("x71s49j"));
        System.Console.WriteLine(outWebElementsFinded.Count);
        var outWebElements = outWebElementsFinded.Where(element => element.GetAttribute("role") == "dialog");
        
        Console.WriteLine("role == \"dialog\": count - " + outWebElements.Count());

        var outWebElement = outWebElements.First();

        var outBlocksElements = outWebElement
            .FindElements(By.TagName("div"))
            .Where(element => element.GetAttribute("data-visualcompletion") == "ignore-dynamic")
            .ToList();

        var joinGroupInputBlocks = new JoinGroupInputBlock[outBlocksElements.Count];
        int counter = 0;
        foreach (var outBlockElement in outBlocksElements)
        {
            joinGroupInputBlocks[counter++] = new JoinGroupInputBlock(driver, outBlockElement);
        }

        bool agreeToTermsRequired = TryGetAgreeToTermsField(outWebElement, out IWebElement? agreeToTermsInput);
        
        return new JoinFormDialog(driver, outWebElement, joinGroupInputBlocks, setMacrosses, agreeToTermsInput);
    }

    
    public static JoinFormDialog GetRandomJoinFormDialog(IWebDriver driver)
    {
        System.Console.WriteLine("GetJoinFormDialog");
        
        var outWebElement = driver
            .FindElement(By.CssSelector(".x71s49j[role=dialog]"));

        var outBlocksElements = outWebElement
            .FindElements(By.CssSelector("div[data-visualcompletion=ignore-dynamic]"))
            .ToList();

        var joinGroupInputBlocks = new JoinGroupInputBlock[outBlocksElements.Count];
        int counter = 0;
        SetGroupBlockMacross[] setMacrosses = new SetGroupBlockMacross[outBlocksElements.Count];
        foreach (var outBlockElement in outBlocksElements)
        {
            joinGroupInputBlocks[counter] = new JoinGroupInputBlock(driver, outBlockElement);

            var inputElements = outBlockElement.FindElements(By.CssSelector("input, textarea"));

            if(inputElements.First().TagName == "textarea")
            {
                setMacrosses[counter] = new SetTextareaBlockMacross("da");
            }
            else if(inputElements.First().GetAttribute("type").Equals("checkbox", StringComparison.OrdinalIgnoreCase))
            {
                setMacrosses[counter] = new SetCheckboxBlockMacross("[0]");
            }
            else if(inputElements.First().GetAttribute("type").Equals("radio", StringComparison.OrdinalIgnoreCase))
            {
                setMacrosses[counter] = new SetRadioBlockMacross("[0]");
            }

            counter++;
        }

        bool agreeToTermsRequired = TryGetAgreeToTermsField(outWebElement, out IWebElement? agreeToTermsInput);
        
        return new JoinFormDialog(driver, outWebElement, joinGroupInputBlocks, setMacrosses, agreeToTermsInput);
    }

    private static bool TryGetAgreeToTermsField(IWebElement formElement, out IWebElement? agreeToTermsInput)
    {
        var resultElements = formElement.FindElements(By.Name("agree-to-group-rules"));
        bool returnResult = resultElements.Count > 0;

        agreeToTermsInput = returnResult ? resultElements.First() : null;

        return returnResult;
    }

    public void InvokeSetBlockMacross(int index)
    {
        _joinGroupBlockMacrosses[index].Set(_joinGroupInputs[index]);
    }

    public async Task InvokeSetBlockMacrossesAsync(TimeSpan yieldDelay)
    {
        for (int i = 0; i < _joinGroupBlockMacrosses.Length; i++)
        {
            _joinGroupBlockMacrosses[i].Set(_joinGroupInputs[i]);

            await Task.Delay(yieldDelay);
        }
    }

    public bool CheckAgreeToTermsField()
    {
        if(AgreeToTermsRequired == false)
        {
            return false;
        }

        // TODO: scroll?
        _agreeToTermsElement!.Click();

        return true;
    }

    public void SubmitForm()
    {
        var targetElements = _element.FindElements(By.ClassName("x1i10hfl"))
            .Where(element => element.GetAttribute("role") == "button")
            .ToList();

        Console.WriteLine("joinform: submit: role == button: " + targetElements.Count);

        foreach (var item in targetElements)
        {
            Console.WriteLine(item.GetAttribute("aria-label"));
        }

        Console.WriteLine("\n" + targetElements[^2].GetAttribute("aria-label"));
        targetElements[^2].Click();
    }

    public static JoinFormDialog GetJoinFormDialog(WebDriver driver, string macrossesInput)
    {
        return GetJoinFormDialog(driver, SetGroupBlockMacrossParser.ParseMacrosses(macrossesInput));
    }

    private JoinFormDialog(IWebDriver driver, IWebElement element, JoinGroupInputBlock[] joinGroupInputs, SetGroupBlockMacross[] joinGroupBlockMacrosses, IWebElement? agreeToTermsElement = null)
    {
        _driver = driver;
        _element = element;
        _joinGroupInputs = joinGroupInputs;
        _joinGroupBlockMacrosses = joinGroupBlockMacrosses;
        _agreeToTermsElement = agreeToTermsElement;
    }
}