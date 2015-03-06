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

            PdfPTable title = new PdfPTable(1);
            PdfPCell titleText = new PdfPCell();
            titleText.VerticalAlignment = Element.ALIGN_MIDDLE;
            titleText.HorizontalAlignment = Element.ALIGN_MIDDLE;
            titleText.MinimumHeight = doc.PageSize.Height - (doc.BottomMargin + doc.TopMargin);

            Paragraph paragraph = new Paragraph("DATABASE BENCHAMRK RESULTS");
            paragraph.Font = new Font(Font.NORMAL, 16f, Font.BOLD, Color.BLUE);
            paragraph.Alignment = Element.ALIGN_MIDDLE;

            titleText.AddElement(paragraph);
            title.AddCell(titleText);
            doc.Add(title);

            int chapterCount = 1;

            foreach (var fr in frames)
            {
                StepFrame frame = fr.Value;
                List<BarChart> barCharts = frame.GetSelectedBarCharts();

                int cellPerRow = 3;
                int cellCount = barCharts.Count > cellPerRow ? cellPerRow : barCharts.Count;

                PdfPTable table = new PdfPTable(cellCount);
                table.WidthPercentage = 100;

                var chapter = new iTextSharp.text.Chapter(fr.Key == TestMethod.SecondaryRead.ToString() ? "Secondary Read" : fr.Key, chapterCount++);

                chapter.Add(new Chunk("\n"));

                for (int i = 0; i < cellCount; i++)
                    AddCellToTable(table, i, barCharts[i]);

                chapter.Add(table);

                if (barCharts.Count > cellPerRow)
                {
                    table = new PdfPTable(barCharts.Count - cellCount);
                    table.WidthPercentage = 100;

                    for (int i = cellCount, index = 0; i < barCharts.Count; i++, index++)
                        AddCellToTable(table, index, barCharts[i]);

                    chapter.Add(table);
                }

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

<<<<<<< HEAD
        private static void AddCellToTable(PdfPTable table, int cellIndex, BarChartFrame frame)
=======
        private static void AddCellToTable(PdfPTable table, int cellIndex, int rowIndex, BarChart frame)
>>>>>>> origin/master
        {
            Image image = Image.GetInstance(frame.ConvertToByteArray());
            PdfPCell cell = new PdfPCell();

            cell.Colspan = cellIndex;
            cell.AddElement(image);

            table.AddCell(cell);
        }

<<<<<<< HEAD
        private static void AddLineChartToDocument(Chapter chapter, LineChartFrame frame)
=======
        private static void AddLineChartToDocument(Document doc, LineChart frame)
>>>>>>> origin/master
        {
            Image image = Image.GetInstance(frame.ConvertToByteArray());
            image.ScalePercent(60f);

            chapter.Add(image);
        }
    }
}
