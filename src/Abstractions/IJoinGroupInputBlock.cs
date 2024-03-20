using System.Runtime.Serialization;
using OpenQA.Selenium;

namespace FacebookPosting.Abstractions;

public interface IJoinGroupInputBlock
{
    [IgnoreDataMember]
    IWebDriver Driver { get; }
    [IgnoreDataMember]
    IWebElement WebElement { get; }

    // TODO: ???

    void Set(ISetGroupBlockMacross macross);
}