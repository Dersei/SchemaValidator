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
        private readonly Dictionary<(int lineNumber, string lineText), string> _errorTips = new Dictionary<(int, string), string>();
        private readonly Dictionary<(int lineNumber, string message), TextRange> _errorDescription = new Dictionary<(int, string), TextRange>();
        private readonly List<RichTextBox> _schemasBoxes = new List<RichTextBox>();
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
            _schemasBoxes.Add(SchemaTextBox);
            Style paragraphStyle = new Style { TargetType = typeof(Paragraph) };
            paragraphStyle.Setters.Add(new Setter
            {
                Property = Block.MarginProperty,
                Value = new Thickness(2)
            });
            Resources.Add(typeof(Paragraph), paragraphStyle);
        }

        private string GetNamespace(string schemaFile)
        {
            var firstIndex = schemaFile.IndexOf("targetNamespace", StringComparison.Ordinal) + "targetNamespace=\"".Length;
            var lastIndex = schemaFile.IndexOf("\"", firstIndex, StringComparison.Ordinal);
            var @namespace = schemaFile.Substring(firstIndex, lastIndex - firstIndex);
            return @namespace;
        }

        private void BackToBlack()
        {
            new TextRange(XmlTextBox.Document.ContentStart, XmlTextBox.Document.ContentEnd).ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
        }

        private void ValidateButton_OnClick(object sender, RoutedEventArgs e)
        {
            _errorTips.Clear();
            _errorDescription.Clear();
            ErrorTextBox.Document.Blocks.Clear();
            BackToBlack();

            var xmlFile = new TextRange(XmlTextBox.Document.ContentStart, XmlTextBox.Document.ContentEnd).Text;


            var sc = new XmlSchemaSet();

            foreach (var schemaBox in _schemasBoxes)
            {
                var schemaFile = new TextRange(schemaBox.Document.ContentStart, schemaBox.Document.ContentEnd).Text;
                sc.Add(GetNamespace(schemaFile), XmlReader.Create(new StringReader(schemaFile)));
            }



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
            catch (XmlSchemaValidationException ex)
            {
                Console.WriteLine("###Creating exception: " + ex.Message);
                WriteErrors(ex, true);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("###Creating exception: " + ex.Message);
                WriteErrors(ex.Message);
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
            var value = (e.Exception.LineNumber, ColorFragment(e.Exception).Text.Trim());
            if (!_errorTips.ContainsKey(value))
            {
                _errorTips.Add(value, e.Exception.Message);
            }
            WriteErrors(e.Exception);
        }

        private void WriteErrors(XmlSchemaException ex, bool schemaError = false)
        {
            var part = schemaError ? "Schema line" : "Line";
            var range = ErrorTextBox.AddText($@"{part}: {ex.LineNumber}{Environment.NewLine}    {ex.Message}");
            ErrorTextBox.AddText(Environment.NewLine);
            if (schemaError)
            {
                range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
                return;
            }
            _errorDescription.Add((ex.LineNumber, ex.Message), range);
        }

        private void WriteErrors(string message)
        {
            var range = ErrorTextBox.AddText($@"Exception: {Environment.NewLine}    {message}");
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
            ErrorTextBox.AddText(Environment.NewLine);
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

            var (lineNumber, range) = XmlTextBox.GetLineUnderCursor(e);

            if (range?.GetPropertyValue(TextElement.ForegroundProperty).ToString() == "#FFFF0000")
            {
                _toolTip.Visibility = Visibility.Visible;
                if (_errorTips.TryGetValue((lineNumber, range.Text.Trim()), out var text))
                {
                    _toolTip.Content = text.Split('.').First();
                }

                if (_errorDescription.TryGetValue((lineNumber, text ?? ""), out var errorRange))
                {
                    BoldFragment(errorRange, true);
                }
            }


        }



        private void AddSchemaButton_Click(object sender, RoutedEventArgs e)
        {
            var schemaBox = new RichTextBox()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            SchemaGrid.RowDefinitions.Add(new RowDefinition());


            Grid.SetRow(schemaBox, SchemaGrid.RowDefinitions.Count - 1);
            SchemaGrid.Children.Add(schemaBox);
            _schemasBoxes.Add(schemaBox);
        }
    }
}
