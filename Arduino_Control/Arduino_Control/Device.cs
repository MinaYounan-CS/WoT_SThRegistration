using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace Arduino_Control
{
    public partial class Device : Form
    {
        public Device()
        {
            InitializeComponent();
            con2db.ConnectionString=Properties.Settings.Default.ConStr;
            textBox1.Text = "" +(1+ get_max_id());
            TreeNode n = new TreeNode("House");

            treeView1.Nodes.Add(n);
            PopulateTreeView(1, treeView1.Nodes[0]);
            Global_Process.LoadCompWithCondition(comboBox1, "operation", "title", "id_operation");
            id_op_list = new int[comboBox1.Items.Count];

            comboBox2.Items.Clear();
            for (int j = 0; j < 14; j++)
                comboBox2.Items.Add("" + j);
            comboBox2.SelectedIndex = 0;
        }

        SqlDataAdapter DA;
        DataTable dt = new DataTable();
        SqlCommandBuilder ComB;
        public SqlConnection con2db = new SqlConnection();//("data Source=MINA-PC\\SQLEXPRESS;Initial Catalog=smart_home;Integrated Security=True;");

        private int[] childs;
        private string[] parts;
        int cur_room_id = 0;
        string cur_path = "";

        string txtPath;
        int[] id_op_list;
        int id_op_count = 0;

        private void PopulateTreeView(int directoryValue, TreeNode parentNode)
        {
            con2db.Close();
            con2db.Open();
            SqlCommand cmd = new SqlCommand("select distinct id,title from building where root_id=" + directoryValue + "", con2db);
            SqlDataAdapter dt = new SqlDataAdapter(cmd);
            DataSet st = new DataSet();
            dt.Fill(st);
            int count = st.Tables[0].Rows.Count;
            DataRowCollection dtc = st.Tables[0].Rows;
            childs = new int[count];
            parts = new string[count];
            int counter = 0;
            foreach (DataRow dr in dtc)
            {
                childs[counter] = int.Parse(dr[0].ToString());
                parts[counter] = dr[1].ToString(); counter++;
            }
            try
            {
                if (childs.Length != 0)
                {
                    foreach (int directory in childs)
                    {
                        string mina = gettitle(directory);
                        TreeNode myNode = new TreeNode(mina);

                        parentNode.Nodes.Add(myNode);
                        PopulateTreeView(directory, myNode);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }//parentNode.Nodes.Add("Access denied"); }

            //con2db.Close();
        }
        private int get_max_id()
        {
            int id;
            con2db.Close();
            con2db.Open();
            SqlCommand cmd = new SqlCommand("select  max(id) from device", con2db);
            try
            {
                id = int.Parse(cmd.ExecuteScalar().ToString());
            }
            catch { id = 0; }
            return id;
        }
        private string get_Description(int id)
        {
            con2db.Close();
            con2db.Open();
            string desc = "";
     
            SqlCommand cmd = new SqlCommand("SELECT   title, type, description FROM  dbo.building WHERE id = "+id+"", con2db);
            SqlDataReader r = cmd.ExecuteReader();

            try
            {
                r.Read();
                desc = ">Title: " + r.GetString(0) + " - >Type: " + r.GetString(1) + " - >Description: " + r.GetString(2);
            }
            catch { };
            
            return desc;
        }
        private string gettitle(int ind)
        {
            con2db.Close();
            con2db.Open();
            SqlCommand cmd = new SqlCommand("select  title  from building where id=" + ind + " ", con2db);
            string part = cmd.ExecuteScalar().ToString();
            return part;

        }
        private int getid(string title)
        {
            string part = "0";
            con2db.Close();
            con2db.Open();
            SqlCommand cmd = new SqlCommand("select  id  from building where title ='" + title + "' ", con2db);
            try
            {
                part = cmd.ExecuteScalar().ToString();
            }
            catch { }
            return int.Parse(part);

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void load_devices(int id)
        {
            con2db.Close();
            con2db.Open();
            string sql_s = "select * from device where root_id=" + id + "";
            try
            {
                DA = new SqlDataAdapter(sql_s, con2db);
                dt.Clear();
                DA.Fill(dt);
                ComB = new SqlCommandBuilder(DA);
                dataGridView1.DataSource = dt;
                con2db.Close();
            }
            catch { }
        }
        private string load_device_no(int room_id)
        {
            con2db.Close();
            con2db.Open();
            SqlCommand cmd = new SqlCommand("select count(id) from device ",con2db);
            string s = cmd.ExecuteScalar().ToString();
            cmd = new SqlCommand("select count(id) from device where root_id=" + room_id + "", con2db);
            s += " is " + cmd.ExecuteScalar().ToString();
            con2db.Close();
            return s;
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
           
            try
            {
                int id = getid(treeView1.SelectedNode.Text);
                cur_room_id = id;
                textBox3.Text = "" + cur_room_id;

                cur_path = treeView1.SelectedNode.FullPath;
                textBox8.Text = cur_path;
                textBox9.Text = get_Description(id);
                load_devices(id);
                lab_count.Text = load_device_no(id);
            }
            catch { }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            try
            {
                
                openFileDialog1.Filter = "عرض الصور(*.JPEG;*.JPG)|*.JPEG;*.JPG|All files (*.*)|*.*";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    txtPath = openFileDialog1.FileName;
                }
                pictureBox3.Image = Image.FromFile(txtPath);
            }
            catch { }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            try
            {

                openFileDialog1.Filter = "عرض الصور(*.JPEG;*.JPG)|*.JPEG;*.JPG|All files (*.*)|*.*";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    txtPath = openFileDialog1.FileName;
                }
                pictureBox4.Image = Image.FromFile(txtPath);
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox8.Text=="House")
                MessageBox.Show("Please Select Path to Add Your Device", "Info.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                data_process("insert");
        }

        private byte[] arrImage;
        private byte[] arrImage2;

        private void data_process(string fun)
        {
            bool img1 = false;
            bool img2 = false;
            
            con2db.Close();
            con2db.Open();// id, root_id, title, model, serial, manufacturer, description, img_on, img_off
            if (fun == "insert")
            {
               
                 try
                {
                    MemoryStream ms = new MemoryStream();
                    pictureBox3.Image.Save(ms, pictureBox3.Image.RawFormat);
                    arrImage = ms.GetBuffer();
                    ms.Close();
                    img1 = true;
                 }catch{}
                 try
                 {
                    MemoryStream ms2 = new MemoryStream();
                    pictureBox4.Image.Save(ms2, pictureBox4.Image.RawFormat);
                    arrImage2 = ms2.GetBuffer();
                    ms2.Close();
                    img2 = true;
                 }
                 catch { }
                 string sql_s = "insert into device values(@id,@root_id,@title,@model,@serial,@manufacturer,@description,@pin_id";
                 if (img1)
                     sql_s += ",@img_off";
                 if (img2)
                     sql_s += ",@img_on";
                 sql_s += ")";
                SqlCommand cmd = new SqlCommand(sql_s, con2db);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = int.Parse(textBox1.Text);
                cmd.Parameters.Add("@root_id", SqlDbType.Int).Value = int.Parse(textBox3.Text);
                cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = textBox2.Text;
                cmd.Parameters.Add("@model", SqlDbType.VarChar).Value = textBox4.Text;
                cmd.Parameters.Add("@serial", SqlDbType.VarChar).Value = textBox5.Text;
                cmd.Parameters.Add("@manufacturer", SqlDbType.VarChar).Value = textBox6.Text;
                cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = textBox7.Text;
                cmd.Parameters.Add("@pin_id", SqlDbType.NChar).Value = comboBox2.Text;
                if(img1)
                 cmd.Parameters.Add("@img_off", SqlDbType.Image).Value = arrImage;
                if(img2)
                 cmd.Parameters.Add("@img_on", SqlDbType.Image).Value = arrImage2;         

                try
                {
                    cmd.ExecuteNonQuery();
                    if (Insert_operation())
                    {
                        load_devices(cur_room_id);
                        load_device_no(cur_room_id);
                        lab_count.Text = load_device_no(cur_room_id);
                           
                        MessageBox.Show("تمت عملية الإضافة بنجاح", "تأكيد", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                        MessageBox.Show("A Warrning for storing functions", "Ensure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch { MessageBox.Show("لم تمت عملية الإضافة ", "تأكيد", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            con2db.Close();
           
                    
        }
        private bool Insert_operation()
        {
            con2db.Close();
            con2db.Open();
            SqlCommand cmd;
           
            for (int j = 0; j < id_op_count; j++)
            {
                cmd = new SqlCommand("insert into device_opeartion values(@id_device,@id_operation)", con2db);
                cmd.Parameters.Add("@id_device", SqlDbType.Int).Value = int.Parse(textBox1.Text);
                cmd.Parameters.Add("@id_operation", SqlDbType.Int).Value = id_op_list[j];
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch { return false; }
            }
            return true;
        }
        private void button13_Click(object sender, EventArgs e)
        {
            if (!listBox1.Items.Contains(comboBox1.Text))
            {
                listBox1.Items.Add(comboBox1.Text);
                id_op_list[id_op_count++] = int.Parse(comboBox1.SelectedValue.ToString());
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {

            int x = listBox1.SelectedIndex;
            if (x != -1)
            {
                int y = listBox1.Items.Count;
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                for (int j = x; j < y - 1; j++)
                    id_op_list[j] = id_op_list[j + 1];

                id_op_list[y - 1] = -1;
                id_op_count--;
            }
            else
            { MessageBox.Show("Please Select One Record","Info",MessageBoxButtons.OK,MessageBoxIcon.Information); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            con2db.Close();
            con2db.Open();
            int device_id=int.Parse(this.dataGridView1[0, dataGridView1.CurrentCell.RowIndex].Value+"");
            SqlCommand cmd = new SqlCommand("delete from device_opeartion where id_device="+device_id+"",con2db);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch { MessageBox.Show("No Fuctions or Operations Related with this device "); };
            cmd = new SqlCommand("delete from device where id=" + device_id + "", con2db);
            try
            {
                cmd.ExecuteNonQuery();
                load_devices(cur_room_id);
                lab_count.Text = load_device_no(cur_room_id);
                textBox1.Text = "" + (1 + get_max_id());
                load_device_no(cur_room_id);
            }
            catch { MessageBox.Show("No Such this Device to delete"); };
            con2db.Close();
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            for (int j = 0; j < 14; j++)
                comboBox2.Items.Add("" + j);
            comboBox2.SelectedIndex = 0;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            for (int j = 0; j < 6; j++)
                comboBox2.Items.Add("A" + j);
            comboBox2.SelectedIndex = 0;
        }

    }
}
