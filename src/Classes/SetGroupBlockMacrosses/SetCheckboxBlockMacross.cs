using System.Runtime.Serialization;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace FacebookPosting.SetGroupBlockMacrosses;

[DataContract]
[KnownType(typeof(SetTextareaBlockMacross))]
public sealed class SetCheckboxBlockMacross : SetGroupBlockMacross
{
    // \"name\" - check attribute by its name. DOESNT WORK EVERYTIME.
    // [0] - check attribute by its index
    // ALL - check all inputs, should be an only parameter 
    public SetCheckboxBlockMacross(string data) 
        : base(data)
    {}
    public override void Set(IJoinGroupInputBlock joinGroupInputBlock)
    {
        var allCheckboxes = joinGroupInputBlock.WebElement
            .FindElements(By.TagName("input"))
            .Where(input => input.GetAttribute("type") == "checkbox")
            .ToList();

        if(Data == "ALL")
        {
            foreach (var yieldCheckbox in allCheckboxes)
            {
                // TODO: scroll?

                yieldCheckbox.Click();
            }

            return;
        }

        var datas = Data.Split(',');
        for (int i = 0; i < datas.Length; i++)
        {
            datas[i] = datas[i].Trim();
        }

        foreach (var yieldData in datas)
        {
            var tmpYieldData = yieldData.Trim();
            if(tmpYieldData.StartsAndEndsWith('\"'))
            {
                var tmpData = tmpYieldData.Replace("\"", "");

                var neededCheckbox = allCheckboxes.First(webElement => webElement.GetAttribute("value") == tmpData);
                
                // TDOD: scroll?
                neededCheckbox.Click();
            }
            else if(tmpYieldData.StartsWith('[') && tmpYieldData.EndsWith(']'))
            {
                var tmpData = tmpYieldData.Replace("[", "").Replace("]", "");
                var tmpIndex = int.Parse(tmpData);

                allCheckboxes[tmpIndex].Click();
            }
        }
    }
}