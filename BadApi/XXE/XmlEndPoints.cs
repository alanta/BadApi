using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BadApi.XXE;

public static class Endpoints
{
    // Xml eXternal Entity (XXE) Injection
    
    public static async Task<IResult> Bad(HttpContext context)
    {
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        var xml = await reader.ReadToEndAsync();

        if (xml.Length <= 0)
        {
            return Results.BadRequest("No XML data was provided.");
        }

        var resolver = new XmlUrlResolver
        {
            Credentials = CredentialCache.DefaultCredentials
        };
        var xmlDoc = new XmlDocument
        {
            XmlResolver = resolver
        };

        try
        {
            xmlDoc.LoadXml(xml);
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Invalid XML data provided: " + ex.Message);
        }

        foreach (XmlNode xn in xmlDoc)
        {
            if (xn.Name == "user") return Results.Text("The user is: " + xn.InnerText);
        }

        return Results.BadRequest("No user data found in XML.");
    }

    public static async Task<IResult> Good(HttpContext context)
    {
        var serializer = new XmlSerializer(typeof(XmlModel));
        XmlModel? model;

        try
        {
            model = (XmlModel?)serializer.Deserialize(context.Request.Body);
        }
        catch (Exception)
        {
            return Results.BadRequest("Invalid XML data provided.");
        }

        if (model == null)
        {
            return Results.BadRequest("No XML data was provided.");
        }

        return Results.Text(model.User);
    }
}
