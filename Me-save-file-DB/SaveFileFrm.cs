using System;

using System.Data;
using System.Drawing;
using System.IO;
using System.Data.OleDb;

using System.Windows.Forms;

namespace Me_save_file_DB
{
    public partial class SaveFileFrm : Form
    {
        //ایجاد کانکشن استرینگ و کوئری های دیتابیس
        OleDbConnection cnn = new OleDbConnection();
        string StrCon = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + @"\FileSave.accdb";
        string StrAttach = "Select [ID],[filename],[filesize] From savetbl ORDER BY [filename]";
        string StrGetAttach = "Select * From [savetbl] Where [ID]=@Attach";
        string StrField = "Select * From savetbl";



        public SaveFileFrm()
        {
            InitializeComponent();
           

            this.Text = "Save File Databae";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

           //تنظیم ارتفاع دیتاگرید ویو
            DataGridViewRow row = this.dataGridView1.RowTemplate;
            row.DefaultCellStyle.BackColor = Color.Bisque;
            row.Height =40;
            row.MinimumHeight = 40;


            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.GreenYellow;
            dataGridView1.EnableHeadersVisualStyles = false;
            ////تغییر رنگ سطر و پس زمینه دیتاگریدویو
            //dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.BurlyWood;
            //dataGridView1.DefaultCellStyle.BackColor = Color.Coral;
            //فرخوانی دیتابیس یا همون رفرش شدن دیتابیس
            cnn.ConnectionString = StrCon;
            FillDataGrid(dataGridView1, StrAttach);
          //اجازه دستکاری و پاک کردن سلسو های رو از کاربر میگیریم
            dataGridView1.AllowUserToDeleteRows = false;
            //جلوگیری از اضافه شدن ردیف
            dataGridView1.AllowUserToAddRows = false;
            //فعال کردن قابلیت فقط خواندنی دیتاگرید
            dataGridView1.ReadOnly = true;
           

        }

        private void FillDataGrid(DataGridView DGV, string StrQuery)
        {
            //ساخت متد برای فراخوانی اطلاعات دیتابیس در دیتاگرید ویو
            DataTable dtbl = new DataTable();
            OleDbDataAdapter adap = new OleDbDataAdapter();
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = cnn;
            
            cmd.CommandText = StrQuery;
            adap.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            adap.SelectCommand = cmd;
            adap.Fill(dtbl);
            DGV.DataSource = dtbl;
        }
       
        private void button2_Click(object sender, EventArgs e)
        {
           // باتن انتخاب فایل جهت درج در دیتابیس
            if (OdfMain.ShowDialog() == DialogResult.OK)
            {//گرفتن اطلاعات فایل مانند اندازه و حجم جهت نمایش
               
                FileInfo FileSpec = new FileInfo(OdfMain.FileName);
               
                label1.Text = "File Size "+FileSpec.Length / 1204 + "  " + "KB" + "    ";
                //شرط حجم فایل در صورتیکه حجم اون از مقدار وارد شده بشتر باشه ثبت انجام نمیشه
                if ((FileSpec.Length / 1024) > 5100)
                {

                    lblmessage.Visible = true;
                    txtUplodeFile.Clear();
                    return;
                }
                else
                {
                    OdfMain.Title = "Select Any File";
                    txtUplodeFile.Text = Path.GetExtension(txtUplodeFile.Text);
                    txtUplodeFile.Text = OdfMain.FileName;
                }




            }







        }

        ThreadClass mythreadd = new ThreadClass();
        private void CreateAttach(string StrFile)
        {

           
            //متد دریافت ورودی یا ذخیره فایل در دیتابیس
            OleDbDataAdapter adap = new OleDbDataAdapter(StrField, cnn);
            adap.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            OleDbCommandBuilder Cbuld = new OleDbCommandBuilder(adap);
            //ساخت ابجکت از جدول
            DataTable dtbl = new DataTable();
            FileStream Ostream = new FileStream(StrFile, FileMode.Open, FileAccess.Read);
            //ارسال پارمترهای بایت برای ثبت در دیتابیس
            //تبدیل هر نوع فایل به ارایه ای از بایت
            int Oint = Convert.ToInt32(Ostream.Length);
            byte[] ObjData;
            ObjData = new byte[Oint];
            DataRow Orow;
            string[] StrPath = StrFile.Split(Convert.ToChar(@"\"));
            adap.Fill(dtbl);

            //****************insert date

            string dattim = DateTime.Now.ToString("HH:MM:ss");

            Ostream.Read(ObjData, 0, Oint);
            Ostream.Close();
            Orow = dtbl.NewRow();
            Orow["filename"] =dattim+ StrPath[StrPath.Length - 1];
            Orow["filesize"] = Oint / 1024+" KB";
            Orow["attachment"] = ObjData;
            
            dtbl.Rows.Add(Orow);
            adap.Update(dtbl);
           
        }
        private void Form1_MouseHover(object sender, EventArgs e)
        {
            lblmessage.Visible = false;

        }

        

        private void button1_Click_1(object sender, EventArgs e)
        {
            //باتن ثبت در دیتابیس در صورتیکه فایلی انتخاب نشد باشه هیچ اتفاقی نمیافته 
            
            if (txtUplodeFile.Text.Trim() == string.Empty)
            {
                return;
            }
            else
            {
               
                mythreadd.startprogress();
                progressBar1.Value = 0;
                //در صورت انتخاب فایل یا دیتا در دیتابیس ثبت میشه
                CreateAttach(OdfMain.FileName);
                FillDataGrid(dataGridView1, StrAttach);

                mythreadd.threadinEnd();
                progressBar1.Maximum = 100;

                progressBar1.Value = 100;
                MessageBox.Show("         File Save Success          ", "File Saving To Database");
                
                txtUplodeFile.Text = "";
                label1.Text = "";
                progressBar1.Value = 0;
                
            }
           
        }
      
       
        private void DownloadData(SaveFileDialog SvObject,DataGridView DGV)
        {
            //ساخت متد برای دریافت اطلاعات از دیتابیس
           
                string GetID = DGV.SelectedRows[0].Cells["ID"].Value.ToString();
            if (!string.IsNullOrEmpty(GetID))
            {
                //ساخت ابجکت برای دریافت دستورات با استفاده از پارامتر
                OleDbCommand cmd = new OleDbCommand(StrGetAttach,cnn);
                cmd.Parameters.AddWithValue("@Attach", GetID); 
                 OleDbDataAdapter adap = new OleDbDataAdapter(cmd);
                DataTable dtbl = new DataTable();
                DataRow ObjRow;
                adap.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                OleDbCommandBuilder oledcmd = new OleDbCommandBuilder(adap);
                adap.Fill(dtbl);
                ObjRow = dtbl.Rows[0];
                //ساخت ابکت از بایت برای تبدیل بایت به فایل
                byte[] objData;
                objData = (byte[])ObjRow["attachment"];

                if (SvObject.ShowDialog()==DialogResult.OK)
                {
                 
                    //مسیردهی برای ذخیره و دانلود فایل
                  
                     string SaveFil =SvObject.FileName;
                    
                  
                    FileStream Ostream = new FileStream(SaveFil, FileMode.Create,FileAccess.Write);
                   
                    Ostream.Write(objData,0,objData.Length);
                    Ostream.Close(); 
                    MessageBox.Show("                       Download   Success                 ","Download File Or Data From Database");
                }
                else
                {
                    MessageBox.Show("Download Canceled","Downloading File From Database Message");
                }
               
            }
           
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //باتن درخواست دانلود فایل و اجرای متد ذخیره فایل
            if (dataGridView1.CurrentRow==null)
            {
                return;
            }
            else
            {
                DownloadData(SdfMain, dataGridView1);
                FillDataGrid(dataGridView1, StrAttach);
            }
           
        }

        private void btndelete_Click(object sender, EventArgs e)
        {
            //متد حذف فایلهای ذخیره شده
            if (dataGridView1.CurrentRow!=null)
            {
                if (MessageBox.Show("آیا میخواهید این رکورد حذف شود؟","اخطار",MessageBoxButtons.OKCancel)==DialogResult.OK)
                {
                    
                    int Dlt = Convert.ToInt32(dataGridView1.CurrentRow.Cells[0].Value.ToString());
                    OleDbConnection cnn = new OleDbConnection(StrCon);
                    OleDbCommand cmd = new OleDbCommand("Delete From savetbl Where ID="+Dlt,cnn);
                    cnn.Open();
                    cmd.ExecuteNonQuery();
                    cnn.Close();
                    FillDataGrid(dataGridView1, StrAttach);
                    MessageBox.Show("Delete Success","Delete Data From Database");
                   
                }
            }

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://urmiapazar.ir");
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            ToolTip msg = new ToolTip();
            msg.AutoPopDelay = 5000;
            msg.InitialDelay = 500;
            msg.ReshowDelay = 500;
            msg.ShowAlways = true;
            
            msg.SetToolTip(this.pictureBox1, "پروگرمر محمدباقر آیرملو");
        }
    }
}
