using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;

namespace SchemaValidatorWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ToolTip _toolTip;
        private readonly Dictionary<string, string> _errorTips = new Dictionary<string, string>();
        private readonly Dictionary<string, TextRange> _errorDescription = new Dictionary<string, TextRange>();
        public MainWindow()
        {
            InitializeComponent();
            _toolTip = new ToolTip
            {
                Content = "",
                Placement = PlacementMode.Relative,
                Visibility = Visibility.Hidden
            };
            XmlTextBox.ToolTip = _toolTip;
        }

        private void ValidateButton_OnClick(object sender, RoutedEventArgs e)
        {
            _errorTips.Clear();
            _errorDescription.Clear();
            ErrorTextBox.Document.Blocks.Clear();

            var xmlFile = new TextRange(XmlTextBox.Document.ContentStart, XmlTextBox.Document.ContentEnd).Text;
            var schemaFile = new TextRange(SchemaTextBox.Document.ContentStart, SchemaTextBox.Document.ContentEnd).Text;

            var sc = new XmlSchemaSet();
            sc.Add(null, XmlReader.Create(new StringReader(schemaFile)));

            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = sc,
                XmlResolver = new XmlUrlResolver()
            };
            settings.ValidationEventHandler += OnValidationEvent;
            XmlReader reader;
            try
            {
                reader = XmlReader.Create(new StringReader(xmlFile), settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine(reader.BaseURI);
            while (reader.Read())
            {
            }
        }

        private void OnValidationEvent(object sender, ValidationEventArgs e)
        {
            Console.WriteLine(e.Message);

            _errorTips.Add(ColorFragment(e.Exception).Text.Trim(), e.Exception.Message);
            WriteErrors(e.Exception);
        }

        private void WriteErrors(XmlSchemaException ex)
        {
            var range = ErrorTextBox.AddText($@"Line: {ex.LineNumber}{Environment.NewLine}    {ex.Message}");
            ErrorTextBox.AddText(Environment.NewLine);
            _errorDescription.Add(ex.Message, range);

        }

        private TextRange ColorFragment(XmlSchemaException ex)
        {
            var index = ex.LineNumber;
            XmlTextBox.CaretPosition = XmlTextBox.Document.ContentStart;
            var range = new TextRange(XmlTextBox.CaretPosition.GetLineStartPosition(index - 1 < 0 ? 0 : index - 1),
                XmlTextBox.CaretPosition.GetLineStartPosition(index));
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
            return range;
        }

        private TextRange BoldFragment(TextRange range, bool bold)
        {
            range.ApplyPropertyValue(TextElement.FontWeightProperty, bold ? FontWeights.Bold : FontWeights.Normal);
            return range;
        }



        private void XmlTextBox_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_toolTip.Visibility == Visibility.Visible)
            {
                _toolTip.VerticalOffset = e.GetPosition(XmlTextBox).Y;
                _toolTip.HorizontalOffset = e.GetPosition(XmlTextBox).X;
            }
            _toolTip.Visibility = Visibility.Hidden;
            _toolTip.Content = "";
            foreach (var item in _errorDescription.Values)
            {
                BoldFragment(item, false);
            }

            var range = XmlTextBox.GetLineUnderCursor(e);
            if (range?.GetPropertyValue(TextElement.ForegroundProperty).ToString() == "#FFFF0000")
            {
                _toolTip.Visibility = Visibility.Visible;
                if (_errorTips.TryGetValue(range.Text.Trim(), out var text))
                {
                    _toolTip.Content = text.Split('.').First();
                }

                if (_errorDescription.TryGetValue(text ?? "", out var errorRange))
                {
                    BoldFragment(errorRange, true);
                }
            }


        }
    }
}
