using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Frames;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseBenchmark.Report
{
    public class PdfUtils
    {
        private Dictionary<string, StepFrame> Frames;

        public PdfUtils(Dictionary<string, StepFrame> frames)
        {
            Frames = frames;
        }

        public void Export(string file)
        {
            var doc = new iTextSharp.text.Document(PageSize.A4);

            if (File.Exists(file))
                File.Delete(file);

            var fileStream = new FileStream(file, FileMode.OpenOrCreate);
            iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fileStream);
            doc.Open();

            doc.Add(new iTextSharp.text.Paragraph("Chart"));

            int chapterCount = 1;
            foreach (var fr in Frames)
            {
                StepFrame frame = fr.Value;
                var chapter = new iTextSharp.text.Chapter(fr.Key == TestMethod.SecondaryRead.ToString() ? "Secondary Read" : fr.Key, chapterCount++);

                PdfPTable table = new PdfPTable(3);
                table.HorizontalAlignment = Element.ALIGN_CENTER;

                AddCellToTable(table, 1, frame.barChartSpeed);
                AddCellToTable(table, 2, frame.barChartSize);
                AddCellToTable(table, 3, frame.barChartTime);

                chapter.Add(new Chunk('\n'));
                chapter.Add(table);

                doc.Add(chapter);

                doc.Add(new Paragraph("Average Speed"));
                AddLineChartToDocument(doc, frame.lineChartAverageSpeed);

                doc.Add(new Paragraph("Moment Speed"));
                AddLineChartToDocument(doc, frame.lineChartMomentSpeed);

                doc.Add(new Paragraph("Average Memory"));
                AddLineChartToDocument(doc, frame.lineChartAverageMemory);

                doc.NewPage();
            }

            doc.Close();
        }

        private void AddCellToTable(PdfPTable table, int cellIndex, BarChartFrame frame)
        {
            Image image = Image.GetInstance(frame.ConvertToByteArray());
            PdfPCell cell = new PdfPCell();

            cell.Colspan = cellIndex;
            cell.VerticalAlignment = Element.ALIGN_CENTER;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;

            table.AddCell(image);
        }

        private void AddLineChartToDocument(Document doc, LineChartFrame frame)
        {
            Image image = Image.GetInstance(frame.ConvertToByteArray());
            image.ScalePercent(60f);

            doc.Add(image);
        }
    }
}
