using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Validation;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseBenchmark.Report
{
    public static class PdfUtils
    {
        public static void Export(string file, Dictionary<string, StepFrame> frames, int flowCount, long recordCount, float randomness, ComputerConfiguration computerInfo, ReportType type)
        {
            var doc = new Document(PageSize.A4);

            if (File.Exists(file))
                File.Delete(file);

            var fileStream = new FileStream(file, FileMode.OpenOrCreate);
            PdfWriter.GetInstance(doc, fileStream);
            doc.Open();

            // Add header page.
            PdfPTable firstPageTable = new PdfPTable(1);
            firstPageTable.WidthPercentage = 100;

            PdfPCell title = new PdfPCell();
            title.VerticalAlignment = Element.ALIGN_MIDDLE;
            title.HorizontalAlignment = Element.ALIGN_CENTER;
            title.MinimumHeight = doc.PageSize.Height - (doc.BottomMargin + doc.TopMargin);
            title.AddElement(Image.GetInstance((System.Drawing.Image)DatabaseBenchmark.Properties.Resources.logo_01, Color.WHITE));

            firstPageTable.AddCell(title);
            doc.Add(firstPageTable);

            int chapterCount = 1;
            Font chapterFont = new Font(Font.TIMES_ROMAN, 16f, Font.TIMES_ROMAN, new Color(System.Drawing.Color.CornflowerBlue));

            Chapter benchamrkConfiguration = new Chapter(new Paragraph("Benchmark parameters.", chapterFont), chapterCount++);
            benchamrkConfiguration.Add(new Chunk("\n"));

            ExportTestSettings(benchamrkConfiguration, chapterFont, flowCount, recordCount, randomness);
            ExportComputerSpecification(benchamrkConfiguration, chapterFont, computerInfo);

            doc.Add(benchamrkConfiguration);

            foreach (var fr in frames)
            {
                StepFrame frame = fr.Value;
                List<BarChart> barCharts;

                if (type == ReportType.Summary)
                    barCharts = frame.GetAllBarCharts().Where(x => x.Title == "Speed (rec/sec)" || x.Title == "Size (MB)").ToList();
                else
                    barCharts = frame.GetSelectedBarCharts();

                PdfPTable table = new PdfPTable(barCharts.Count);
                table.WidthPercentage = 100;

                string chapterTitle = fr.Key == TestMethod.SecondaryRead.ToString() ? "Secondary read" : fr.Key;
                Chapter chapter = new Chapter(new Paragraph(chapterTitle, chapterFont), chapterCount++);
                chapter.Add(new Chunk("\n"));

                for (int i = 0; i < barCharts.Count; i++)
                    AddCellToTable(table, string.Empty, barCharts[i].ConvertToByteArray);

                chapter.Add(table);

                table = new PdfPTable(1);
                table.WidthPercentage = 100;

                AddCellToTable(table, "Average Speed:", frame.lineChartAverageSpeed.ConvertToByteArray);
                AddCellToTable(table, "Moment Speed:", frame.lineChartMomentSpeed.ConvertToByteArray);

                if (type == ReportType.Detailed)
                {
                    AddCellToTable(table, "Average Memory:", frame.lineChartAverageMemory.ConvertToByteArray);
                    AddCellToTable(table, "Average CPU:", frame.lineChartAverageCPU.ConvertToByteArray);
                    AddCellToTable(table, "Average I/O:", frame.lineChartAverageIO.ConvertToByteArray);
                }

                chapter.Add(table);
                doc.Add(chapter);
            }

            doc.Close();
        }

        public static void ExportTestSettings(Chapter chapter, Font font, int flowCount, long recordCount, float randomness)
        {
            Section settings = chapter.AddSection(new Paragraph("Test settings.", font));

            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            settings.Add(new Paragraph(string.Format("\t \t Benchmark - {0}", version)));
            settings.Add(new Paragraph(string.Format("\t \t Export date - {0}", DateTime.Now)));
            settings.Add(new Chunk("\n"));

            Section testSettings = chapter.AddSection(new Paragraph("Settings.", font));
            testSettings.Add(new Paragraph(string.Format("\t \t Flow count - {0}", flowCount)));
            testSettings.Add(new Paragraph(string.Format("\t \t Record count - {0}", recordCount)));
            testSettings.Add(new Paragraph(string.Format("\t \t Randomness - {0}%", randomness * 100)));
            testSettings.Add(new Paragraph(string.Format("\t \t Key type - {0}", randomness == 0f ? KeysType.Sequential : KeysType.Random)));
            testSettings.Add(new Chunk("\n"));
        }

        public static void ExportComputerSpecification(Chapter chapter, Font font, ComputerConfiguration computerInfo)
        {
            Section sectionPC = chapter.AddSection(new Paragraph("Computer specification.", font));
            sectionPC.Add(new Chunk("\n"));

            Section osSection = sectionPC.AddSection(new Paragraph("Operating System.", font));
            string bits = computerInfo.OperatingSystem.Is64bit ? " 64bit" : "32bit";
            osSection.Add(new Paragraph(string.Format("\t \t {0} {1}", computerInfo.OperatingSystem.Name, bits)));

            sectionPC.Add(new Chunk("\n"));

            Section processor = sectionPC.AddSection(new Paragraph("Processors.", font));
            foreach (var pr in computerInfo.Processors)
            {
                processor.Add(new Paragraph(string.Format("\t \t Name: {0}", pr.Name)));
                processor.Add(new Paragraph(string.Format("\t \t Threads: {0}", pr.Threads)));
                processor.Add(new Paragraph(string.Format("\t \t Max clock speed: {0} MHz", pr.MaxClockSpeed)));
            }

            sectionPC.Add(new Chunk("\n"));

            Section memory = sectionPC.AddSection(new Paragraph("Memory modules.", font));
            PdfPTable table = new PdfPTable(3);

            table.AddCell(CreateHeaderPdfPCell("Type"));
            table.AddCell(CreateHeaderPdfPCell("Capacity (GB)"));
            table.AddCell(CreateHeaderPdfPCell("Speed (MHz)"));

            foreach (var mem in computerInfo.MemoryModules)
            {
                table.AddCell(new PdfPCell(new Phrase(mem.MemoryType.ToString())));
                table.AddCell(new PdfPCell(new Phrase(mem.Capacity.ToString())) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(mem.Speed.ToString())) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            memory.Add(new Chunk("\n"));
            memory.Add(table);

            sectionPC.Add(new Chunk("\n"));

            Section storage = sectionPC.AddSection(new Paragraph("Storages.", font));
            table = new PdfPTable(3);

            table.AddCell(CreateHeaderPdfPCell("Model"));
            table.AddCell(CreateHeaderPdfPCell("Size (GB)"));
            table.AddCell(CreateHeaderPdfPCell("Partitions"));

            foreach (var stor in computerInfo.StorageDevices)
            {
                table.AddCell(new PdfPCell(new Phrase(stor.Model)));
                table.AddCell(new PdfPCell(new Phrase(stor.Size.ToString())) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(string.Join(",", stor.DriveLetters.Select(x => x.Replace(":", ""))))));
            }

            storage.Add(new Chunk("\n"));
            storage.Add(table);
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

        private static PdfPCell CreateHeaderPdfPCell(string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text));
            cell.BackgroundColor = new iTextSharp.text.Color(204, 204, 204);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_CENTER;

            return cell;
        }
    }
}
