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
    public partial class MainWindow
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
            XmlTextBox.Document.PageWidth = 1000; //To disable text wrapping in most situations
            SchemaTextBox.Document.PageWidth = 1000;
            _schemasBoxes.Add(SchemaTextBox);
            var paragraphStyle = new Style { TargetType = typeof(Paragraph) };
            paragraphStyle.Setters.Add(new Setter //sets size of margins
            {
                Property = Block.MarginProperty,
                Value = new Thickness(2)
            });
            Resources.Add(typeof(Paragraph), paragraphStyle);
        }

        /// <summary>
        /// Reads target namespace declared in schema text
        /// </summary>
        /// <param name="schemaFile"></param>
        /// <returns></returns>
        private static string GetNamespace(string schemaFile)
        {
            var firstIndex = schemaFile.IndexOf("targetNamespace", StringComparison.Ordinal) + "targetNamespace=\"".Length;
            var lastIndex = schemaFile.IndexOf("\"", firstIndex, StringComparison.Ordinal);
            var @namespace = schemaFile.Substring(firstIndex, lastIndex - firstIndex);
            return @namespace;
        }

        private void ValidateButton_OnClick(object sender, RoutedEventArgs e)
        {
            _errorTips.Clear();
            _errorDescription.Clear();
            ErrorTextBox.Document.Blocks.Clear();
            XmlTextBox.ColorWhole(Colors.Black);
            _isValid = true;

            var xmlFile = XmlTextBox.GetText();

            var sc = new XmlSchemaSet();

            foreach (var schemaBox in _schemasBoxes) //reads schema from each box and adds it to set
            {
                var schemaFile = schemaBox.GetText();
                try //Necessary in case of errors in schema
                {
                    if (!string.IsNullOrWhiteSpace(schemaFile))
                        sc.Add(GetNamespace(schemaFile), XmlReader.Create(new StringReader(schemaFile)));
                }
                catch (Exception ex)
                {

                    switch (ex)
                    {
                        case XmlSchemaException exs:
                            WriteErrors(exs, true);
                            return;
                        case XmlException exx:
                            WriteErrors(new XmlSchemaException(exx.Message, exx, exx.LineNumber, exx.LinePosition), true);
                            return;
                        default:
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                    }
                }
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
            catch (XmlSchemaException ex) //Sometimes happens
            {
                WriteErrors(ex, true);
                return;
            }
            catch (Exception ex)
            {
                WriteErrors(ex.Message);
                return;
            }

            try
            {
                while (reader.Read())
                {
                }
            }
            catch (XmlException ex)
            {
                WriteErrors(ex);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_isValid)
            {
                WriteSuccess();
            }
        }

        private bool _isValid = true;

        private void OnValidationEvent(object sender, ValidationEventArgs e)
        {
            _isValid = false;

            var value = (e.Exception.LineNumber, XmlTextBox.ColorFragment(e.Exception.LineNumber, Colors.Red).Text.Trim());
            if (!_errorTips.ContainsKey(value))
            {
                _errorTips.Add(value, e.Exception.Message);
            }
            WriteErrors(e.Exception);
        }

        private void WriteSuccess()
        {
            var range = ErrorTextBox.AddText("Success");
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Green));
            range.ApplyPropertyValue(TextElement.FontSizeProperty, 36.0);
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

        private void WriteErrors(XmlException ex)
        {
            var range = ErrorTextBox.AddText($@"XML Error{Environment.NewLine}Line: {ex.LineNumber}{Environment.NewLine}    {ex.Message}");
            ErrorTextBox.AddText(Environment.NewLine);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.OrangeRed));
            _errorDescription.Add((ex.LineNumber, ex.Message), range);
        }

        private void WriteErrors(string message)
        {
            var range = ErrorTextBox.AddText($@"Exception: {Environment.NewLine}    {message}");
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
            ErrorTextBox.AddText(Environment.NewLine);
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
                item.ChangeWeight(FontWeights.Normal);
            }

            var (lineNumber, range) = XmlTextBox.GetLineUnderCursor(e);

            if (range?.GetPropertyValue(TextElement.ForegroundProperty).ToString() != "#FFFF0000") return;
            _toolTip.Visibility = Visibility.Visible;

            if (_errorTips.TryGetValue((lineNumber, range.Text.Trim()), out var text))
            {
                _toolTip.Content = text.Split('.').First();
            }

            if (_errorDescription.TryGetValue((lineNumber, text ?? ""), out var errorRange))
            {
                errorRange.ChangeWeight(FontWeights.Bold);
            }
        }

        private void AddSchemaButton_Click(object sender, RoutedEventArgs e)
        {
            var schemaBox = new RichTextBox
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Document = {PageWidth = 1000}
            };
            SchemaGrid.RowDefinitions.Add(new RowDefinition());

            Grid.SetRow(schemaBox, SchemaGrid.RowDefinitions.Count - 1);
            SchemaGrid.Children.Add(schemaBox);
            _schemasBoxes.Add(schemaBox);
        }
    }
}