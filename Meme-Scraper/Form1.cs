using MaterialSkin;
using MaterialSkin.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;

namespace Meme_Scraper
{
    public partial class Form1 : MaterialForm
    {
        public Form1()
        {
            InitializeComponent();

            /*Setup theme*/
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Red800, Primary.Red900, Primary.Red500, Accent.Red700, TextShade.WHITE);

            /*Disable cross thread checking*/
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        /*Public statements for use in different voids*/
        public FolderBrowserDialog folderbrowserdialog = new FolderBrowserDialog();

        /*Public variables for use in different voids*/
        public class Variables
        {
            private static int pages = 1;
            public static int PAGES { get { return pages; } set { pages = value; } }

            private static int memes = 1;
            public static int MEMES { get { return memes; } set { memes = value; } }

            private static bool enabled = false;
            public static bool ENABLED { get { return enabled; } set { enabled = value; } }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                materialSingleLineTextField1.Text = Directory.GetCurrentDirectory() + @"\memes";
            }
            catch
            {
                /*Unable to get current directory = blank*/
            }
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            /*Select folder*/
            folderbrowserdialog.ShowDialog();
            materialSingleLineTextField1.Text = folderbrowserdialog.SelectedPath;
        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {
            /*Empty textbox = error*/
            if (materialSingleLineTextField1.Text == "")
            {
                MessageBox.Show("Please fill in a valid save location.", "Meme scraper", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                /*Start scanner / setup form*/
                toggle_button();
            }
        }

        private void toggle_button()
        {
            /*If already disabled, enable*/
            if (Variables.ENABLED == false)
            {
                /*Folder checking*/
                if (Directory.Exists(materialSingleLineTextField1.Text))
                {
                    /*Directory exists, run program*/
                    Variables.ENABLED = true;
                    panel1.Enabled = false;
                    materialLabel4.Text = "Status: running";
                    materialRaisedButton2.Text = "Stop";
                    scanner();
                }
                else
                {
                    /*Directory doesn't exists, create + run program*/
                    try
                    {
                        Directory.CreateDirectory(materialSingleLineTextField1.Text);
                        Variables.ENABLED = true;
                        panel1.Enabled = false;
                        materialLabel4.Text = "Status: running";
                        materialRaisedButton2.Text = "Stop";
                        scanner();
                    }
                    /*Error creating directory*/
                    catch
                    {
                        MessageBox.Show("A directory error occured, please make sure you have entered a correct directory or have write permissions.", "Meme scraper", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            /*If already enabled, disable*/
            else
            {
                Variables.ENABLED = false;
                panel1.Enabled = true;
                materialLabel4.Text = "Status: idle";
                materialRaisedButton2.Text = "Start";
            }
        }

        public void scanner()
        {
            /*Scan memedroid pages*/
            (new Thread(() =>
            {
                while (true)
                {
                    WebClient webClient = new WebClient();
                    Uri URL = new Uri("http://www.memedroid.com/memes/latest/" + Variables.PAGES);
                    webClient.OpenReadAsync(URL);
                    webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(scraper);
                    materialLabel2.Text = "Pages scanned: " + Variables.PAGES;
                    Variables.PAGES += 1000000;
                    Thread.Sleep(500);
                    if (Variables.ENABLED == false) { break; }
                }
            }
            )).Start();
        }

        public void scraper(object sender, OpenReadCompletedEventArgs e)
        {
            /*Stupid regex a.k.a brain tumor*/
            const string PATTERN = @"img src=""http://images(?<link>.+).jpeg""";
            Regex regex = new Regex(PATTERN, RegexOptions.IgnoreCase);
            TextReader TR = new StreamReader(e.Result);
            string content = TR.ReadToEnd();
            MatchCollection MC = regex.Matches(content);
            foreach (Match match in MC)
            {
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(new Uri("http://images" + match.Groups["link"].ToString() + ".jpeg"), materialSingleLineTextField1.Text + @"\" + RandomString(6) + ".jpeg");
                    materialLabel3.Text = "Memes downloaded: " + Variables.MEMES;
                    Variables.MEMES++;
                }
                catch
                {
                    MessageBox.Show("A fatal error has occured, the program has shut down.", "Meme scraper", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            TR.Close();
        }

        /*Random image names*/
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }
    }
}
