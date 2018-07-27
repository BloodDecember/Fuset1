using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fuset
{
    public partial class Setting_format : Form
    {
        
        SQLiteConnection m_dbConn = new SQLiteConnection("Data Source=sample.sqlite;Version=3;");//Пример нормального считывания данных в грид данных
        SQLiteCommand m_sqlCmd = new SQLiteCommand();
        SQLiteDataAdapter adapter = new SQLiteDataAdapter();


        public void DataGridUpdate()
        {
            DataTable dTable = new DataTable();
            m_dbConn.Open();
            adapter = new SQLiteDataAdapter("SELECT id, akk, prof, proxy FROM Setting", m_dbConn);
            adapter.Fill(dTable);
            if (dTable.Rows.Count > 0)
            {
                dataGridView1.Rows.Clear();

                for (int i = 0; i < dTable.Rows.Count; i++)
                    dataGridView1.Rows.Add(dTable.Rows[i].ItemArray);
            }



            DataTable dTable1 = new DataTable();
            adapter = new SQLiteDataAdapter("SELECT proxy, usage FROM Proxy_list", m_dbConn);
            adapter.Fill(dTable1);
            if (dTable1.Rows.Count > 0)
            {
                dataGridView2.Rows.Clear();

                for (int i = 0; i < dTable1.Rows.Count; i++)
                    dataGridView2.Rows.Add(dTable1.Rows[i].ItemArray);
            }
            m_dbConn.Close();

        }




        public Setting_format()
        {
            InitializeComponent();
        }

        private void Setting_format_Load(object sender, EventArgs e)
        {

            DataGridUpdate();
        }
    }
}
