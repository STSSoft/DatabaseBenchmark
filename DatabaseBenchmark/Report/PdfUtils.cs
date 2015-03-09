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
    public static class PdfUtils
    {
        public static void Export(string file, Dictionary<string, StepFrame> frames)
        {
            var doc = new iTextSharp.text.Document(PageSize.A4);

            if (File.Exists(file))
                File.Delete(file);

            var fileStream = new FileStream(file, FileMode.OpenOrCreate);
            iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fileStream);
            doc.Open();

            // Add header page.
            PdfPTable firstPageTable = new PdfPTable(1);
            PdfPCell title = new PdfPCell();
            title.VerticalAlignment = Element.ALIGN_MIDDLE;
            title.HorizontalAlignment = Element.ALIGN_CENTER;
            title.MinimumHeight = doc.PageSize.Height - (doc.BottomMargin + doc.TopMargin);

            Paragraph paragraph = new Paragraph("DATABASE BENCHAMRK RESULTS");
            paragraph.Font = new Font(Font.NORMAL, 16f, Font.BOLD, Color.BLUE);
            paragraph.Alignment = Element.ALIGN_MIDDLE;

            title.AddElement(paragraph);
            firstPageTable.AddCell(title);

            doc.Add(firstPageTable);

            int chapterCount = 1;

            foreach (var fr in frames)
            {
                StepFrame frame = fr.Value;
                List<BarChart> barCharts = frame.GetSelectedBarCharts();

                PdfPTable table = new PdfPTable(barCharts.Count);
                table.WidthPercentage = 100;

                Chapter chapter = new Chapter(fr.Key == TestMethod.SecondaryRead.ToString() ? "Secondary Read" : fr.Key, chapterCount++);

                chapter.Add(new Chunk("\n"));

                for (int i = 0; i < barCharts.Count; i++)
                    AddCellToTable(table, string.Empty, barCharts[i].ConvertToByteArray);

                chapter.Add(table);

                table = new PdfPTable(1);
                table.WidthPercentage = 100;

                AddCellToTable(table, "Average Speed:", frame.lineChartAverageSpeed.ConvertToByteArray);
                AddCellToTable(table, "Moment Speed:", frame.lineChartMomentSpeed.ConvertToByteArray);
                AddCellToTable(table, "Average Memory:", frame.lineChartAverageMemory.ConvertToByteArray);
                AddCellToTable(table, "Average CPU:", frame.lineChartAverageCPU.ConvertToByteArray);
                AddCellToTable(table, "Average I/O:", frame.lineChartAverageIO.ConvertToByteArray);

                chapter.Add(table);
                doc.Add(chapter);
            }

            doc.Close();
        }

        private static void AddCellToTable(PdfPTable table, string text, Func<byte[]> converter)
        {
            Image image = Image.GetInstance(converter());
            PdfPCell cell = new PdfPCell();

            cell.AddElement(new Paragraph(text));
            cell.AddElement(new Chunk("\n"));
            cell.AddElement(image);

            table.AddCell(cell);
        }
    }
}
