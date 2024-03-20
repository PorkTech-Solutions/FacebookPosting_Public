using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenQA.Selenium;

namespace FacebookPosting;

[DataContract]
[KnownType(typeof(CustomizablePostSubmitMacross))]
public class CustomizablePostSubmitMacross : IPostSubmitMacross // DECORATOR PATTERN
{
    [JsonIgnore]
    private readonly string _postUrlResultsPath = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd-hh-mm") + ".txt";
    private readonly DefaultPostSubmitMacross _postSubmitMacross;
    [DataMember]
    private readonly SetGroupBlockMacross[] _setGroupBlocksMacrosses;

    public event Action<Uri> GetPostUri = delegate { };

    public bool ContainsSetGroupBlockMacrosses => _setGroupBlocksMacrosses != null && _setGroupBlocksMacrosses.Length != 0;
    public CustomizablePostSubmitMacross(DefaultPostSubmitMacross postSubmitMacross)
    {
        _postSubmitMacross = postSubmitMacross;
        _setGroupBlocksMacrosses = new SetGroupBlockMacross[0];
    }
    public CustomizablePostSubmitMacross(DefaultPostSubmitMacross postSubmitMacross, IEnumerable<SetGroupBlockMacross> setGroupBlockMacrosses)
    {
        _postSubmitMacross = postSubmitMacross;
        _setGroupBlocksMacrosses = setGroupBlockMacrosses.ToArray();
    }

    public CustomizablePostSubmitMacross(DefaultPostSubmitMacross postSubmitMacross, string setGroupBlockMacrossesInput)
    {
        _postSubmitMacross = postSubmitMacross;
        _setGroupBlocksMacrosses = SetGroupBlockMacrossParser.ParseMacrosses(setGroupBlockMacrossesInput);
    }
    public async Task Execute(WebDriver driver, FacebookPostData postData)
    {
        #if DEBUG
        try
        {
        #endif

        var isGroupAvailable = _postSubmitMacross.CheckIfGroupIsAvailable(driver);
        if(isGroupAvailable == false)
        {
            Console.WriteLine("{0}: IS NOT AVAILABLE", driver.Url);
            return;
        }
        await Task.Delay(6000);
        var joinGroupButtonAvaible = _postSubmitMacross.TryGetJoinGroupButton(driver, out IWebElement? joinButton);

        if(joinGroupButtonAvaible)
        {
            joinButton!.Click();
            await Task.Delay(5000);

            if(_setGroupBlocksMacrosses.Length != 0)
            {
                //var joinFormDialog = JoinFormDialog.GetJoinFormDialog(driver, _setGroupBlocksMacrosses);
                var joinFormDialog = JoinFormDialog.GetRandomJoinFormDialog(driver); // TEST
                await Task.Delay(5000);

                if(_setGroupBlocksMacrosses.First() is not SetNoneBlockMacross)
                {
                    await joinFormDialog.InvokeSetBlockMacrossesAsync(TimeSpan.FromSeconds(2));
                }
                
                joinFormDialog.CheckAgreeToTermsField();
                await Task.Delay(8000);

                joinFormDialog.SubmitForm();
                await Task.Delay(3000);
            }
        }

        await Task.Delay(5000);
        //return;
            await _postSubmitMacross.Execute(driver, postData);
        

        Uri postUri = _postSubmitMacross.GetLastArticleUrl(driver)!;
        
        if(_postSubmitMacross.GetCurrentUserPictureUrl(driver) != _postSubmitMacross.GetLastArticleUserPictureUrl(driver))
        {
            return;
        }
        
        GetPostUri?.Invoke(postUri);

        File.AppendAllText(_postUrlResultsPath, postUri.ToString() + "\n");


        #if DEBUG
        }
        catch (Exception e) { System.Console.WriteLine("EXCEPTION: " + e.Message + "\n\n" + e.StackTrace); }
        #endif
    }

    public string? GetSerializedSetBlockMacrosses()
    {
        if(ContainsSetGroupBlockMacrosses == false)
        {
            return null;
        }

        return SetGroupBlockMacrossParser.SerializeMacrosses(_setGroupBlocksMacrosses);
    }
}
