using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            string strSql = "select * from person";
            DataSet ds = SQLiteHelper.ExecuteQuery(strSql);
            if (ds != null && ds.Tables.Count > 0)
            {
                dataGridView1.DataSource = ds.Tables[0];
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(new Person(), 1);
            form2.ShowDialog();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataGridViewRow dr = dataGridView1.CurrentRow;
            if (dr == null)
            {
                MessageBox.Show("请选择修改的数据");
                return;
            }
            string strSql = "delete from person where id=@id";
            SQLiteParameter para = new SQLiteParameter("@id", dr.Cells["id"].Value);
            if (SQLiteHelper.ExecSQL(strSql, para))
            {
                MessageBox.Show("删除成功");
            }
            else
            {
                MessageBox.Show("删除失败");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DataGridViewRow dr = dataGridView1.CurrentRow;
            if (dr == null)
            {
                MessageBox.Show("请选择修改的数据");
                return;
            }
            Person person = new Person();
            person.id = int.Parse(dr.Cells["id"].Value.ToString());
            person.name = dr.Cells["name"].Value.ToString();
            person.QQ = dr.Cells["QQ"].Value.ToString();
            Form2 form2 = new Form2(person, 2);
            form2.ShowDialog();

        }
    }

}
