using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;


namespace monopoly
{

    public partial class MainWindow
    {
        private static int _count = 0;

        private static List<Border> Kastid = new List<Border>();
        private static List<Plot> Plots = new List<Plot>();
        private static Dictionary<int, long> Sides = new Dictionary<int, long> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 } };

        private static string[] lühinimed = new[] {
            "GO", "A1", "CC1", "A2", "T1", "R1", "B1", "CH1", "B2", "B3",
            "JAIL", "C1", "U1", "C2", "C3", "R2", "D1", "CC2", "D2", "D3",
            "FP", "E1", "CH2", "E2", "E3", "R3", "F1", "F2", "U2", "F3",
            "G2J", "G1", "G2", "CC3", "G3", "R4", "CH3", "H1", "T2", "H2" };

        private static int asukoht = 0;
        private List<bool> lastrolls = new List<bool> { false, false, false };
        private int pealminekaartCC = 0;
        private int pealminekaartCH = 0;
        private int[] cckaardid = new[] { 0, 10 };
        private int[] chkaardid = new[] { 0, 10, 11, 24, 39, 5 };
        private List<int> rolls;

        public MainWindow()
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 0);
            dispatcherTimer.Start();
            DataContext = this;
            _count = 0;

            InitializeComponent();
            CreatePlotList(mGrid);
            PaintBox(asukoht);
            Plots[asukoht].Stops++;
        }

        private static void CreatePlotList(Grid mgrid)
        {
            Kastid.AddRange(mgrid.Children.OfType<Border>().ToArray());
            foreach (var sirge in mgrid.Children.OfType<Grid>())
            {
                foreach (Border kast in sirge.Children)
                {
                    Kastid.Add(kast);
                }
            }
            foreach (var kast in Kastid)
            {
                kast.BorderBrush = Brushes.DeepPink;
                kast.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            Kastid = Kastid.OrderBy(k => k.Name).ToList();

            for (var index = 0; index < Kastid.Count; index++)
            {
                var k = Kastid[index];
                Plots.Add(new Plot
                {
                    koht = index,
                    Stops = 0,
                    nimi = lühinimed[index],
                    gElement = Kastid[index]
                });
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
           // PaintBox(asukoht);
            var a1 = asukoht;

            asukoht = KalkAsukoht(asukoht);

            var a2 = asukoht;

            //Line(Kastid[a1],Kastid[a2]);
           // PaintBox(asukoht);
            Plots[asukoht].Stops++;
            _count++;

            if (_count % 10000 == 0)
            {
            fuk.Text = _count.ToString();
            CCnr.Text = pealminekaartCC.ToString();
            CHnr.Text = pealminekaartCH.ToString();
            d1.Text = rolls[0].ToString();
            d2.Text = rolls[1].ToString();
            Stats();
            }
        }

        private void Stats()
        {
            var string1 = "";
            var string2 = "";
            var s = Plots.OrderByDescending(p => p.Stops).ToList();
            foreach (var vs in s)
            {
                string1 += vs.koht + ":" + vs.nimi + "\n";
                string2 += vs.Stops + "\n";
            }
            foreach (var side in Sides.Keys)
            {
                string1 += side + " : \n";
                string2 += (Sides[side]*50)/_count + "\n";
            }
            Stats1.Text = string1;
            Stats2.Text = string2;
        }

        private int KalkAsukoht(int asukoht)
        {
            var rand = new Random();

            rolls = new List<int> { rand.Next(6) + 1, rand.Next(6) + 1 };
            rolls.ForEach(x => Sides[x]++);


            lastrolls.Add(rolls.All(x => x == rolls[0]));
            lastrolls.RemoveAt(0);

            //Järgmine veeretatud koht
            var potentskoht = (asukoht + rolls.Sum(r => r)) % 40;

            //Vangi kui 3 korda sama visanud
            if (lastrolls.All(b => b))
                potentskoht = 10;

            //JAIL
            if (potentskoht == 30)
                potentskoht = 10;

            //Chance
            if (potentskoht == 7 || potentskoht == 22 || potentskoht == 36)
            {
                /*
                Advance to GO
                Go to JAIL
                Go to C1
                Go to E3
                Go to H2
                Go to R1
                */
                if (pealminekaartCH < 6)
                    potentskoht = chkaardid[pealminekaartCH];

                /*
                Go to next R (railway company)
                Go to next R    
                 */
                if (pealminekaartCH == 6 || pealminekaartCH == 7)
                {
                    if (potentskoht == 7) potentskoht = 15;
                    if (potentskoht == 22) potentskoht = 25;
                    if (potentskoht == 36) potentskoht = 5;
                }

                //Go to next U (utility company)
                if (pealminekaartCH == 8)
                    potentskoht = asukoht == 22 ? 28 : 12;

                if (pealminekaartCH == 9)
                    potentskoht -= 3;

                pealminekaartCH = ++pealminekaartCH % 16;
            }

            //Community Chest
            if (potentskoht == 2 || potentskoht == 17 || potentskoht == 33)
            {
                if (pealminekaartCC < 2)
                {
                    potentskoht = cckaardid[pealminekaartCC];
                }
                pealminekaartCC = ++pealminekaartCC % 16;
            }

            if (potentskoht == 10)
            {
                lastrolls[0] = false;
                lastrolls[1] = false;
                lastrolls[2] = false;
            }
            return potentskoht;
        }

        private void Line(Border startB, Border endB)
        {
            if (Kanvas.Children.Count != 0)
                Kanvas.Children.Remove(Kanvas.Children[0]);

            var btn1Point = startB.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
            var btn2Point = endB.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
            Line l1 = new Line
            {
                Stroke = new SolidColorBrush(Colors.DeepPink),
                StrokeThickness = 4.0,
                X1 = btn1Point.X + startB.ActualWidth / 2,
                X2 = btn2Point.X + endB.ActualWidth / 2,
                Y1 = btn1Point.Y + startB.ActualHeight / 2,
                Y2 = btn2Point.Y + endB.ActualHeight / 2,
                Uid = "Linex"
            };
            Kanvas.Children.Add(l1);
        }
        private void PaintBox(int nr)
        {
            ImageBrush imgBrush = new ImageBrush
            {
                ImageSource =
                    new BitmapImage(new Uri(
                        @"https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/14494864_1214873718570661_3454889043315582664_n.jpg?oh=c18f2917e1a06b1df5b7ca75992a1436&oe=59BFBFA3"))
            };


            var ell = new Ellipse
            {
                Width = 40,
                Height = 40,
                Stroke = Brushes.DeepPink,
                StrokeThickness = 2,
                Fill = imgBrush

            };

            var ell2 = new Ellipse
            {
                Width = 40,
                Height = 40,
                Stroke = Brushes.DeepPink,
                StrokeThickness = 2,
                Fill = Brushes.DeepPink

            };

            var kanv = new Canvas
            {

                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 0,
                Height = 0,
            };
            kanv.Children.Add(ell2);
            kanv.Children.Add(ell);
            Canvas.SetLeft(ell2, -20);
            Canvas.SetTop(ell2, -20);
            Canvas.SetLeft(ell, -20);
            Canvas.SetTop(ell, -20);
            if (Plots[nr].gElement.Child != null)
            {
                Plots[nr].gElement.Child = null;
                Plots[nr].gElement.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                Plots[nr].gElement.Child = kanv;
                Plots[nr].gElement.BorderThickness = new Thickness(4, 4, 4, 4);
            }
        }
    }
}
