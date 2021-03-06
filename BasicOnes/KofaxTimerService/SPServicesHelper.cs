﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace KofaxTimerService
{
    class SPServicesHelper
    {
        
        public void QueryList() {
            XNamespace s = "http://schemas.microsoft.com/sharepoint/soap/";
            XNamespace rs = "urn:schemas-microsoft-com:rowset";
            XNamespace z = "#RowsetSchema";
            
            // Make sure that you use the correct namespace, as well as the correct reference
            // name.  The namespace (by default) is the same as the name of the application
            // when you created it.  You specify the reference name in the Add Web Reference
            // dialog box.
            //
            // Namespace            Reference Name       Namespace              Reference Name
            //    |                      |                   |                        |
            //    V                      V                   V                        V
            CLUK.SPServices.ISDevListService.Lists SPListService = new CLUK.SPServices.ISDevListService.Lists();

            // Make sure that you update the following URL to point to the Lists web service
            // for your SharePoint site.
            SPListService.Url = "http://clint/it/isdev/_vti_bin/Lists.asmx";
            //lists.Url = "http://xyzteamsite/_vti_bin/Lists.asmx";

            SPListService.Credentials = System.Net.CredentialCache.DefaultCredentials;

            XElement queryOptions = new XElement("QueryOptions",
                new XElement("Folder"),
                new XElement("IncludeMandatoryColumns", false)
            );

            XElement viewFields = new XElement("ViewFields");
            XElement listCollection = SPListService.GetListCollection().GetXElement();

            XmlDocument xmlDoc = new System.Xml.XmlDocument();

            XmlNode ndQuery = xmlDoc.CreateNode(XmlNodeType.Element, "Query", "");
            XmlNode ndViewFields =
                xmlDoc.CreateNode(XmlNodeType.Element, "ViewFields", "");
            XmlNode ndQueryOptions =
                xmlDoc.CreateNode(XmlNodeType.Element, "QueryOptions", "");

            ndQueryOptions.InnerXml = "<IncludeMandatoryColumns>FALSE</IncludeMandatoryColumns>" + "<DateInUtc>TRUE</DateInUtc>";
            ndViewFields.InnerXml = "";//<FieldRef Name='Field1' /><FieldRef Name='Field2'/>
            ndQuery.InnerXml = "";
            //"<Where><And><Gt><FieldRef Name='Field1'/>" +  "<Value Type='Number'>5000</Value></Gt><Gt><FieldRef Name='Field2'/>" + "<Value Type='DateTime'>2003-07-03T00:00:00</Value></Gt></And></Where>";

            XmlNode ndListItems = SPListService.GetListItems("LARApprovedUsers", null, ndQuery, ndViewFields, null, ndQueryOptions, null);

            var counter = 0;
            foreach (System.Xml.XmlNode node in ndListItems)
            {
                if (node.Name == "rs:data")
                {
                    for (int i = 0; i < node.ChildNodes.Count; i++)
                    {
                        if (node.ChildNodes[i].Name == "z:row")
                        {
                            var titleAttrib = node.ChildNodes[i].Attributes["ows_Title"];
                            if (titleAttrib != null)
                                WriteLog(titleAttrib.Value + "</br>");

                            counter++;
                        }
                    }
                }
            }

            WriteLog(string.Format("List Item Count: {0} ", counter));

            XElement report = new XElement("Report",
                listCollection
                    .Elements(s + "List")
                    .Select(
                        l =>
                        {
                            return new XElement("List",
                                l.Attribute("Title"),
                                l.Attribute("DefaultViewUrl"),
                                l.Attribute("Description"),
                                l.Attribute("DocTemplateUrl"),
                                l.Attribute("BaseType"),
                                l.Attribute("ItemCount"),
                                l.Attribute("ID"),
                                SPListService.GetListItems((string)l.Attribute("ID"), "", null,
                                    viewFields.GetXmlNode(), "", queryOptions.GetXmlNode(), "")
                                    .GetXElement()
                                    .Descendants(z + "row")
                                    .Select(r =>
                                        new XElement("Row",
                                            r.Attribute("ows_Title"),
                                            r.Attribute("ows_ContentType"),
                                            r.Attribute("ows_FSObjType"),
                                            r.Attribute("ows_Attachments"),
                                            r.Attribute("ows_FirstName"),
                                            r.Attribute("ows_LinkFilename"),
                                            r.Attribute("ows_EncodedAbsUrl"),
                                            r.Attribute("ows_BaseName"),
                                            r.Attribute("ows_FileLeafRef"),
                                            r.Attribute("ows_FileRef"),
                                            r.Attribute("ows_ID"),
                                            r.Attribute("ows_UniqueId"),
                                            r.Attribute("ows_GUID")
                                        )
                                    )
                            );
                        }
                    )
            );
            //WriteLog(report.ToStringAlignAttributes());
        
        } 
        
    private void WriteLog(string message) 
    {
        string timeStamp = DateTime.Now.ToString("yyyyMMdd.hhmmss");
        var path = Path.Combine(string.Format("C:\\ServiceRoot\\KofaxTimerService\\ListService.{0}.log", timeStamp));

        System.IO.File.AppendAllText(path, message + "\r\n");
    }


    }
    

    public static class MyExtensions
    {
        public static XElement GetXElement(this XmlNode node)
        {
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
                node.WriteTo(xmlWriter);
            return xDoc.Root;
        }

        public static XmlNode GetXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }

        public static string ToStringAlignAttributes(this XElement element)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;
            StringBuilder stringBuilder = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))
                element.WriteTo(xmlWriter);
            return stringBuilder.ToString();
        }
    }

   
}
