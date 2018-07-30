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
            extractnsp();
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
        }

        public void onbtn()
        {
            ipbutton.IsEnabled = true;
            ptchbutton.IsEnabled = true;
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
            }

            if (keylabel.Content.Equals("0"))
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
                fwlabel.Content = "1.0.0";
            }

            if (keylabel.Content.Equals("1"))
            {
                fwlabel.Content = "3.0.0";
            }

            if (keylabel.Content.Equals("2"))
            {
                fwlabel.Content = "3.0.1";
            }

            if (keylabel.Content.Equals("3"))
            {
                fwlabel.Content = "4.0.0";
            }

            if (keylabel.Content.Equals("4"))
            {
                fwlabel.Content = "5.0.0";
            }

            if (keylabel.Content.Equals("5"))
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

            statuslabel.Content = "";
            keylabel.Content = "";
            fwlabel.Content = "";

            stopbar();

            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"\\tmp", true);

            onbtn();

            System.Windows.MessageBox.Show("Congrats this NSP will now work on " + fwlabel.Content + "!");
        }

        private void upnspipbutton_Click(object sender, RoutedEventArgs e)
        {

            openFileDialog.Filter = "NSW NSP File|*.nsp";
            openFileDialog.Title = "Select a NSW NSP File";

            if (openFileDialog.ShowDialog() == true)
                upnspinputdisplay.Text = openFileDialog.FileName;
        }

        public async void extractncau()
        {
            offbtn();

            string updir = AppDomain.CurrentDomain.BaseDirectory + "\\upd";
            Directory.CreateDirectory(updir);

            statuslabel.Content = "Extracting NCA's...";

            string hctdir = AppDomain.CurrentDomain.BaseDirectory + "\\hactool.exe";
            string arg = @"-tpfs0 --pfs0dir=upd " + "\"" + inputdisplay.Text + "\"";

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
        }

        public void applyupdate()
        {
            string upddir = AppDomain.CurrentDomain.BaseDirectory + "\\upd";
            var di = new DirectoryInfo(upddir);
            var result = di.GetFiles().OrderByDescending(x => x.Length).Take(1).ToList();
            var larnca = di.GetFiles().OrderByDescending(x => x.Length).Take(1).Select(x => x.FullName).ToList();

            string bigbnca = String.Join(" ", larnca);


            string nspudir = AppDomain.CurrentDomain.BaseDirectory + "\\hactool.exe";
            string arg = @"-k keys.txt﻿﻿﻿ --titlekey=﻿" + titlkyinput.Text + " --basenca=" + inputdisplay.Text + " --section1=romfs.bin --exefsdir=exefs" + bigbnca;

            System.Windows.MessageBox.Show("Arguments are:" + arg);

            Process nsu = new Process();
            nsu.StartInfo.FileName = nspudir;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            applyupdate();
        }
    }
}
 