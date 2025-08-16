using NXSBinEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        private void ImportTxt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog importDialog = new OpenFileDialog())
            {
                importDialog.Filter = "Text files (*.txt)|*.txt";
                importDialog.Multiselect = false;
                if (importDialog.ShowDialog() == DialogResult.OK)
                {
                    List<string> txtLines = new List<string>();
                    using (StreamReader reader = new StreamReader(importDialog.FileName))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            txtLines.Add(line);
                        }
                    }

                    // Check if the number of lines matches
                    if (txtLines.Count != listBox1.Items.Count)
                    {
                        MessageBox.Show($"The number of lines in the TXT file ({txtLines.Count}) does not match the number of entries in the ListBox ({listBox1.Items.Count}). Import aborted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Import lines from TXT if not empty
                    for (int i = 0; i < listBox1.Items.Count; i++)
                    {
                        string txtLine = txtLines[i];
                        if (!string.IsNullOrWhiteSpace(txtLine))
                        {
                            listBox1.Items[i] = txtLine;
                        }
                    }

                    // Update the TextBox if an item is selected
                    if (listBox1.SelectedIndex >= 0)
                    {
                        textBox1.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
                    }

                    MessageBox.Show("Import completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ImportCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog importDialog = new OpenFileDialog())
            {
                importDialog.Filter = "CSV files (*.csv)|*.csv";
                importDialog.Multiselect = false;
                if (importDialog.ShowDialog() == DialogResult.OK)
                {
                    List<string> enUsValues = new List<string>();
                    bool isHeaderSkipped = false;
                    using (StreamReader reader = new StreamReader(importDialog.FileName))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!isHeaderSkipped)
                            {
                                isHeaderSkipped = true;
                                continue; 
                            }

                            string[] parts = ParseCsvLine(line);
                            if (parts.Length >= 3)
                            {
                                string en_US = parts[1].Trim('"');
                                enUsValues.Add(en_US);
                            }
                        }
                    }

                    if (enUsValues.Count != listBox1.Items.Count)
                    {
                        MessageBox.Show($"The number of lines in the CSV file ({enUsValues.Count}) does not match the number of entries in the ListBox ({listBox1.Items.Count}). Import canceled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    for (int i = 0; i < listBox1.Items.Count; i++)
                    {
                        string en_US = enUsValues[i];
                        if (!string.IsNullOrWhiteSpace(en_US))
                        {
                            listBox1.Items[i] = en_US;
                        }
                    }

                    if (listBox1.SelectedIndex >= 0)
                    {
                        textBox1.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
                    }

                    MessageBox.Show("Import completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private string[] ParseCsvLine(string line)
        {
            List<string> parts = new List<string>();
            bool inQuotes = false;
            string currentPart = "";
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"' && !inQuotes)
                {
                    inQuotes = true;
                }
                else if (c == '"' && inQuotes)
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentPart += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else if (c == '|' && !inQuotes)
                {
                    parts.Add(currentPart);
                    currentPart = "";
                }
                else
                {
                    currentPart += c;
                }
            }
            parts.Add(currentPart);
            return parts.ToArray();
        }
    }
}
