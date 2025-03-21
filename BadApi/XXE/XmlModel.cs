namespace BadApi.XXE;

public class XmlModel
{
    [System.Xml.Serialization.XmlElement("user")]
    public string User { get; set; }
}