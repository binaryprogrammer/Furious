//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework.Content;
//using System.IO;
//using System.Xml.Serialization;

//namespace FuriousGameEngime_XNA4.GameEntities.Properties
//{
//    class PhysicsXMLReader
//    {
//        PhysicsProperties[] properties;

//        internal PhysicsXMLReader(ContentManager content)
//        {
//            string[] xmlFiles = Directory.GetFiles(content.RootDirectory + "//XMLFiles");

//            properties = new PhysicsProperties[xmlFiles.Length];

//            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PhysicsProperties));

//            FileStream fileStream;

//            for (int i = 0; i < xmlFiles.Length; ++i)
//            {
//                fileStream = new FileStream(xmlFiles[i], FileMode.Create);
//                xmlSerializer.Serialize(fileStream, properties[i]);
//                fileStream.Close();

//                //fileStream = new FileStream(xmlFiles[i], FileMode.Open);
//                //properties[i] = (PhysicsProperties)xmlSerializer.Deserialize(fileStream);
//                //properties[i].Initialize(System.IO.Path.GetFileNameWithoutExtension(xmlFiles[i])));
//                //fileStream.Close();
//            }
//        }

//        internal PhysicsProperties this[int index]
//        {
//            get
//            {
//                return properties[index];
//            }
//        }

//        internal PhysicsProperties this[string fileName]
//        {
//            get
//            {
//                for (int i = 0; i < this.Count; ++i)
//                {
//                    if (properties[i].FileName == fileName)
//                    {
//                        return properties[i];
//                    }
//                }
//                throw new Exception("No such file as " + fileName);
//            }
//        }

//        /// <summary>
//        /// gets the number of physics property blueprints in the game
//        /// </summary>
//        internal int Count
//        {
//            get
//            {
//                return properties.Length;
//            }
//        }
//    }
//}
