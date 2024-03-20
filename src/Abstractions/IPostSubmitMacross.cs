using System.Runtime.Serialization;
using OpenQA.Selenium;

namespace FacebookPosting.Abstractions;

public interface IPostSubmitMacross
{
    Task Execute(WebDriver driver, FacebookPostData postData);
}