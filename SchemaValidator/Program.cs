using System;
using System.Xml;
using System.Xml.Schema;

namespace SchemaValidator
{
    internal static class Program
    {
        private static ConsoleColor _originalBackground;
        private static ConsoleColor _originalForeground;

        private static void Main(string[] args)
        {
            _originalBackground = Console.BackgroundColor;
            _originalForeground = Console.ForegroundColor;
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
            var schemaFilename = Console.ReadLine();
            schemaFilename = "" +  (string.IsNullOrWhiteSpace(schemaFilename) ? xmlFilename : schemaFilename);
            Console.WriteLine("Namespace");
            var namespaceName = Console.ReadLine();

            var sc = new XmlSchemaSet();
            sc.Add(string.IsNullOrWhiteSpace(namespaceName) ? null : namespaceName, schemaFilename.EndsWith(".xsd") ? schemaFilename : schemaFilename + ".xsd");

            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema, Schemas = sc, XmlResolver = new XmlUrlResolver()
            };
            settings.ValidationEventHandler += OnValidationEvent;
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
                Console.ForegroundColor = _originalForeground;
                Console.BackgroundColor = _originalBackground;
                return;
            }

            Console.WriteLine(reader.BaseURI);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            while (reader.Read())
            {
            }

            Console.ForegroundColor = _originalForeground;
            Console.BackgroundColor = _originalBackground;
        }

        private static void OnValidationEvent(object sender, ValidationEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}