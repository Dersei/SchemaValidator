using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace SchemaValidator
{
    class Program
    {
        private static ConsoleColor originalBackground;
        private static ConsoleColor originalForeground;
        static void Main(string[] args)
        {
            originalBackground = Console.BackgroundColor;
            originalForeground = Console.ForegroundColor;
            string xmlFilename;
            if (args.Length != 0)
            {
                xmlFilename = args[0];
            }
            else
            {
                Console.WriteLine("Xml file's name:");
                xmlFilename = Console.ReadLine();
            }
            Console.WriteLine("Schema file's name:");
            string schemaFilename = Console.ReadLine();
            schemaFilename = string.IsNullOrWhiteSpace(schemaFilename) ? xmlFilename : schemaFilename;
            Console.WriteLine("Namespace");
            string namespaceName = Console.ReadLine();

            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add(string.IsNullOrWhiteSpace(namespaceName) ? null : namespaceName, schemaFilename.EndsWith(".xsd") ? schemaFilename : schemaFilename + ".xsd");

            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = sc };
            settings.XmlResolver = new XmlUrlResolver();
            settings.ValidationEventHandler += new ValidationEventHandler(OnValidationEvent);
            XmlReader reader;
            try
            {
                reader = XmlReader.Create(xmlFilename.EndsWith(".xml") ? xmlFilename : xmlFilename + ".xml", settings);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = originalForeground;
                Console.BackgroundColor = originalBackground;
                return;
            }

            Console.WriteLine(reader.BaseURI);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            while (reader.Read()) ;
            Console.ForegroundColor = originalForeground;
            Console.BackgroundColor = originalBackground;
        }

        private static void OnValidationEvent(object sender, ValidationEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}