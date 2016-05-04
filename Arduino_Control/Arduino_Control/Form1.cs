using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Data.SqlClient;

namespace Arduino_Control
{
    public partial class Form1 : Form
    {
        // Serial Port
        private SerialPort serialPort = new SerialPort();
        const string TERM_CHAR = "\n";

        private int[] baudrates = { 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };// Available baud rates
        // Threads
        Thread t;
        ManualResetEvent runThread = new ManualResetEvent(false);

        // Delegates
        private delegate void DelegateAddToList(string msg);
        private DelegateAddToList m_DelegateAddToList;
        private delegate void DelegateStopPerfmormClick();
        private DelegateStopPerfmormClick m_DelegateStopPerfmormClick;

        SqlDataAdapter DA;
        DataTable dt = new DataTable();
        SqlCommandBuilder ComB;
        public SqlConnection con2db=new SqlConnection() ;//("data Source=MINA-PC\\SQLEXPRESS;Initial Catalog=smart_home;Integrated Security=True;");
      
        private int[] childs;
        private string[] parts;
        int cur_room_id=0;
        string cur_room_description = "";
        string cur_path = "";

        public Form1()
        {
            InitializeComponent();
            TreeNode n = new TreeNode("House");
            string str = Properties.Settings.Default.ConStr;
            con2db.ConnectionString = str;
            treeView1.Nodes.Add(n);
            PopulateTreeView(1, treeView1.Nodes[0]);
        }
       
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
        private string gettitle(int ind)
        {

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
            con2db.Close();
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

       
        private void Form1_Load(object sender, EventArgs e)
        {
            FillPortComboBox();      // Fill Ports combobox
                                     // Fill baudrate_combobox
            foreach (int baudrate in baudrates)
                baudrate_combobox.Items.Add(baudrate.ToString());

            baudrate_combobox.SelectedIndex = 1;
            serialPort.NewLine = TERM_CHAR; // Set terminating character

            m_DelegateAddToList = new DelegateAddToList(AddToList);
            m_DelegateStopPerfmormClick = new DelegateStopPerfmormClick(stop_button.PerformClick);
            //serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);

            t = new Thread(ReceiveThread);
            t.Start();
        }
        // Fill port combo box with available COM ports
        private void FillPortComboBox()
        {
            port_combobox.Items.Clear();

            SerialPort tmp;

            foreach (string portname in SerialPort.GetPortNames())
            {

                if (port_combobox.Items.Contains(portname)) continue;
                tmp = new SerialPort(portname);

                try
                {
                    tmp.Open();
                    if (tmp.IsOpen)
                    {
                        tmp.Close();
                        port_combobox.Items.Add(portname);
                    }
                }
                catch { }

            }


            port_combobox.Text = "";
            try { port_combobox.SelectedIndex = 0; }
            catch { }
        }

        private void ReceiveThread()
        {
            while (true)
            {
                runThread.WaitOne(Timeout.Infinite);

                while (true)
                {
                    try
                    {
                        // receive data 
                        string msg = serialPort.ReadLine();
                        this.Invoke(this.m_DelegateAddToList, new Object[] { "R: " + msg });
                    }
                    catch
                    {
                        try
                        {
                            this.Invoke(this.m_DelegateStopPerfmormClick, new Object[] { });
                        } catch { }

                        runThread.Reset();
                        break;
                    }

                }
            }
        }


        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // blocks until TERM_CHAR is received
            try
            {
                string msg = serialPort.ReadLine();
                this.Invoke(this.m_DelegateAddToList, new Object[] { "R: " + msg });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                stop_button.PerformClick();
            }

        }

        private void AddToList(string msg)
        {
            int n = msg_listbox.Items.Add(msg);
            msg_listbox.SelectedIndex = n;
            msg_listbox.ClearSelected();
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            
            if (port_combobox.Text == "" || baudrate_combobox.Text == "")
            {
                MessageBox.Show("You must specify serial port and baud rate.");
                return;
            }

            // Open serial port
            serialPort.PortName = port_combobox.Text;
            serialPort.BaudRate = Convert.ToInt32(baudrate_combobox.Text);

            try
            {
                serialPort.Open();
                serialPort.WriteLine("s");
                groupBox2.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Connection established
            
            port_combobox.Enabled = false;
            baudrate_combobox.Enabled = false;
            start_button.Enabled = false;
            stop_button.Enabled = true;
            int n = msg_listbox.Items.Add("Connection established...");
            msg_listbox.SelectedIndex = n;
            msg_listbox.ClearSelected();

            runThread.Set();
            
        }

        private void stop_button_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort.IsOpen == true)
                {
                    // Connection  closed
                   // serialPort.Write("p");
                    serialPort.Close();
                    groupBox2.Enabled = false;
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            runThread.Reset();

            port_combobox.Enabled = true;
            baudrate_combobox.Enabled = true;
            start_button.Enabled = true;
            stop_button.Enabled = false;
            int n = msg_listbox.Items.Add("Connection closed.");
            msg_listbox.SelectedIndex = n;
            msg_listbox.ClearSelected();

            // Refill port_combobox
            FillPortComboBox();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            base.OnClosing(e);

            // Close serial port (if opened)
            if (serialPort.IsOpen)
            {
                stop_button.PerformClick();
            }

            // Abort thread
            t.Abort();
            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            serialPort.WriteLine("w");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            serialPort.WriteLine("r");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            serialPort.WriteLine("m");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            serialPort.WriteLine("p");
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Form1 frm = new Form1();
            frm.ShowDialog();
            this.Close();
        }

        private bool is_aroom(int id)
        {
            bool r = false;
            string device;
            con2db.Close();
            con2db.Open();
            SqlCommand cmd = new SqlCommand("select  id  from device where root_id =" + id + " ", con2db);
            try
            {
                device = cmd.ExecuteScalar().ToString();
                if (device.Length > 0)
                    r = true;
            }
            catch { }
            con2db.Close();
            return r;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (is_aroom(cur_room_id))
            {
                Operation frm = new Operation(cur_room_id,cur_path,cur_room_description);
                frm.ShowDialog();
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            con2db.Close();
            con2db.Open();
            try
            {
                int id = getid(treeView1.SelectedNode.Text);
                cur_room_id = id;
                cur_path = treeView1.SelectedNode.FullPath;
                
                string sql_s = "select * from building where id=" + id + "";
                try
                {
                    DA = new SqlDataAdapter(sql_s, con2db);
                    dt.Clear();
                    DA.Fill(dt);
                    ComB = new SqlCommandBuilder(DA);
                    dataGridView1.DataSource = dt;
                    con2db.Close();
                    cur_room_description = " >> Title: "+dataGridView1[1, 0].Value + "  - Type: " + dataGridView1[3, 0] .Value+ "  - Desc: " + dataGridView1[4, 0].Value;
                }
                catch { }
            }
            catch { }
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            button1_Click_1(sender, e);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            Device frm = new Device();
            frm.ShowDialog();
        }



        //==========================================================
    }
}
