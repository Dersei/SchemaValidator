using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SchemaValidatorWPF
{
    public static class RichTextBoxHelper
    {
        public static string GetText(this RichTextBox @this)
        {
            return new TextRange(@this.Document.ContentStart, @this.Document.ContentEnd).Text;
        }

        public static TextRange ColorFragment(this RichTextBox @this, int lineNumber, Color color)
        {
            @this.CaretPosition = @this.Document.ContentStart;
            var range = new TextRange(@this.CaretPosition.GetLineStartPosition(lineNumber - 1 < 0 ? 0 : lineNumber - 1),
                @this.CaretPosition.GetLineStartPosition(lineNumber));
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            return range;
        }

        public static void ColorWhole(this RichTextBox @this, Color color)
        {
            new TextRange(XmlTextBox.Document.ContentStart, @this.Document.ContentEnd).ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
        }

        public static (int lineNumber, TextRange textRange) GetLineUnderCursor(this RichTextBox @this, MouseEventArgs e)
        {
            var position = @this.GetPositionFromPoint(e.GetPosition(@this), false);

            if (position is null) return (-1, null);
            try
            {
                position.GetLineStartPosition(-int.MaxValue, out var lineNumber);
                return (-lineNumber + 1, new TextRange(position.GetLineStartPosition(0), position.GetLineStartPosition(1)));
            }
            catch
            {
                return (-1, null);
            }
        }

        public static TextRange AddText(this RichTextBox @this, string text)
        {
            @this.Document.Blocks.Add(new Paragraph(new Run(text)));
            return new TextRange(@this.Document.Blocks.LastBlock.ContentStart, @this.Document.Blocks.LastBlock.ContentEnd);
        }

        public static TextRange ChangeWeight(this TextRange @this, FontWeight weight)
        {
            @this.ApplyPropertyValue(TextElement.FontWeightProperty, weight);
            return @this;
        }
    }
}
