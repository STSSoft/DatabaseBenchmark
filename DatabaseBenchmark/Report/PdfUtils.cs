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

            PdfPCell title = new PdfPCell(new PdfPTable(1));
            title.VerticalAlignment = Element.ALIGN_MIDDLE;
            title.HorizontalAlignment = Element.ALIGN_CENTER;
            title.MinimumHeight = doc.PageSize.Height - (doc.BottomMargin + doc.TopMargin);

            Paragraph paragraph = new Paragraph("DATABASE BENCHAMRK RESULTS");
            paragraph.Font = new Font(Font.NORMAL, 16f, Font.BOLD, Color.BLUE);
            paragraph.Alignment = Element.ALIGN_MIDDLE;

            title.AddElement(paragraph);
            doc.Add(title);

            int chapterCount = 1;

            foreach (var fr in frames)
            {
                StepFrame frame = fr.Value;
                List<BarChart> barCharts = frame.GetSelectedBarCharts();

                PdfPTable table = new PdfPTable(barCharts.Count);
                table.WidthPercentage = 100;

                var chapter = new iTextSharp.text.Chapter(fr.Key == TestMethod.SecondaryRead.ToString() ? "Secondary Read" : fr.Key, chapterCount++);

                chapter.Add(new Chunk("\n"));

                for (int i = 0; i < barCharts.Count; i++)
                    AddCellToTable(table, barCharts[i]);

                chapter.Add(table);

                chapter.Add(new Paragraph("Average Speed"));
                AddLineChartToDocument(chapter, frame.lineChartAverageSpeed);

                chapter.Add(new Paragraph("Moment Speed"));
                AddLineChartToDocument(chapter, frame.lineChartMomentSpeed);

                chapter.Add(new Paragraph("Average Memory"));
                AddLineChartToDocument(chapter, frame.lineChartAverageMemory);
                
                doc.Add(chapter);
            }

            doc.Close();
        }

        private static void AddCellToTable(PdfPTable table,BarChart frame)
        {
            Image image = Image.GetInstance(frame.ConvertToByteArray());
            PdfPCell cell = new PdfPCell();

            cell.AddElement(image);

            table.AddCell(cell);
        }

        private static void AddLineChartToDocument(Chapter chapter, LineChart frame)
        {
            Image image = Image.GetInstance(frame.ConvertToByteArray());
            //image.ScalePercent(50f);
            image.WidthPercentage = 40f;

            chapter.Add(image);
        }
    }
}
