using System.Runtime.Serialization;
using OpenQA.Selenium;

namespace FacebookPosting;

public class JoinGroupInputBlock : IJoinGroupInputBlock
{
    [IgnoreDataMember]
    public IWebDriver Driver { get; }
    [IgnoreDataMember]
    public IWebElement WebElement { get; }
    public virtual void Set(ISetGroupBlockMacross macross)
    {
        macross.Set(this);
    }
    public JoinGroupInputBlock(IWebDriver driver, IWebElement element)
    {
        WebElement = element;
        Driver = driver;
    }
}