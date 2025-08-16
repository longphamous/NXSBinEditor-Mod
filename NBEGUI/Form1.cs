using NXSBinEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace NBEGUI {
    public partial class NBEGUI : Form {
        public NBEGUI() {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\n' || e.KeyChar == '\r') {
                try {
                    listBox1.Items[listBox1.SelectedIndex] = textBox1.Text;
                } catch { }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                textBox1.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
            } catch { }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            isAppending = false;
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowDialog();
        }

        private void AppendMultiple_Click(object sender, EventArgs e)
        {
            isAppending = true;
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            saveFileDialog1.ShowDialog();
        }

        BinHelper Editor;
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            if (!isAppending)
            {
                listBox1.Items.Clear();
                fileNamesPerLine.Clear();
            }

            foreach (string file in openFileDialog1.FileNames)
            {
                Editor = new BinHelper(File.ReadAllBytes(file));
                foreach (string str in Editor.Import())
                {
                    listBox1.Items.Add(str);
                    fileNamesPerLine.Add(Path.GetFileName(file));
                }
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) { 
            List<string> Rst = new List<string>();
            foreach (string str in listBox1.Items)
                Rst.Add(str);

            File.WriteAllBytes(saveFileDialog1.FileName, Editor.Export(Rst.ToArray()));

            MessageBox.Show("Saved");
        }

        private void ExportList_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text files (*.txt)|*.txt";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                    {
                        foreach (var item in listBox1.Items)
                        {
                            writer.WriteLine(item.ToString());
                        }
                    }
                }
            }
        }

        private void ExportCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV files (*.csv)|*.csv";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                    {
                        writer.WriteLine("ja_JP|en_US|comment");

                        for (int i = 0; i < listBox1.Items.Count; i++)
                        {
                            string ja_JP = $"\"{listBox1.Items[i].ToString().Replace("\"", "\"\"")}\"";
                            string en_US = "\"\"";
                            string comment = (i < fileNamesPerLine.Count) ? $"\"{fileNamesPerLine[i].Replace("\"", "\"\"")}\"" : "\"\"";
                            writer.WriteLine($"{ja_JP}|{en_US}|{comment}");
                        }
                    }
                }
            }
        }
    }
}
