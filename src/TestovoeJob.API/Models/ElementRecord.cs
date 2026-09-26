namespace TestovoeJob.API.Models;

public sealed class ElementRecord
{
    public ElementRecord(string attributeValue, string htmlCode)
    {
        AttributeValue = attributeValue;
        HtmlCode = htmlCode;
    }

    public string AttributeValue { get; }

    public string HtmlCode { get; }
}
