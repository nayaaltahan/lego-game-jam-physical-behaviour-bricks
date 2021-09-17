using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace LEGOModelImporter
{
    public class PaletteGenerator
    {
        static void WriteDesignIDsToPalette(List<string> designIds, string outPath)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.CloseOutput = true;

            using (XmlWriter writer = XmlWriter.Create(outPath, settings))
            {
                writer.WriteStartElement("INVENTORY");

                foreach (var designId in designIds)
                {
                    writer.WriteStartElement("ITEM");

                    writer.WriteElementString("ITEMTYPE", "P");
                    writer.WriteElementString("ITEMID", designId);

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.Flush();

                Debug.Log("Finished generating palette: " + outPath);
            }
        }

        [MenuItem("LEGO Tools/Dev/Generate Palette")]
        static void GeneratePalette()
        {
            var folderPath = EditorUtility.SaveFolderPanel("Choose destination folder", "", "");
            if(folderPath.Length == 0)
            {
                return;
            }

            var newPartsZipArchive = ZipFile.OpenRead(LEGOModelImporter.PartUtility.newPartsPath);
            var entries = newPartsZipArchive.Entries.Where(x => x.FullName.StartsWith("CollisionBox_Connectivity_Info/") && x.FullName.Contains(".xml"));

            List<string> supportedDesignIds = new List<string>();
            List<string> partiallySupportedDesignIds = new List<string>();

            foreach (var entry in entries)
            {
                var archiveStream = entry.Open();
                var doc = new XmlDocument();
                Debug.Log(entry.FullName);
                doc.Load(archiveStream);

                var primitiveNode = doc.SelectSingleNode("LEGOPrimitive");
                var connectivityNode = primitiveNode.SelectSingleNode("Connectivity");
                if (connectivityNode != null)
                {
                    var planarFields = connectivityNode.SelectNodes("Custom2DField");
                    var axleFields = connectivityNode.SelectNodes("Axel"); // Typo in Connectivity description files
                    var fixedFields = connectivityNode.SelectNodes("Fixed");
                    var hingeFields = connectivityNode.SelectNodes("Hinge");
                    var sliderFields = connectivityNode.SelectNodes("Slider");
                    var ballJointFields = connectivityNode.SelectNodes("Ball");
                    var gearFields = connectivityNode.SelectNodes("Gear");

                    var supported = planarFields.Count != 0 || axleFields.Count != 0 || fixedFields.Count != 0;
                    var unsupported = hingeFields.Count != 0 || sliderFields.Count != 0 || ballJointFields.Count != 0 || gearFields.Count != 0;

                    var designId = entry.Name.Replace(".xml", "");

                    if (supported && unsupported)
                    {
                        partiallySupportedDesignIds.Add(designId);
                    }
                    else if (supported && !unsupported)
                    {
                        supportedDesignIds.Add(designId);
                    }
                }
            }
            WriteDesignIDsToPalette(supportedDesignIds, folderPath + "/SupportedPalette.xml");
            WriteDesignIDsToPalette(partiallySupportedDesignIds, folderPath + "/PartiallySupportedPalette.xml");
        }
    }
}
