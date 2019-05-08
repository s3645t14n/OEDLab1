using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MathNet.Numerics.Distributions;

namespace OEDLab1
{
    public partial class Form1 : Form
    {
        //вычисление коэффициента Пирсона
        public static double calculatechi(int[] Y, int quan, int cols)
        {
            double chisquare = 0;
            for (int i = 0; i < cols; i++)
                chisquare += (Math.Pow((Y[i] - quan / cols), 2)) / (double)(quan / cols);
            return chisquare;
        }

        //вычисление коэффициента автокорреляции
        public static double correlationCoefficient(double[] X, double[] Y, double n)
        {
            double sum_X = 0, sum_Y = 0, sum_XY = 0;
            double squareSum_X = 0, squareSum_Y = 0;

            for (int i = 0; i < n; i++)
            {
                sum_X = sum_X + X[i];
                sum_Y = sum_Y + Y[i];
                sum_XY = sum_XY + X[i] * Y[i];
                squareSum_X = squareSum_X + X[i] * X[i];
                squareSum_Y = squareSum_Y + Y[i] * Y[i];
            }

            double corr = (double)(n * sum_XY - sum_X * sum_Y) /
                         (double)(Math.Sqrt((n * squareSum_X -
                         sum_X * sum_X) * (n * squareSum_Y -
                         sum_Y * sum_Y)));

            return corr;
        }

        //начальные моменты
        public static double smoment(double[] m1, int n, int k)
        {
            double sum = 0;
            for (int i = 0; i < n; i++)
                sum += Math.Pow(m1[i], k) * (1.0 / n);
            return sum;
        }

        //центральные моменты
        public static double cmoment(double[] m1, int n, int k)
        {
            double sum = 0, M = 0;
            for (int i = 0; i < n; i++)
                M += m1[i] * (1.0 / n);
            for (int i = 0; i < n; i++)
                sum += Math.Pow((m1[i] - M), k) * (1.0 / n);
            return sum;
        }

        public class Point
        {
            public double X { get; private set; }
            public double Y { get; private set; }

            public Point(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        public double[] m1;
        public double rndv;
        public double min, max;
        public int quantity;

        public Form1()
        {
            this.KeyPreview = true;
            this.KeyPress += new KeyPressEventHandler(g_KeyDown);
            this.KeyPress += new KeyPressEventHandler(c_KeyDown);
            this.KeyPress += new KeyPressEventHandler(f_KeyDown);
            //button5.Click += button1_Click;
            //button6.Click += button2_Click;
            //openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            textBox1.Text = "10";//кратность
            textBox2.Text = "10";//разряды гистограммы
            textBox3.Text = "0";//мин
            textBox4.Text = "1";//макс
            textBox5.Text = "2";//шаг корреляции
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndex = comboBox1.FindStringExact("равномерное");
            button2.Enabled = false;

            //предварительная обработка гистограмм
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 10;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
        }

        //сгенерировать выборку
        private void button1_Click(object sender, EventArgs e)
        {
            if (button2.Enabled == false) button2.Enabled = true;
            Random rand = new Random();
            int n;

            //получение типа распределения
            string selected = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);

            Int32.TryParse(textBox1.Text, out n);
            Double.TryParse(textBox3.Text, out min);
            Double.TryParse(textBox4.Text, out max);
            quantity = 10 * n;
            m1 = new double[quantity];

            switch (selected) {
                case "равномерное":
                    for (int i = 0; i < quantity; i++)
                        m1[i] = rand.NextDouble() * (max - min) + min;

                    break;

                case "нормальное": //методом Бокса-Мюллера
                    double mean = min;
                    double stdDev = max; //стандартное отклонение

                    for (int i = 0; i < quantity; i++)
                    {
                        //double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
                        //double u2 = 1.0 - rand.NextDouble();
                        //double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                        //m1[i] = randStdNormal;// mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
                        MathNet.Numerics.Distributions.Normal normalDist = new Normal(mean, stdDev);
                        m1[i] = normalDist.Sample();
                    }
                    break;
            }
            toTable();
        }

        //загрузка из файла
        private void button5_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            if (button2.Enabled == false) button2.Enabled = true;
            // получаем выбранный файл
            string filename = openFileDialog1.FileName;
            // читаем файл в строку

            string fileText = System.IO.File.ReadAllText(filename);
            //textBox8.Text = fileText;

            string[] split = fileText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            m1 = new double[split.Length];
            for (int i = 0; i < split.Length; i++)
                m1[i] = Double.Parse(split[i].Trim(), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);

            //m1 = fileText.Split(',').Select(double.Parse).ToArray();
            //m1 = fileText.Split(',').Select(double.Parse).ToArray();

            //quantity = 10 * n;
            //m1 = new double[quantity];

            MessageBox.Show("Файл открыт");
            toTable();
        }

        //привязкак таблице
        private void toTable()
        {
            //получение мин/макс и колва для диаграммы
            min = m1.Min();
            max = m1.Max();
            quantity = m1.Length;
            toolStripStatusLabel8.Text = Math.Round(min, 5).ToString();
            toolStripStatusLabel10.Text = Math.Round(max, 5).ToString();
            toolStripStatusLabel2.Text = quantity.ToString();

            DataTable dt1 = new DataTable();
            for (int j = 0; j < 10; j++)
                dt1.Columns.Add(j.ToString());
            for (int i = 0; i < quantity/10; i++)
            {
                DataRow r1 = dt1.NewRow();
                for (int j = 0; j < 10; j++)
                    r1[j] = m1[10 * i + j];
                dt1.Rows.Add(r1);
            }
            dataGridView1.DataSource = dt1;

            //обработка внешнего вида таблицы
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            foreach (DataGridViewColumn col in dataGridView1.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            //обработка строки состояния
            toolStripStatusLabel2.Text = quantity.ToString();
            toolStripStatusLabel4.Text = Math.Round((m1.Sum() / quantity), 5).ToString();
            double disp = 0;
            for (int i = 0; i < quantity; i++)
                disp += (m1[i] - (m1.Sum() / quantity)) * (m1[i] - (m1.Sum() / quantity));
            disp /= (quantity - 1);
            toolStripStatusLabel6.Text = Math.Round(disp, 5).ToString();

            Invalidate();
        }
        
        //рассчитать разрядность по формуле Стерджесса
        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = (1 + (int)(Math.Log(quantity, 2))).ToString();//разряды гистограммы
        }

        //рассчитать гистограмму
        private void button2_Click(object sender, EventArgs e)
        {
            int cols;
            Int32.TryParse(textBox2.Text, out cols);

            chart1.Series.Clear();
            chart1.Series.Add("1Dhist");
            chart1.Series["1Dhist"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            chart1.Series.Add("1Dspline");
            chart1.Series["1Dspline"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.Series["1Dspline"].Color = Color.Red;
            if (cols > 20) chart1.Series["1Dspline"].BorderWidth = 1;
            else chart1.Series["1Dspline"].BorderWidth = 2;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = cols + 1;
            chart1.ChartAreas[0].AxisY.Minimum = 0;

            int[] ydata = new int[cols + 1];
            for (int i = 0; i < quantity; i++)
                ydata[(int)((m1[i] - min) / ((max - min) / cols))]++;//формула

            List<Point> mydata = new List<Point>();
            for (int x = 0; x < cols; x++)
                mydata.Add(new Point(x + 1, ydata[x]));

            for (int x = 0; x < cols; x++)
                chart1.Series["1Dhist"].Points.DataBindXY(mydata, "X", mydata, "Y");
            for (int x = 0; x < cols; x++)
                chart1.Series["1Dspline"].Points.DataBindXY(mydata, "X", mydata, "Y");

            //вычисление хи квадрат для равномерного распределения
            double chi2 = calculatechi(ydata, quantity, cols);
            label8.Text = Math.Round(chi2, 3).ToString();

            //оценка равномерности по Пирсону
            Double chitab;
            Double.TryParse(textBox6.Text.ToString(), out chitab);
            if (chitab != 0)
                if (chitab > chi2)
                {
                    label16.ForeColor = Color.Green;
                    label16.Text = "равномерно";
                }
                else
                {
                    label16.ForeColor = Color.Red;
                    label16.Text = "не равномерно";
                }
            else
            {
                label16.ForeColor = Color.Black;
                label16.Text = "n/a";
            }

            //коэффициент автокорреляции
            int step;
            Int32.TryParse(textBox5.Text, out step);
            double[] X = new double[quantity - 1];
            double[] Y = new double[quantity - 1];
            for (int i = 0; i < quantity - step; i++)
            {
                X[i] = m1[i];
                Y[i] = m1[i + step];
            }
            double r = correlationCoefficient(X, Y, quantity / 2);
            label10.Text = Math.Round(r, 4).ToString();

            //оценка критерия АК по Чеддоку
            if (Math.Abs(r) >= 0 && Math.Abs(r) <= 0.3)
            {
                label12.ForeColor = Color.Green;
                label12.Text = "слабая корреляция";
            }
            else if (Math.Abs(r) > 0.3 && Math.Abs(r) <= 0.5)
            {
                label12.ForeColor = Color.Blue;
                label12.Text = "умеренная корреляция";
            }
            else if (Math.Abs(r) > 0.5 && Math.Abs(r) <= 0.7)
            {
                label12.ForeColor = Color.Red;
                label12.Text = "заметная корреляция";
            }
            else if (Math.Abs(r) > 0.7 && Math.Abs(r) <= 0.9)
            {
                label12.ForeColor = Color.Red;
                label12.Text = "высокая корреляция";
            }
            else if (Math.Abs(r) > 0.9)
            {
                label12.ForeColor = Color.Red;
                label12.Text = "крайне высокая";
            }

            //начальные и центральные моменты
            label24.Text = Math.Round(smoment(m1, quantity, 1), 5).ToString();
            label25.Text = Math.Round(smoment(m1, quantity, 2), 5).ToString();
            label26.Text = Math.Round(cmoment(m1, quantity, 1), 5).ToString();
            label27.Text = Math.Round(cmoment(m1, quantity, 2), 5).ToString();
            label28.Text = Math.Round(cmoment(m1, quantity, 3), 5).ToString();
            label29.Text = Math.Round(cmoment(m1, quantity, 4), 5).ToString();
        }

        //Сгенерировать и рассчитать
        private void button4_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
            button2_Click(sender, e);
        }

        ////////////горячие клавиши///////////
        private void g_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'g')
                button1_Click(sender, e);
        }

        private void c_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (button2.Enabled == true)
                if (e.KeyChar == 'c')
                    button2_Click(sender, e);
        }

        private void f_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'f')
            {
                button1_Click(sender, e);
                button2_Click(sender, e);
            }
        }
    }
}
