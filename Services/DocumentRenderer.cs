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
            foreach (var text in body.Descendants<Text>())
            {
                if (string.IsNullOrEmpty(text.Text)) continue;
                foreach (var kvp in tokens)
                {
                    var placeholder = "{{" + kvp.Key + "}}";
                    if (text.Text.Contains(placeholder))
                    {
                        text.Text = text.Text.Replace(placeholder, kvp.Value);
                    }
                }
            }
            wordDoc.MainDocumentPart.Document.Save();
        }

        return ms.ToArray();
    }
}
