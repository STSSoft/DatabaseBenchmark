using DatabaseBenchmark.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DatabaseBenchmark.Serialization
{
    /// <summary>
    /// Persists the state of the application (including: application settings, database settings, window layout).
    /// </summary>
    public class ProjectManager
    {
        private ILog Logger;
        private MainLayout MainLayout;

        public string DockConfigPath { get; private set; }

        public ProjectManager(MainLayout layout)
        {
            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);

            MainLayout = layout;
        }

        public void Store(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    //LayoutPersist layoutPersist = new LayoutPersist(MainLayout);
                    //DatabasePersist databasePersist = new DatabasePersist(MainLayout.TreeView.GetAllDatabasesAndCheckedStates());

                    //var settings = new XmlWriterSettings()
                    //{
                    //    Indent = true,
                    //    NewLineHandling = NewLineHandling.Entitize,
                    //    ConformanceLevel = ConformanceLevel.Fragment,
                    //    OmitXmlDeclaration = true
                    //};

                    //using (XmlWriter documentWriter = XmlWriter.Create(stream))
                    //{
                    //    documentWriter.WriteStartDocument();
                    //    documentWriter.WriteWhitespace(Environment.NewLine);

                    //    // <Project>
                    //    documentWriter.WriteStartElement("Project");
                    //    documentWriter.WriteWhitespace(Environment.NewLine);
                    //    documentWriter.Flush();

                    //    using (XmlWriter fragmentWriter = XmlWriter.Create(stream, settings))
                    //    {
                    //        layoutPersist.WriteXml(fragmentWriter);
                    //        databasePersist.WriteXml(fragmentWriter);
                    //    }

                    //    // </Project>
                    //    documentWriter.WriteEndElement();
                    //}
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Persist store error ...", exc);
            }
        }

        public void Load(string path)
        {
            try
            {
                MainLayout.TreeView.ClearTreeViewNodes();

                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        reader.ReadStartElement("Project");

                        //LayoutPersist layoutPersist = new LayoutPersist(MainLayout);
                        //layoutPersist.ReadXml(reader);

                        DatabasePersist databasePersist = new DatabasePersist();
                        databasePersist.ReadXml(reader);

                        reader.ReadEndElement();
                        
                        // Add databases in TreeView.
                        foreach (var database in databasePersist.Databases)
                            MainLayout.TreeView.CreateTreeViewNode(database.Key, database.Value);
                    }
                }

                MainLayout.TreeView.ExpandAll();
                MainLayout.TreeView.SelectFirstNode();

                //RefreshPropertiesFrame();

            }
            catch (Exception exc)
            {
                Logger.Error("Persist load error ...", exc);
            }
        }
    }
}