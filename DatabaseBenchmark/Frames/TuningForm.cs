using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseBenchmark.Frames
{
    public partial class TuningForm : Form
    {
        private DataTable Table;
        private List<KeyValuePair<string, int[]>> Values;
        private List<List<int>> Combinations;
        private Database SourceDatabase;
        private Random random;
        private List<Database> TuningDatabaseInstances;

        public Action<bool> OkBtnClicked;

        public TuningForm()
        {
            InitializeComponent();
            CenterToParent();
        }

        public void Initialize(List<Tuple<string, object>> properties, Database database)
        {
            Values = new List<KeyValuePair<string, int[]>>();
            Combinations = new List<List<int>>();
            TuningDatabaseInstances = new List<Database>();
            random = new Random();

            SourceDatabase = database;
            Table = new DataTable();
            Table.Columns.Add(" ", typeof(bool));

            //Property Column 
            DataColumn column = new DataColumn("Property");

            column.ReadOnly = true;
            Table.Columns.Add(column);

            Table.Columns.Add("Min Value");
            Table.Columns.Add("Step");
            Table.Columns.Add("Max Value");

            foreach (var tuple in properties)
            {
                Table.Rows.Add(false, tuple.Item1, tuple.Item2, 0, tuple.Item2);                
            }

            gvPublicProperties.AutoGenerateColumns = true;
            gvPublicProperties.DataSource = Table;

            Controls.Add(gvPublicProperties);
        }

        public List<KeyValuePair<string, Color>> GetSelectedDatabasesChartValues()
        {
            return new List<KeyValuePair<string, Color>>(TuningDatabaseInstances.Select(x => new KeyValuePair<string, Color>(x.Name, x.Color)));
        }

        public List<Database> GetTuningDatabaseInstances()
        {
            return TuningDatabaseInstances;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            KeyValuePair<string, int[]> currentProperty;

            object[] tmpArray;
            int[] values;

            for (int i = 0; i < Table.Rows.Count; i++)
            {
                if ((bool)Table.Rows[i][0] == true)
                {
                    tmpArray = Table.Rows[i].ItemArray;

                    int min = Convert.ToInt32(tmpArray[2]);
                    int step = Convert.ToInt32(tmpArray[3]);
                    int max = Convert.ToInt32(tmpArray[4]);

                    if (min > max || step > max)
                        return;

                    int count = ((max - min) / step);
                    count += 1;

                    values = new int[count];

                    for (int j = 0; j < count; j++)
                    {
                        values[j] = min;
                        min += step;
                    }

                    currentProperty = new KeyValuePair<string, int[]>(tmpArray[1].ToString(), values);
                    Values.Add(currentProperty);
                }
            }
            if (Values.Count == 0)
                return;

            GetCombinations(Values, 0, new int[Values.Count]);

            foreach (var property in Combinations)
                TuningDatabaseInstances.Add(GetNewDatabase(property));

            Utils.DirectoryUtils.CreateAndSetDatabasesDataDirectory(MainForm.DATABASES_DIRECTORY, TuningDatabaseInstances);

            DialogResult = DialogResult.OK;
            Close();
        }

        private Database GetNewDatabase(List<int> customProperties)
        {
            Database database = ReflectionUtils.CreateDatabaseInstance(SourceDatabase.GetType());
            for (int i = 0; i < customProperties.Count; i++)
                ReflectionUtils.ChangePropertyValue(Values[i].Key, customProperties[i], database);

            database.Name = string.Format("{0} {1}", SourceDatabase.Name, String.Join("x", customProperties));
            database.Color = Color.FromArgb(random.Next(0, 250), random.Next(0, 250), random.Next(0, 250));

            return database;
        }

        private void GetCombinations(List<KeyValuePair<string, int[]>> values, int index, int[] result)
        {
            for (int i = 0; i < values[index].Value.Length; i++)
            {
                result[index] = values[index].Value[i];

                if (index == values.Count - 1)
                    Combinations.Add(result.ToList());
                else
                    GetCombinations(values, index + 1, result);
            }
        }

        private void gvPublicProperties_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
