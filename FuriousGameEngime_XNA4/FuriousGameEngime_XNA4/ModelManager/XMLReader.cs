using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
using System.IO;

namespace FuriousGameEngime_XNA4.ModelManager
{
    class XMLReaderWriter
    {
        static Dictionary<string, PhysicsProperties> _properties = new Dictionary<string, PhysicsProperties>();

        internal static void AddPhysicsObjectProperties(ContentManager manager, string xmlFileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PhysicsProperties));

            FileStream fileStream;
            
            fileStream = new FileStream("Content//XMLFiles//" + xmlFileName + ".xml", FileMode.Open);
            _properties.Add(xmlFileName, (PhysicsProperties)xmlSerializer.Deserialize(fileStream));
            _properties[xmlFileName].Initialize(System.IO.Path.GetFileNameWithoutExtension(xmlFileName));
            fileStream.Close();
        }

        //not sure how this will work. Probably have to update the PhysicsProperty of the object first
        internal static void WritePhysicsObjectProperties(ContentManager manager, string xmlFileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PhysicsProperties));

            FileStream fileStream;

            fileStream = new FileStream("Content//" + xmlFileName + ".xml", FileMode.Create);
            xmlSerializer.Serialize(fileStream, PhysicsProperties(xmlFileName));
            fileStream.Close();
        }

        internal static PhysicsProperties PhysicsProperties(string key)
        {
            return _properties[key];
        }

        //internal static PhysicsProperties this[int index]
        //{
        //    get
        //    {
        //        return _properties[index];
        //    }
        //}

        //internal static PhysicsProperties this[string fileName]
        //{
        //    get
        //    {
        //        for (int i = 0; i < _properties.Count; ++i)
        //        {
        //            if (_properties[i].FileName == fileName)
        //            {
        //                return _properties[i];
        //            }
        //        }
        //        throw new Exception("No such file as " + fileName);
        //    }
        //}

        /// <summary>
        /// gets the number of physics property blueprints in the game
        /// </summary>
        //internal int Count
        //{
        //    get
        //    {
        //        return _properties.Count;
        //    }
        //}

        //PhysicsProperties[] properties;

        //internal XMLReader(ContentManager content)
        //{
        //    string[] xmlFiles = Directory.GetFiles(content.RootDirectory + "//XMLFiles");

        //    properties = new PhysicsProperties[xmlFiles.Length];

        //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(PhysicsProperties));

        //    FileStream fileStream;

        //    for (int i = 0; i < xmlFiles.Length; ++i)
        //    {
        //        fileStream = new FileStream(xmlFiles[i], FileMode.Create);
        //        xmlSerializer.Serialize(fileStream, properties[i]);
        //        fileStream.Close();

        //        //fileStream = new FileStream(xmlFiles[i], FileMode.Open);
        //        //properties[i] = (PhysicsProperties)xmlSerializer.Deserialize(fileStream);
        //        //properties[i].Initialize(System.IO.Path.GetFileNameWithoutExtension(xmlFiles[i])));
        //        //fileStream.Close();
        //    }
        //}
    }
}
