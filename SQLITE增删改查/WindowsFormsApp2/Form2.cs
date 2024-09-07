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
    public partial class Form2 : Form
    {
        int type1;
        public Form2(Person person, int type)
        {
            InitializeComponent();
            type1 = type;
            if (type1 == 1)
            {
                btnOK.Text = "添加";
            }
            txtName.Text = person.name;
            txtQQ.Text = person.QQ;
            txtID.Text = person.id.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (type1 == 1)
            {
                Person person = new Person();
                person.name = txtName.Text;
                person.QQ = txtQQ.Text;
                string strSql = "insert into person (name,QQ) values(@name,@QQ)";
                SQLiteParameter[] paras = new SQLiteParameter[]
                {
                 new SQLiteParameter("@name",txtName.Text),
                 new SQLiteParameter("@QQ",txtQQ.Text)
                };
                if (SQLiteHelper.ExecSQL(strSql, paras))
                {
                    MessageBox.Show("添加成功");
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("删除失败");
                }
            }
            else
            {
                string strSql = "update person set name=@name,QQ=@QQ where id=@id";
                SQLiteParameter[] paras = new SQLiteParameter[]
                {
                new SQLiteParameter("@name",txtName.Text),
                new SQLiteParameter("@QQ",txtQQ.Text),
                new SQLiteParameter("@id",txtID.Text)
                };
                if (SQLiteHelper.ExecSQL(strSql, paras))
                {
                    MessageBox.Show("修改成功");
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("修改失败");
                }
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
