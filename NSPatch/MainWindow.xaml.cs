using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace NSPatch
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string ktxt = AppDomain.CurrentDomain.BaseDirectory + "\\keys.txt";

            bool kcheck = File.Exists(ktxt);

            if (kcheck == false)
            {
                DialogResult kcheckd = System.Windows.Forms.MessageBox.Show(" keys.txt is missing.\n Please add it to the current working directory to ensure that\n" +
                    " updating NSP's will work properly.",
                    "Warning", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (kcheckd == System.Windows.Forms.DialogResult.Cancel)
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }

        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

        private void ipbutton_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = "NSW NSP File|*.nsp";
            openFileDialog.Title = "Select a NSW NSP File";

            if (openFileDialog.ShowDialog() == true)
                inputdisplay.Text = openFileDialog.FileName;

        }

        private void ptchbutton_Click(object sender, RoutedEventArgs e)
        {
           // extractnsp();
        }

        public void startbar()
        {
            bar1.IsIndeterminate = true;
        }

        public void stopbar()
        {
            bar1.IsIndeterminate = false;
        }

        public void offbtn()
        {
            ipbutton.IsEnabled = false;
            ptchbutton.IsEnabled = false;
            upnspipbutton.IsEnabled = false;
            updtbutton.IsEnabled = false;
        }

        public void onbtn()
        {
            ipbutton.IsEnabled = true;
            ptchbutton.IsEnabled = true;
            upnspipbutton.IsEnabled = true;
            updtbutton.IsEnabled = true;
        }

        public void readnsp()
        {

        }

        public void patchfwversion()
        {

        }

        public async void extractnsp()
        {
            offbtn();

            string tmpdir = AppDomain.CurrentDomain.BaseDirectory + "\\tmp";
            Directory.CreateDirectory(tmpdir);

            statuslabel.Content = "Extracting NSP Container...";

            string hctdir = AppDomain.CurrentDomain.BaseDirectory + "\\hactool.exe";
            string arg = @"-tpfs0 --pfs0dir=tmp " + "\"" + inputdisplay.Text + "\"";

            Process hct = new Process();
            hct.StartInfo.FileName = hctdir;
            hct.StartInfo.Arguments = arg;
            hct.StartInfo.CreateNoWindow = true;
            hct.StartInfo.UseShellExecute = false;
            hct.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            hct.EnableRaisingEvents = true;

            hct.Start();

            startbar();

            await Task.Run(() => hct.WaitForExit());

            hct.Close();

            readxml();
        }

        public void readxml()
        {
            statuslabel.Content = "Reading XML File...";

            string xmldir = AppDomain.CurrentDomain.BaseDirectory + @"\\tmp";
            string[] xmlfile = Directory.GetFiles(xmldir, "*.xml");

            if (xmlfile.Length != 1)
            {
                System.Windows.MessageBox.Show("There is no file to patch within your NSP file!");
                return;
            }

            XDocument xdoc = XDocument.Load(xmlfile[0]);
            foreach (var order in xdoc.Descendants("ContentMeta"))
            {
                string mrmk = order.Element("KeyGenerationMin").Value;
                {
                    keylabel.Content = mrmk;
                }

                string tid = order.Element("Id").Value;
                {
                    tidlabel.Content = tid.Substring(2);
                }
            }

            if (keylabel.Content.Equals(null))
            {
                fwlabel.Content = "???";

                DialogResult uspg = System.Windows.Forms.MessageBox.Show("This NSP is not supported!",
                "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);

                if (uspg == System.Windows.Forms.DialogResult.OK)
                    return;
            }

            if (keylabel.Content.Equals("0"))
            {
                fwlabel.Content = "all";
            }

            if (keylabel.Content.Equals("1"))
            {
                fwlabel.Content = "1.0.0";
            }

            if (keylabel.Content.Equals("2"))
            {
                fwlabel.Content = "3.0.0";
            }

            if (keylabel.Content.Equals("3"))
            {
                fwlabel.Content = "3.0.1";
            }

            if (keylabel.Content.Equals("4"))
            {
                fwlabel.Content = "4.0.0";
            }

            if (keylabel.Content.Equals("5"))
            {
                fwlabel.Content = "5.0.0";
            }

            if (keylabel.Content.Equals("6"))
            {
                fwlabel.Content = "???";

                DialogResult uspg = System.Windows.Forms.MessageBox.Show("This NSP is not supported yet!",
              "Error", MessageBoxButtons.OK,
               MessageBoxIcon.Error);

              if (uspg == System.Windows.Forms.DialogResult.OK)
              return;
            }
             
            patchxml();
        }

        public void patchxml()
        {
            statuslabel.Content = "Patching XML File...";

            string xmldir = AppDomain.CurrentDomain.BaseDirectory + @"\\tmp";
            string[] xmlfile = Directory.GetFiles(xmldir, "*.xml");
            XDocument xdoc = XDocument.Load(xmlfile[0]);

            var patch = xdoc.Root.Descendants("RequiredSystemVersion").FirstOrDefault();

            if (patch == null)
                return;

            patch.Value = "0";

            string xmlname = String.Join(",", xmlfile);
            xdoc.Save(xmlname);

            repacknsp();
        }

        public async void repacknsp()
        {
            statuslabel.Content = "Repacking NSP Container...";

            string nspbdir = AppDomain.CurrentDomain.BaseDirectory + "\\nspBuild.pyw";
            string[] tmpfiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\\tmp");
            string args = String.Join(" ", tmpfiles);

            Process nsb = new Process();
            nsb.StartInfo.FileName = nspbdir;
            nsb.StartInfo.Arguments = args;
            nsb.StartInfo.CreateNoWindow = true;
            nsb.StartInfo.UseShellExecute = true;
            nsb.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            nsb.EnableRaisingEvents = true;

            nsb.Start();

            await Task.Run(() => nsb.WaitForExit());

            nsb.Close(); 

            stopbar();

            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\\tmp", true);          

            System.Windows.MessageBox.Show("Congrats this NSP will now work on " + fwlabel.Content + "!");

            statuslabel.Content = "";
            keylabel.Content = "";
            fwlabel.Content = "";
            tidlabel.Content = "";

            onbtn();
        }

        private void upnspipbutton_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = "NSW NSP File|*.nsp";
            openFileDialog.Title = "Select a NSW NSP File";

            if (openFileDialog.ShowDialog() == true)
                upnspinputdisplay.Text = openFileDialog.FileName;
        }

        private void updtbutton_Click(object sender, RoutedEventArgs e)
        {
            checkttlky();
        }

        public void checkttlky()
        {
            string bsgtk = bsgtitlkyinput.Text;
            string uptk = updtitlkyinput.Text;

            if (bsgtk == "" || uptk == "")
            {
                DialogResult uspg = System.Windows.Forms.MessageBox.Show("You must fill in the Titlekeys!",
                "Error", MessageBoxButtons.OK,
                 MessageBoxIcon.Error);

                if (uspg == System.Windows.Forms.DialogResult.OK)
                    return;
            }
            else
            {
                checkbsdir();
            }
        }

        public void checkbsdir()
        {
            string bsdir = AppDomain.CurrentDomain.BaseDirectory + "\\tmp";

            if (Directory.Exists(bsdir))
            {
                decryptbsgnca();
            }
            else
            {
                reextractnsp();
            }
        }

        public async void reextractnsp()
        {
            offbtn();

            startbar();

            string tmpdir = AppDomain.CurrentDomain.BaseDirectory + "\\tmp";
            Directory.CreateDirectory(tmpdir);

            statuslabel.Content = "Extracting NSP Container...";

            string hctdir = AppDomain.CurrentDomain.BaseDirectory + "\\hactool.exe";
            string arg = @"-tpfs0 --pfs0dir=tmp " + "\"" + inputdisplay.Text + "\"";

            Process hct = new Process();
            hct.StartInfo.FileName = hctdir;
            hct.StartInfo.Arguments = arg;
            hct.StartInfo.CreateNoWindow = true;
            hct.StartInfo.UseShellExecute = false;
            hct.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            hct.EnableRaisingEvents = true;

            hct.Start();

            await Task.Run(() => hct.WaitForExit());

            hct.Close();

            decryptbsgnca();
        }
        
        public async void decryptbsgnca()
        {
            offbtn();

            startbar();

            statuslabel.Content = "Decrypting Base Game NCA...";

            string tmpdir = AppDomain.CurrentDomain.BaseDirectory + "\\tmp";

            var di = new DirectoryInfo(tmpdir);
            var result = di.GetFiles().OrderByDescending(x => x.Length).Take(1).ToList();
            var larbnca = di.GetFiles().OrderByDescending(x => x.Length).Take(1).Select(x => x.FullName).ToList();

            string basenca = String.Join(" ", larbnca);

            string nspddir = AppDomain.CurrentDomain.BaseDirectory + "\\hactool.exe";
            string titlkeyp = bsgtitlkyinput.Text;
            string bsgtk = new string(titlkeyp.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray());
            string arg1 = @"-k keys.txt﻿﻿﻿ " + "--titlekey=" + bsgtk + " " + basenca;
            string arg2 = " --plaintext=" + tmpdir + "\\NCAID_PLAIN.nca";
            string arg = arg1 + arg2;
          
            Process decrnca = new Process();
            decrnca.StartInfo.FileName = nspddir;
            decrnca.StartInfo.Arguments = arg;
            decrnca.StartInfo.CreateNoWindow = true;
            decrnca.StartInfo.UseShellExecute = false;
            decrnca.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            decrnca.EnableRaisingEvents = true;

            decrnca.Start();

            await Task.Run(() => decrnca.WaitForExit());

            decrnca.Close();

            extractncau();
        }

        public async void extractncau()
        {
            string updir = AppDomain.CurrentDomain.BaseDirectory + "\\upd";
            Directory.CreateDirectory(updir);

            statuslabel.Content = "Extracting Update NCA's...";

            string hctdir = AppDomain.CurrentDomain.BaseDirectory + "\\hactool.exe";
            string arg = @"-tpfs0 --pfs0dir=upd " + "\"" + upnspinputdisplay.Text + "\"";

            Process hct = new Process();
            hct.StartInfo.FileName = hctdir;
            hct.StartInfo.Arguments = arg;
            hct.StartInfo.CreateNoWindow = true;
            hct.StartInfo.UseShellExecute = false;
            hct.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            hct.EnableRaisingEvents = true;

            hct.Start();

            await Task.Run(() => hct.WaitForExit());

            hct.Close();

            applyupdate();
        }

        public async void applyupdate()
        {
            statuslabel.Content = "Merging NCA's...";

            string curdir = AppDomain.CurrentDomain.BaseDirectory;
            string tmpdir = AppDomain.CurrentDomain.BaseDirectory + "\\tmp";
            string upddir = AppDomain.CurrentDomain.BaseDirectory + "\\upd";
            string nspudir = AppDomain.CurrentDomain.BaseDirectory + "\\hactool.exe";
            string basenca = tmpdir + "\\NCAID_PLAIN.nca";

            var di = new DirectoryInfo(upddir);
            var result = di.GetFiles().OrderByDescending(x => x.Length).Take(1).ToList();
            var larupdnca = di.GetFiles().OrderByDescending(x => x.Length).Take(1).Select(x => x.FullName).ToList();

            string updnca = String.Join(" ", larupdnca);

            string titlkeyp = updtitlkyinput.Text;
            string upgtk = new string(titlkeyp.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray());

            string arg1 = @"-k keys.txt﻿﻿﻿ " + "--titlekey=" + upgtk + " --basenca=" + basenca + " --section1=" + curdir + "\\romfs.bin" + " --exefsdir=";
            string arg2 = tmpdir + "\\exefs " + updnca;
            string arg = arg1 + arg2; 

            Process aplupd = new Process();
            aplupd.StartInfo.FileName = nspudir;
            aplupd.StartInfo.Arguments = arg;
            aplupd.StartInfo.CreateNoWindow = true;
            aplupd.StartInfo.UseShellExecute = false;
            aplupd.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            aplupd.EnableRaisingEvents = true;

            aplupd.Start();

            await Task.Run(() => aplupd.WaitForExit());

            aplupd.Close();

            stopbar();

            statuslabel.Content = "";

            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\\tmp", true);
            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\\upd", true);

            onbtn();

            System.Windows.MessageBox.Show("Update applyment finished.\nYou can now use your updated romFS via fs-mitm.");
        }      
    }
}
 