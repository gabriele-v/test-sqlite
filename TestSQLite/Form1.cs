using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SQLite;
using SQLitePCL;

namespace TestSQLite
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class Test
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            [MaxLength(8)]
            public string TestText { get; set; }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            SQLite3Plugin.Init();
            SQLitePCL.ISQLite3Provider imp = new SQLitePCL.SQLite3Provider_sqlcipher();
            SQLitePCL.raw.SetProvider(imp);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "UnEncrypted DB File|*.udb";
            saveFileDialog1.Title = "Save UnEncrypted DB";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName == "")
                return;

            var db = new SQLiteConnection(saveFileDialog1.FileName);
            db.CreateTable<Test>();

            db.Insert(new Test()
            {
                TestText = DateTime.Now.ToString()
            });

            var query = db.Table<Test>();

            foreach (var test in query)
                Console.WriteLine("Test: " + test.TestText);
            db.Dispose();
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "UnEncrypted DB File|*.udb";
            openFileDialog1.Title = "Open UnEncrypted DB";
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName == "")
                return;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Encrypted DB File|*.edb";
            saveFileDialog1.Title = "Save Encrypted DB";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName == "")
                return;

            SQLite3Plugin.Init();
            SQLitePCL.ISQLite3Provider imp = new SQLitePCL.SQLite3Provider_sqlcipher();
            SQLitePCL.raw.SetProvider(imp);

            sqlite3 db;
            raw.sqlite3_open(openFileDialog1.FileName, out db);
            raw.sqlite3_exec(db, "ATTACH DATABASE '"+ saveFileDialog1.FileName + "' AS encrypted KEY 'testkey';");
            raw.sqlite3_exec(db, "SELECT sqlcipher_export('encrypted');");
            raw.sqlite3_exec(db, "DETACH DATABASE encrypted;");
            db.Dispose();
        }

        private void EncryptRead_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Encrypted DB File|*.edb";
            openFileDialog1.Title = "Open Encrypted DB";
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName == "")
                return;

            SQLite3Plugin.Init();
            SQLitePCL.ISQLite3Provider imp = new SQLitePCL.SQLite3Provider_sqlcipher();
            SQLitePCL.raw.SetProvider(imp);
            var db = new SQLiteConnection(openFileDialog1.FileName);
            db.Execute("PRAGMA key = 'testkey';");
            var query = db.Table<Test>();
            foreach (var test in query)
                MessageBox.Show(test.TestText);
        }
    }
}
