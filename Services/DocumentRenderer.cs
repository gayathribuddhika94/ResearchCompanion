using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ResearchCompanion.Services;

// Fills the Word template (Templates/Proposal_Template.docx) by replacing
// {{TokenName}} placeholders with generated section text. Keeping all
// formatting inside the template file — not in code — means a new
// institution's template can be added later without changing this class
// (Technical Design, Section 8).
public class DocumentRenderer
{
    public byte[] Render(string templatePath, Dictionary<string, string> tokens)
    {
        var templateBytes = File.ReadAllBytes(templatePath);
        using var ms = new MemoryStream();
        ms.Write(templateBytes, 0, templateBytes.Length);

        using (var wordDoc = WordprocessingDocument.Open(ms, true))
        {
            var body = wordDoc.MainDocumentPart!.Document.Body!;

            // ToList(): the multi-line replacement below inserts new sibling
            // runs into the tree, so we must snapshot the list of Text nodes
            // before mutating it rather than enumerate it live.
            foreach (var text in body.Descendants<Text>().ToList())
            {
                if (string.IsNullOrEmpty(text.Text)) continue;
                foreach (var kvp in tokens)
                {
                    var placeholder = "{{" + kvp.Key + "}}";
                    if (text.Text.Contains(placeholder))
                    {
                        ReplaceWithLineBreakSupport(text, placeholder, kvp.Value);
                    }
                }
            }
            wordDoc.MainDocumentPart.Document.Save();
        }

        return ms.ToArray();
    }

    // Word ignores a raw '\n' inside a run's text — it does not start a new
    // line. A multi-entry value (e.g. one IEEE reference per line in the
    // {{References}} token, see CitationFormatterService) would otherwise
    // render as one run-on paragraph. When the replacement text contains
    // newlines, this splits it across sibling runs joined by explicit
    // <w:br/> line breaks instead of a plain string replace.
    private static void ReplaceWithLineBreakSupport(Text text, string placeholder, string value)
    {
        var replaced = text.Text.Replace(placeholder, value);
        if (!replaced.Contains('\n'))
        {
            text.Text = replaced;
            return;
        }

        var lines = replaced.Split('\n');
        text.Text = lines[0];
        text.Space = SpaceProcessingModeValues.Preserve;

        var originalRun = text.Parent as Run;
        OpenXmlElement insertAfter = (OpenXmlElement?)originalRun ?? text;

        for (var i = 1; i < lines.Length; i++)
        {
            var newRun = originalRun != null ? (Run)originalRun.CloneNode(false) : new Run();
            if (originalRun?.RunProperties != null)
            {
                newRun.RunProperties = (RunProperties)originalRun.RunProperties.CloneNode(true);
            }
            newRun.AppendChild(new Break());
            newRun.AppendChild(new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve });

            insertAfter.InsertAfterSelf(newRun);
            insertAfter = newRun;
        }
    }
}
