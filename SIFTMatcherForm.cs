using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace sift_image_analyzer
{
    //KeyPoint class
    public struct SiftFrame
    {
        public float x;
        public float y;
        public float scale;
        public float orientation;
        public SiftFrame(float x, float y, float scale, float orientation)
        {
            this.x = x;
            this.y = y;
            this.scale = scale;
            this.orientation = orientation;
        }
    }

    //Descriptor class
    public struct SiftDesc
    {
        public byte[] desc;
        public SiftDesc(byte[] desc)
        {
            this.desc = desc;
        }
    }
    
    public struct Idxs
    {
        public int i;
        public int j;
        public float dist;
        public float secdist1;
        public float secdist2;

        public Idxs(int i, int j, float dist, float secdist1, float secdist2)
        {
            this.i = i;
            this.j = j;
            this.dist = dist;
            this.secdist1 = secdist1;
            this.secdist2 = secdist2;
        }
    }
    public partial class SIFT_Matcher_Form : Form
    {

        List<SiftFrame> sift_frames = new List<SiftFrame>();
        List<SiftDesc> sift_desc = new List<SiftDesc>();

        List<SiftFrame> sift_frames2 = new List<SiftFrame>();
        List<SiftDesc> sift_desc2 = new List<SiftDesc>();


        List<Idxs> pairs = new List<Idxs>();
        int next_pair=0;

        Image pictureLeft;
        Image pictureRight;



        public SIFT_Matcher_Form()
        {
            InitializeComponent();
        }

        private void siftLeftReadButtonClick(object sender, EventArgs e)
        {
            DialogResult dr_sift = openFileDialog1.ShowDialog();
            if(dr_sift == DialogResult.OK)
            {
                sift_frames.Clear();
                sift_desc.Clear();
                StreamReader sift_reader = new StreamReader(openFileDialog1.FileName);
                while(!sift_reader.EndOfStream)
                {
                    string[] frame_params = sift_reader.ReadLine().Replace(".",",").Split(new char[] {' '});
                    if(frame_params.Length>=4+128)  //todo
                    {
                        float x = Single.Parse(frame_params[0]);
                        float y = Single.Parse(frame_params[1]);
                        float scale = Single.Parse(frame_params[2]);
                        float orient = Single.Parse(frame_params[3]);


                        sift_frames.Add(new SiftFrame(x, y, scale, orient));

                        byte[] v = new byte[128];
                        for (int i = 0; i < 128; i++)
                        {
                            v[i] = byte.Parse(frame_params[4 + i]);
                        }
                        sift_desc.Add(new SiftDesc(v));
         
                    }

                }
                sift_reader.Close();
                pictureBox1.Refresh();
            }
                
        }

        private void siftRightReadButtonClick(object sender, EventArgs e)
        {
            DialogResult dr_sift = openFileDialog1.ShowDialog();
            if (dr_sift == DialogResult.OK)
            {
                sift_frames2.Clear();
                sift_desc2.Clear();
                StreamReader sift_reader = new StreamReader(openFileDialog1.FileName);
                while (!sift_reader.EndOfStream)
                {
                    string[] frame_params = sift_reader.ReadLine().Replace(".", ",").Split(new char[] { ' ' });
                    if (frame_params.Length >= 4)  //todo
                    {

                        float x = Single.Parse(frame_params[0]);
                        float y = Single.Parse(frame_params[1]);
                        float scale = Single.Parse(frame_params[2]);
                        float orient = Single.Parse(frame_params[3]);


                        sift_frames2.Add(new SiftFrame(x, y, scale, orient));
                        
                        byte[] v = new byte[128];
                        for (int i = 0; i < 128; i++)
                        {
                            v[i] = byte.Parse(frame_params[4 + i]);
                        }
                        sift_desc2.Add(new SiftDesc(v));

                    }

                }
                sift_reader.Close();
                pictureBox1.Refresh();
            }

        }

        private void matchButtonClick(object sender, EventArgs e)
        {
            double best_to_secbest=0;
            try
            {
                best_to_secbest = Double.Parse(textBox1.Text);
            }
            catch (Exception ex)
            {
                textBox1.Text = ex.Message;
                return;
            }

            //rozmiary zbiorów deskryptorów dla obu obrazów

            int i = sift_desc.Count;
            int j = sift_desc2.Count;

            //czyszczenie poprzednich par
            pairs.Clear();
            
            //pomocnicza kolekcja do przechowywania par
            List<Idxs> pairs2 = new List<Idxs>();

            int best_distance_val;
            int best_distance_idx;

            int secbest_distance_val;
            int secbest_distance_idx;

            
            
            //TODO: liczenie odległości pomiędzy deskryptorami (może być każdy z każdym)

            int[,] odleglosc = new int[i, j];
            for (int k = 0; k < i; k++)
            {
                for (int l = 0; l < j; l++)
                {
                    odleglosc[k, l] = 0;
                    //a następnie wypełnić wartościami kwadratu odległości
                    for (int m = 0; m < 128; m++)
                    {
                        odleglosc[k, l] +=
                        (((int)sift_desc[k].desc[m]) - ((int)sift_desc2[l].desc[m])) *
                        (((int)sift_desc[k].desc[m]) - ((int)sift_desc2[l].desc[m]));
                    }
                }
            }
            
            //TODO: usuwanie w oparciu o kryterium ilorazowe
            Double prog = Double.Parse(textBox1.Text);
            Double ratioB, ratioA;
            Double varA, varB;
            int minA = int.MaxValue;
            int minB = int.MaxValue;
            int a = -1;
            int b = -1;

            for (int k = 0; k < i; k++)
            {
                minA = int.MaxValue;
                minB = int.MaxValue;
                a = -1;
                b = -1;
                for (int l = 0; l < j; l++)
                {
                    if (odleglosc[k, l] < minA)
                    {
                        minB = minA;
                        minA = odleglosc[k, l];
                        b = a;
                        a = l;
                    }

                    /*
                    varA = (double)odleglosc[k,l] / (double)odleglosc[l,k];
                    varB = (double)odleglosc[l,k] / (double)odleglosc[k,l];
                    ratioA = varA / varB;
                    if (ratioA > prog) continue;
                    ratioB = varB / varA;
                    if (ratioB > prog) continue;
                    //umieść to gdzieś
                    Idxs dic = new Idxs(k, l, (float)ratioA, (float)3.14, (float)5.109);
                    */

                }
                ratioA = ((double)minA / (double)minB);
                if (ratioA > prog)
                    continue;
                /*ratioB = ((double)minB / (double)minA);
                if (ratioB > prog)
                    continue;*/
                //umieść to gdzieś
                Idxs dic = new Idxs(k, a, minA, 0f, 0f);
                pairs.Add(dic);
            }

                
        }

        private void showNextPairButtonClick(object sender, EventArgs e)
        {
            if (pairs == null || pairs.Count==0) return;
            
            next_pair++;
            if (next_pair >= pairs.Count)
                next_pair = 0;

            SiftFrame sf1 = sift_frames[pairs[next_pair].i];
            SiftFrame sf2 = sift_frames2[pairs[next_pair].j];

            pictureBox1.Refresh();
            

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            if (pictureLeft != null && pictureRight!=null)
            {
                Pen pen = new Pen(Color.Black, 2);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                Size s = new Size(pictureLeft.Size.Width + pictureRight.Size.Width + 10, pictureLeft.Size.Height < pictureRight.Size.Height ? pictureRight.Size.Height : pictureLeft.Size.Height);
                pictureBox1.Size = s;
                e.Graphics.DrawImage((Bitmap)pictureLeft, 0, 0, pictureLeft.Size.Width, pictureLeft.Size.Height);
                e.Graphics.DrawImage((Bitmap)pictureRight, pictureLeft.Size.Width + 10, 0, pictureRight.Size.Width, pictureRight.Size.Height);
                
                foreach (Idxs ind in pairs)
                {
                    SiftFrame sf1 = sift_frames[ind.i];
                    SiftFrame sf2 = sift_frames2[ind.j];

                    e.Graphics.DrawLine(pen, sf1.x, sf1.y, sf2.x + pictureLeft.Size.Width + 10, sf2.y);
                }

                if (pairs.Count > next_pair)
                {
                    SiftFrame sf1 = sift_frames[pairs[next_pair].i];
                    SiftFrame sf2 = sift_frames2[pairs[next_pair].j];

                    e.Graphics.DrawLine(Pens.Red, sf1.x, sf1.y, sf2.x + pictureLeft.Size.Width + 10, sf2.y);

                }

                if (showBlobs.Checked)
                {
                    foreach (SiftFrame sf in sift_frames)
                    {

                        e.Graphics.DrawEllipse(Pens.Yellow, sf.x - sf.scale / 2, sf.y - sf.scale / 2, sf.scale, sf.scale);
                         //e.Graphics.DrawLine(Pens.Yellow, sf.x, sf.y, sf.x + (float)Math.Cos(sf.orientation) * (sf.scale), sf.y + (float)Math.Sin(sf.orientation) * (sf.scale));
                    }
                    foreach (SiftFrame sf in sift_frames2)
                    {
                        e.Graphics.DrawEllipse(Pens.Yellow, pictureLeft.Size.Width + 10 + sf.x - sf.scale / 2, sf.y - sf.scale / 2, sf.scale, sf.scale);
                    }
                }

            }

        }

        private void readRasterLeftClick(object sender, EventArgs e)
        {
            DialogResult dr_image = openFileDialog1.ShowDialog();

            if (dr_image == DialogResult.OK)
            {
                pictureLeft = Bitmap.FromStream(openFileDialog1.OpenFile());

                pictureBox1.Refresh();
            }

        }
        private void readRasterRightClick(object sender, EventArgs e)
        {
            DialogResult dr_image = openFileDialog1.ShowDialog();

            if (dr_image == DialogResult.OK)
            {
                pictureRight = Bitmap.FromStream(openFileDialog1.OpenFile());
                pictureBox1.Refresh();

            }
        }
        
        private void dumpPairs_Click(object sender, EventArgs e)
        {
            DialogResult dr_dump = saveDump.ShowDialog();

            if (DialogResult.OK == dr_dump)
            {
                TextWriter tw = new StreamWriter(saveDump.FileName);

                foreach (Idxs index in pairs)
                {
                    tw.WriteLine("" + sift_frames[index.i].x.ToString(CultureInfo.InvariantCulture) + " " + sift_frames[index.i].y.ToString(CultureInfo.InvariantCulture) + " "
                                    + sift_frames2[index.j].x.ToString(CultureInfo.InvariantCulture) + " " + sift_frames2[index.j].y.ToString(CultureInfo.InvariantCulture));
                }

                tw.Close();
            }
        }

        private void showBlobs_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
            
        }

    }
}
