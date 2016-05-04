using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Arduino_Control
{
    class Global_Process
    {
        private static SqlConnection con2db = new SqlConnection();//("data Source=MINA-PC\\SQLEXPRESS;Initial Catalog=smart_home;Integrated Security=True;");
        
        public static void LoadCompWithCondition(ComboBox CBox, string table_name, string display_column, string value_column, string condition)
        {
            //  SqlConnection con;
            DataTable dt = new DataTable();

            //con = new SqlConnection(" server= . ;" + "database=" + G_Variable.DataBase + ";Trusted_Connection=True;");
            //con.Open();
            con2db.ConnectionString = Properties.Settings.Default.ConStr;
            con2db.Close();
            con2db.Open();
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "select " + display_column + " , " + value_column + " from " + table_name + " where " + condition + " ";
            cmd.Connection = con2db;
            SqlDataAdapter adaptorr = new SqlDataAdapter(cmd);
            DataTable DTt = new DataTable();
            adaptorr.Fill(DTt);
            CBox.DataSource = DTt;
            CBox.DisplayMember = "" + display_column + "";
            CBox.ValueMember = "" + value_column + "";
            con2db.Close();
        }
        public static void LoadCompWithCondition(ComboBox CBox, string table_name, string display_column, string value_column)
        {
            //  SqlConnection con;
            DataTable dt = new DataTable();

            //con = new SqlConnection(" server= . ;" + "database=" + G_Variable.DataBase + ";Trusted_Connection=True;");
            //con.Open();
            con2db.ConnectionString = Properties.Settings.Default.ConStr;
            con2db.Close();
            con2db.Open();
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "select " + display_column + " , " + value_column + " from " + table_name + " ";
            cmd.Connection = con2db;
            SqlDataAdapter adaptorr = new SqlDataAdapter(cmd);
            DataTable DTt = new DataTable();
            adaptorr.Fill(DTt);
            CBox.DataSource = DTt;
            CBox.DisplayMember = "" + display_column + "";
            CBox.ValueMember = "" + value_column + "";
            con2db.Close();
        }
    }
}
