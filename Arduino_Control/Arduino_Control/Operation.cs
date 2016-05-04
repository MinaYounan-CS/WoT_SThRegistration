using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;

namespace Arduino_Control
{
    public partial class Operation : Form
    {
        public Operation(int room,string path,string des)
        {
            InitializeComponent();
            cur_room_id = room;
            textBox4.Text = "" + room;
            textBox3.Text=" Path is: "+path;
            textBox3.Text+="\n "+des;
            
        }
        int cur_room_id;
        SqlDataAdapter DA,DA_op,DA_op_in;
        DataTable dt = new DataTable();
        DataTable dt_op = new DataTable();
        DataTable dt_op_in = new DataTable();
        SqlCommandBuilder ComB;
        public SqlConnection con2db = new SqlConnection();//"data Source=MINA-PC\\SQLEXPRESS;Initial Catalog=smart_home;Integrated Security=True;");
       
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
           // serialport.Write(trackBar1.Value.ToString());
            label8.Text = trackBar1.Value.ToString()+" %";
        }
        void load_resources()
        {
            con2db.Close();
            con2db.Open();
            try
            {
                
                string sql_s = "select id, title,model from device where root_id=" + cur_room_id + "";
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
            catch { }

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            con2db.Close();
            con2db.Open();
            this.dataGridView1.CurrentCell = this.dataGridView1[0, dataGridView1.CurrentCell.RowIndex];
            int device_id = int.Parse(this.dataGridView1.CurrentCell.Value + "");
            try
            {
                string sql_s= " SELECT        dbo.operation.id_operation, dbo.operation.title, dbo.operation.description"
                            + "  FROM            dbo.device_opeartion INNER JOIN dbo.operation ON dbo.device_opeartion.id_operation = dbo.operation.id_operation"
                            + " WHERE        (dbo.device_opeartion.id_device =" + device_id +")";
               
                try
                {
                    
                    DA_op = new SqlDataAdapter(sql_s, con2db);
                    dt_op.Clear();
                    DA_op.Fill(dt_op);
                    ComB = new SqlCommandBuilder(DA_op);
                    dataGridView2.DataSource = dt_op;
                    con2db.Close();
                }
                catch { }
            }
            catch { }

        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            con2db.Close();
            con2db.Open();
            try
            {
                int operation_id = int.Parse(this.dataGridView2[0, dataGridView2.CurrentCell.RowIndex].Value + "");
                textBox1.Text = "" + operation_id;
                string sql_s=  " SELECT  dbo.operation_input.title, dbo.operation_input.type, dbo.operation_input.default_value, dbo.operation_input.nullable"
                            +" FROM    dbo.operation_input INNER JOIN"
                            +"         dbo.operation ON dbo.operation_input.id_operation = dbo.operation.id_operation"
                            +" WHERE   (dbo.operation.id_operation = " + operation_id + ")";
              
                try
                {

                    DA_op_in = new SqlDataAdapter(sql_s, con2db);
                    dt_op_in.Clear();
                    DA_op_in.Fill(dt_op_in);
                    ComB = new SqlCommandBuilder(DA_op_in);
                    dataGridView3.DataSource = dt_op_in;
                    con2db.Close();
                }
                catch { }
            }
            catch { }
        }
//----------------------------------------------------------------
        private System.Threading.Timer tmrThreadingTimer;
        private delegate void ShowTimerEventFiredDelegate(int cVal);
        bool goingUp = true;
        int i = 0;
        int interval = 50;
        
        private void StartTimer()
        {
            //if (i > point)
            //    goingUp = false;
            //else
            //    goingUp = true;
            
            try
            {
                if (tmrThreadingTimer == null)
                {
                    tmrThreadingTimer = new System.Threading.Timer(
                        new TimerCallback(tmrThreadingTimer_TimerCallback),
                        null,
                        0,
                        interval
                        );
                }
                else
                {
                    tmrThreadingTimer.Change(0, interval);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Message: " + ex.Message + "\n\tError type: " + ex.GetType().ToString(), "Error starting thread");
            }
        }

        private void StopTimer()
        {
            try
            {
                if (tmrThreadingTimer != null)
                {
                    tmrThreadingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Message: " + ex.Message + "\n\tError type: " + ex.GetType().ToString(), "Error stopping thread");
            }
        }
        private void tmrThreadingTimer_TimerCallback(object state)
        {
            try
            {
                if (i == int.Parse(textBox5.Text))
                    StopTimer();
                if (goingUp)
                {
                    i++;
                    //if (i > basicProgressBar1.Maximum)
                    //{
                    //    goingUp = false;
                    //    i = basicProgressBar1.Maximum;
                    //}
                    if (i > int.Parse(textBox5.Text))
                    {
                        goingUp = false;
                        i = int.Parse(textBox5.Text);
                    }
                }
                else
                {
                    i--;
                    //if (i < basicProgressBar1.Minimum)
                    //{
                    //    goingUp = true;
                    //    i = basicProgressBar1.Minimum;
                    //}
                    if (i < int.Parse(textBox5.Text))
                    {
                        goingUp = true;
                        i = int.Parse(textBox5.Text);
                    }

                }
                ShowTimerEventFired(i);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Message: " + ex.Message + "\n\tError type: " + ex.GetType().ToString(), "Error in TimerCallback");
            }
        }
        private void ShowTimerEventFired(int cVal)
        {
            //try
            //{
            //    //InvokeRequired will be true when using System.Threading.Timer
            //    //or System.Timers.Timer (without a SynchronizationObject)...
            //    if (basicProgressBar1.InvokeRequired)
            //    {
            //        //Marshal this call back to the UI thread (via the form instance)...
            //        BeginInvoke(
            //            new ShowTimerEventFiredDelegate(ShowTimerEventFired),
            //            new object[] { cVal }
            //            );
            //    }
            //    else
            //    {
            //        basicProgressBar1.Value = cVal;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Message: " + ex.Message + "\n\tError type: " + ex.GetType().ToString(), "Error in ShowTimerEventFired");
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartTimer();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StopTimer();
        }

        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Operation_Load(object sender, EventArgs e)
        {
            string str = Properties.Settings.Default.ConStr;
            con2db.ConnectionString = str;
            load_resources();
        }

        //================================================
    }
}
