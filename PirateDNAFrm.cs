using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace Pirate_DNA
{
    public partial class PirateDNAFrm : Form
    {
        ArrayList match_segments = new ArrayList();
        string match1 = null;
        string match2 = null;

        ArrayList ln = null;
        ArrayList chr = null;
        ArrayList pos = null;

        string[] files_to_combine = null;

        string autosomal_filename = null;
        StringBuilder sb = new StringBuilder();

        public PirateDNAFrm()
        {
            InitializeComponent();
        }

        

        private bool isPositionValid(string position, string chromosome)
        {
            if (chromosome == "X" || chromosome == "Y" || chromosome == "XY" || chromosome == "MT")
                return false;

            int p = int.Parse((string)position);
            int c = int.Parse((string)chromosome);           
            int c_tmp = -1;
            int p_start_tmp = -1;
            int p_end_tmp = -1;
            foreach (string[] segment in match_segments)
            {
                c_tmp = int.Parse(segment[0]);
                p_start_tmp = int.Parse(segment[1]);
                p_end_tmp = int.Parse(segment[2]);

                if (p_start_tmp <= p && p <= p_end_tmp && c_tmp == c)
                {
                    return true;
                }
            }
            return false;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            StreamWriter sw = new StreamWriter(autosomal_filename);
            sw.WriteLine("RSID,CHROMOSOME,POSITION,RESULT");
            for (int i = 0; i < ln.Count; i++)
            {
                if (isPositionValid((string)pos[i], (string)chr[i]))
                {
                    sw.WriteLine(ln[i]);
                }
            }
            sw.Close();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            statusLbl.Text = "Done.";
            statusLbl.Visible = false;
            MessageBox.Show("File successfully saved.");
            tabControl1.SelectedIndex = tabControl1.SelectedIndex + 1;
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string[] data = null;
                match1 = null;
                match2 = null;
                if (openFileDialog1.FileName.EndsWith(".csv"))
                {
                    // ftdna
                    string[] lines = File.ReadAllLines(openFileDialog1.FileName);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("NAME,MATCHNAME"))
                            continue;
                        data = line.Split(",".ToCharArray());
                        if (match1 == null || match2 == null)
                        {
                            match1 = data[0];
                            match2 = data[1];
                        }
                        match_segments.Add(new string[] { data[2], data[3], data[4] });
                    }
                }
                else if (openFileDialog1.FileName.EndsWith(".html") || openFileDialog1.FileName.EndsWith(".htm"))
                {
                    string page = File.ReadAllText(openFileDialog1.FileName);

                    string match_kits = page.Substring(page.IndexOf("Comparing Kit "), 150);
                    int loc = page.IndexOf("<tr><td align=center>");
                    int len = page.IndexOf("</table>", loc);
                    page = page.Substring(loc, len - loc);
                    page = page.Replace("<tr><td align=center>", "");
                    page = page.Replace("</td><td align=center>", "|");
                    page = page.Replace("</td></tr>", "\r\n");
                    page = page.Replace(",", "");
                    page = page.Replace("|", ",");

                    StringReader br = new StringReader(page);
                    string line = null;
                    match1 = match_kits.Substring("Comparing Kit ".Length, match_kits.IndexOf("and") - "Comparing Kit ".Length).Trim();
                    int start_idx = match_kits.IndexOf(" and ") + " and ".Length;
                    match2 = match_kits.Substring(start_idx, match_kits.IndexOf(")", start_idx) - start_idx).Trim();
                    while ((line = br.ReadLine()) != null)
                    {
                        if (line.Trim() == "")
                            continue;
                        data = line.Split(",".ToCharArray());
                        match_segments.Add(new string[] { data[0], data[1], data[2] });
                    }
                    br.Close();
                }
                comboBox1.Enabled = true;
                button2.Enabled = true;
                comboBox1.Items.Clear();
                comboBox1.Items.Add(match1);
                comboBox1.Items.Add(match2);
                comboBox1.SelectedIndex = 0;
                tabControl1.SelectedIndex = tabControl1.SelectedIndex+1;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label11.Text = (string)comboBox1.Items[(comboBox1.SelectedIndex + 1) % 2];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog(this) == DialogResult.OK)
            {
                string tmp = null;
                string[] lines = File.ReadAllLines(openFileDialog2.FileName);
                string[] data = null;
                ln = new ArrayList();
                chr = new ArrayList();
                pos = new ArrayList();
                foreach (string line in lines)
                {
                    if (line.StartsWith("RSID") || line.StartsWith("#"))
                        continue;
                    tmp = line.Replace("\"", "");
                    if (tmp.IndexOf(",") != -1)
                        data = tmp.Split(",".ToCharArray());
                    else
                        data = tmp.Split("\t".ToCharArray());
                    ln.Add("\"" + data[0] + "\",\"" + data[1] + "\",\"" + data[2] + "\",\"" + data[3] + "\"");
                    chr.Add(data[1]);
                    pos.Add(data[2]);
                }
                button3.Enabled = true;
                tabControl1.SelectedIndex = tabControl1.SelectedIndex + 1;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                statusLbl.Visible = true;
                statusLbl.Text = "Working... (this will take several minutes)";
                autosomal_filename = saveFileDialog1.FileName;
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            comboBox1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            //
            if (match_segments == null)
                match_segments = new ArrayList();
            else
                match_segments.Clear();
            match1 = null;
            match2 = null;

            ln = null;
            chr = null;
            pos = null;
            tabControl1.SelectedIndex = 0;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Extracts DNA from matches to recontruct (reverse engineer) DNA profile.\r\n\r\nDeveloper : Felix Jeyareuben <i@fc.id.au>\r\nWebsite : y-str.org", "Pirate DNA Extractor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.y-str.org/");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = tabControl1.SelectedIndex + 1;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (openFileDialog3.ShowDialog(this) == DialogResult.OK)
            {
                files_to_combine = openFileDialog3.FileNames;
                if (files_to_combine.Length == 1)
                {
                    MessageBox.Show("You had just selected one file. Please select multiple files by holding the control key", "Single selection warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                statusLbl.Visible = true;
                statusLbl.Text = "Working (this may take several minutes) ...";
                backgroundWorker2.RunWorkerAsync();
            }
        }

        private string getSNP(ArrayList list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string snp in list)
                sb.Append(snp);
            string snp_list = sb.ToString();
            int[] allele = new int[4];
            //ATGC order
            string[] allele_str = new string[] { "A", "T", "G", "C" };
            allele[0]= snp_list.Count(f => f == 'A');
            allele[1] = snp_list.Count(f => f == 'T');
            allele[2] = snp_list.Count(f => f == 'G');
            allele[3] = snp_list.Count(f => f == 'C');

            string max = "";
            int max_idx = -1;
            int max_count=0;
            for (int i = 0; i < allele.Length;i++)
            {
                if (allele[i] > max_count)
                {
                    max_count = allele[i];
                    max_idx = i;
                    max = allele_str[i];
                }                
            }
            //
            string max2 = "";
            int max2_count = 0;
            for (int i = 0; i < allele.Length; i++)
            {
                if (i == max_idx)
                    continue;
                if (allele[i] > max2_count)
                {
                    max2_count = allele[i];
                    max2 = allele_str[i];
                }
            }

            return max + max2;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            //
            ArrayList[] combined_rsid = new ArrayList[22];
            ArrayList[] combined_pos = new ArrayList[22];
            ArrayList[] combined_snp = new ArrayList[22];
            string[] lines = null;
            string[] row_data = null;
            string tmp = null;
            int chr = 0;
            ArrayList snp = new ArrayList();
            int c = 0;

            for (int i = 0; i < 22; i++)
            {
                    combined_rsid[i] = new ArrayList();
                    combined_pos[i] = new ArrayList();
                    combined_snp[i] = new ArrayList();
            }

            foreach (string file in files_to_combine)
            {
                lines = File.ReadAllLines(file);
                c = 0;
                int idx = 0;
                foreach (string line in lines)
                {
                    c++;
                    if (line.StartsWith("RSID") || line.StartsWith("#"))
                        continue;
                    //                   

                    tmp = line.Replace("\"", "");
                    if(tmp.IndexOf(",")!=-1)
                        row_data = tmp.Split(",".ToCharArray());
                    else
                        row_data = tmp.Split("\t".ToCharArray());
                    try
                    {
                        chr = Int32.Parse(row_data[1]);
                        backgroundWorker2.ReportProgress((c * 100 / lines.Length), file);                        
                    }
                    catch (Exception)
                    {
                        continue;
                    }                    
                    
                    if (combined_rsid[chr - 1].Contains(row_data[0]))
                    {
                        //contains...
                        idx = combined_rsid[chr - 1].IndexOf(row_data[0]);
                        snp = (ArrayList)combined_snp[chr - 1][idx];
                        snp.Add(row_data[3]);
                        combined_snp[chr - 1][idx] = snp;
                    }
                    else
                    {
                        combined_rsid[chr - 1].Add(row_data[0]);
                        combined_pos[chr - 1].Add(row_data[2]);
                        snp = new ArrayList();
                        snp.Add(row_data[3]);
                        combined_snp[chr - 1].Add(snp);
                    }
                }
            }
            //

            ArrayList l = null;
            string correct_snp = null;
            sb.Clear();
            sb.Append("RSID,CHROMOSOME,POSITION,RESULT\r\n");
            for (int i = 0; i < 22; i++)
            {               
                for (int j = 0; j < combined_rsid[i].Count; j++)
                {
                    l = (ArrayList)combined_snp[i][j];
                    if (l.Count == 1)
                        correct_snp = l[0].ToString();
                    else                    
                        correct_snp = getSNP(l);                       
                    sb.Append("\"" + combined_rsid[i][j] + "\",\"" + (i + 1) + "\",\"" + combined_pos[i][j] + "\",\"" + correct_snp + "\"\r\n");
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName,sb.ToString());
                MessageBox.Show("File successfully saved!");
                tabControl1.SelectedIndex = tabControl1.SelectedIndex + 1;
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            statusLbl.Text = "Done";
            statusLbl.Visible = false;
            tabControl1.SelectedIndex = tabControl1.SelectedIndex + 1;
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusLbl.Text = e.UserState+" - "+e.ProgressPercentage.ToString() + "% completed";
        }
    }
}
