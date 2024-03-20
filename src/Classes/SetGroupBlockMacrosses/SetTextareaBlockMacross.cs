using System.Runtime.Serialization;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace FacebookPosting.SetGroupBlockMacrosses;

[DataContract]
[KnownType(typeof(SetTextareaBlockMacross))]
public sealed class SetTextareaBlockMacross : SetGroupBlockMacross
{
    // "text" - text for first finded element with tag <textarea>
    public SetTextareaBlockMacross(string data) 
        : base(data)
    {}
    public override void Set(IJoinGroupInputBlock joinGroupInputBlock)
    {
        var textareaElement = joinGroupInputBlock.WebElement.FindElement(By.TagName("textarea"));

        var sendData = Data.Replace("\n", Keys.Enter);

        textareaElement.SendKeys(sendData);
    }
}