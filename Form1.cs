using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kdrs_dcm_log_read
{
    public partial class Form1 : Form
    {
        string fileName = String.Empty;
        string fileFolder = String.Empty;
        string outFile = String.Empty;

        int seqCounter = 0;

        public Form1()
        {
            InitializeComponent();

            this.AllowDrop = true;
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            fileName = files[0];

            fileFolder = Path.GetDirectoryName(fileName);

            outFile = Path.Combine(fileFolder, "dcm_operations.txt");

            txtBoxInfo.Text = fileName;


            if (files.Count() > 1)
                MessageBox.Show("One file at the time");
            else
                readDcmLog();
        }

        private void readDcmLog()
        {
            Dictionary<string, int> seqCount = new Dictionary<string, int>();
            List<string> description = new List<string>();
            using (StreamReader reader = new StreamReader(fileName))
            {
                string start = "126";
                string sequence = String.Empty;
                string line;
                int lineCount = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lineCount++;
                    string[] splitArray = { "BLOBConversionProcess:", " - " };
                    string[] splitArrayDesc = { "BLOBConversionProcess:"};

                    if (line.Contains("BLOBConversionProcess:126"))
                    {
                        if (String.IsNullOrEmpty(sequence))
                            sequence = start;
                        else if (seqCount.ContainsKey(sequence))
                        {
                            seqCounter++;

                            Console.WriteLine(seqCounter);
                            try
                            {
                                seqCount[sequence]++;
                            }catch (Exception ex)
                            {
                                Console.WriteLine("Unable to add sequence value");
                                Console.WriteLine(sequence);
                                break;
                            }
                            txtBoxLog.Text = seqCounter.ToString();
                        }
                        else
                        {
                            seqCounter++;
                            txtBoxLog.Text = seqCounter.ToString();
                            Console.WriteLine(seqCounter);
                            try
                            {
                                seqCount.Add(sequence, 1);

                            }catch(Exception ex)
                            {
                                Console.WriteLine("Unable to add sequence");
                                Console.WriteLine(sequence);
                                break;
                            }
                        }

                        sequence = start;

                       // description.Add("126 - Starting conversion of blob");
                    }

                    if (!line.Contains("BLOBConversionProcess") || !line.Contains("INFO") || String.IsNullOrEmpty(sequence))
                        continue;

                    


                    if (!line.Contains("BLOBConversionProcess:126"))
                    {
                        try
                        {
                            sequence = sequence + "-" + line.Split(splitArray, 3, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                        }
                        catch
                        {
                            Console.WriteLine("Unable to add to sequence");
                            Console.WriteLine(sequence);
                            Console.WriteLine("Line: " + lineCount);
                            
                            break;

                        }
                        // Console.WriteLine("Sequence: " + sequence);
                    }

                    string tempSplit = line.Split(splitArrayDesc, 2, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                    if (tempSplit.Length > 80)
                        tempSplit = tempSplit.Substring(0, 80);

                    try
                    {
                        if (!description.Contains(tempSplit))
                            description.Add(tempSplit);
                    }
                    catch
                    {
                        Console.WriteLine("Unable to add to description");
                        Console.WriteLine(tempSplit);
                        break;
                    }


                }
            }
            description.Sort();

            using (StreamWriter writer = new StreamWriter(outFile))
            {
                try
                {
                    foreach (var entry in seqCount)
                    {
                        //Console.WriteLine(kay.Key + " occured " + kay.Value + " times");
                        writer.Write(entry.Key + " , has occured " + entry.Value + " times\r\n");
                    }
                }
                catch
                {
                    Console.WriteLine("Unable to write dictionary");

                }
                writer.Write("\r\n");

                try
                {
                    foreach (string s in description)
                        writer.Write(s + "\r\n");
                }
                catch
                {
                    Console.WriteLine("Unable to write descriptions");

                }
            }
        }
    }
}
