using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace SchemaValidatorWPF
{
    public static class RichTextBoxHelper
    {
        public static string GetText(this RichTextBox @this)
        {
            return new TextRange(@this.Document.ContentStart, @this.Document.ContentEnd).Text;
        }

        public static TextRange GetLineUnderCursor(this RichTextBox @this, MouseEventArgs e)
        {
            var position = @this.GetPositionFromPoint(e.GetPosition(@this), false);
            if (position is null) return null;
            try
            {
                return new TextRange(position.GetLineStartPosition(0), position.GetLineStartPosition(1));
            }
            catch
            {
                return null;
            }
        }

        public static TextRange AddText(this RichTextBox @this, string text)
        {
            @this.Document.Blocks.Add(new Paragraph(new Run(text)));
            return new TextRange(@this.Document.Blocks.LastBlock.ContentStart, @this.Document.Blocks.LastBlock.ContentEnd);
        }
    }
}
