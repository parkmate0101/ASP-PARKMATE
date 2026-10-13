using System.Text;

namespace ParkMate.Services
{
    public class SimplePdfService
    {
        public byte[] BuildReceiptPdf(IEnumerable<string> lines)
        {
            var safeLines = lines
                .Select(line => line ?? string.Empty)
                .ToList();

            var content = new StringBuilder();
            content.AppendLine("BT");
            content.AppendLine("/F1 20 Tf");
            content.AppendLine("50 760 Td");
            content.AppendLine("(ParkMate Parking Receipt) Tj");
            content.AppendLine("/F1 11 Tf");

            foreach (string line in safeLines)
            {
                content.AppendLine("0 -24 Td");
                content.Append('(');
                content.Append(Escape(line));
                content.AppendLine(") Tj");
            }

            content.AppendLine("ET");

            string stream = content.ToString();
            byte[] streamBytes = Encoding.ASCII.GetBytes(stream);

            var pdf = new StringBuilder();
            pdf.Append("%PDF-1.4\n");
            pdf.Append("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
            pdf.Append("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
            pdf.Append("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>\nendobj\n");
            pdf.Append("4 0 obj\n<< /Length ");
            pdf.Append(streamBytes.Length);
            pdf.Append(" >>\nstream\n");
            pdf.Append(stream);
            pdf.Append("endstream\nendobj\n");
            pdf.Append("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

            byte[] body = Encoding.ASCII.GetBytes(pdf.ToString());
            var offsets = new List<int> { 0 };

            // Rebuild with correct xref offsets.
            var output = new MemoryStream();
            WriteAscii(output, "%PDF-1.4\n");

            WriteObject(output, offsets, 1,
                "<< /Type /Catalog /Pages 2 0 R >>");

            WriteObject(output, offsets, 2,
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");

            WriteObject(output, offsets, 3,
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>");

            offsets.Add((int)output.Position);
            WriteAscii(output, $"4 0 obj\n<< /Length {streamBytes.Length} >>\nstream\n");
            output.Write(streamBytes, 0, streamBytes.Length);
            WriteAscii(output, "endstream\nendobj\n");

            WriteObject(output, offsets, 5,
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

            long xrefPosition = output.Position;
            WriteAscii(output, "xref\n0 6\n");
            WriteAscii(output, "0000000000 65535 f \n");

            for (int i = 1; i <= 5; i++)
            {
                WriteAscii(
                    output,
                    offsets[i].ToString("D10") + " 00000 n \n");
            }

            WriteAscii(output,
                "trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n" +
                xrefPosition +
                "\n%%EOF");

            return output.ToArray();
        }

        private static void WriteObject(
            Stream stream,
            List<int> offsets,
            int objectNumber,
            string body)
        {
            offsets.Add((int)stream.Position);
            WriteAscii(
                stream,
                $"{objectNumber} 0 obj\n{body}\nendobj\n");
        }

        private static void WriteAscii(Stream stream, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string Escape(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }
    }
}
