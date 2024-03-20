using System.ComponentModel;
using System.Runtime.Serialization;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace FacebookPosting.SetGroupBlockMacrosses;

[DataContract]
[KnownType(typeof(SetTextareaBlockMacross))]
public sealed class SetRadioBlockMacross : SetGroupBlockMacross
{
    // "[0]" - set by index
    // "Agree" - set by value(ordinalIgnoreCase)
    public SetRadioBlockMacross(string data) 
        : base(data)
    {}
    public override void Set(IJoinGroupInputBlock joinGroupInputBlock)
    {
        var radioWebElements = joinGroupInputBlock.WebElement.FindElements(By.TagName("input"));

        string trimData = Data.Trim();
        //var actions = new Actions();

        if(Data.StartsWith('[') && Data.EndsWith(']'))
        {
            int indexToSet = int.Parse(trimData.Replace("[", "").Replace("]", ""));

            radioWebElements[indexToSet].Click();
            
            return;
        }

        var neededRadioElement = radioWebElements.First(radioElement 
            => radioElement.GetAttribute("value").Equals(Data, StringComparison.OrdinalIgnoreCase));

        neededRadioElement.Click();

    }
}