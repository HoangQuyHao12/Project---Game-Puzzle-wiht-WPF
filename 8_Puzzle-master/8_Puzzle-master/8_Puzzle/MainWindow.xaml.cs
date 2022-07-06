using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace _8_Puzzle
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Step counter
        private int step = 0;
        public int Step
        {
            get => step; set
            {
                step = value;
                OnPropertyChanged("Step");
            }
        }
        #endregion
        #region Time counter
        Timer _timer = new Timer(1000);
        private string timercolor = "Black";
        public string timerColor
        {
            get => timercolor;
            set
            {
                timercolor = value;
                OnPropertyChanged("timerColor");
            }
        }
        public int CountTime;
        public string countTimer = "00:03:00";
        public string CountTimer
        {
            get => countTimer; set
            {
                countTimer = value;
                OnPropertyChanged("CountTimer");
            }
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CountTime--;
            CountTimer = TimeSpan.FromSeconds(CountTime).ToString();
            if (CountTime == 60)
                timerColor = "Red";
            if (CountTime == 0)
            {
                TimeOut();
            }
        }
        #endregion
        #region Image Source
        private string imgSource;
        public string ImgSource
        {
            get => imgSource; set
            {
                imgSource = value;
                OnPropertyChanged("ImgSource");
            }
        }
        private int paddingLeft;
        private int paddingTop;
        #endregion
        #region Game State
        public bool isEnded = false;
        public bool isStarted = false;

        public Image[,] images = new Image[3, 3];
        public List<int> listorder = new List<int>();
        public Tuple<int, int> emptyPiece;
        #endregion
        #region attribute of pieces
        public int ratio;
        public int imageWidth;
        public int imageHeight;
        #endregion
        private Random rng = new Random();
        BrushConverter bc = new BrushConverter();
        Brush blue;
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string newName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(newName));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            blue = bc.ConvertFrom("#228FDE") as Brush;
            blue.Freeze();
            
            ImgSource = AppDomain.CurrentDomain.BaseDirectory + "Images/Sample.png";
            _timer.Elapsed += Timer_Elapsed;
            PlayGame();
        }

        #region Game
        private void PlayGame()
        {
            MessageBoxResult Result = MessageBox.Show("Start???", "Start Game", MessageBoxButton.OK);
            if (Result == MessageBoxResult.OK)
            {
                this.Activate();
                isStarted = true;
                isEnded = false;
                Step = 0;
                RemovePieces();
                genarateRandomOrder();
                LoadImg(ImgSource);
                CountTime = 180;
                CountTimer = TimeSpan.FromSeconds(CountTime).ToString();
                timerColor = "Black";
                _timer.Enabled = true;
            }
        }
        public void LoadImg(String ImgSource)
        {
            var bitmap = new BitmapImage(new Uri(imgSource));
            var pixelWidth = bitmap.PixelWidth / 3;
            var pixelHeight = bitmap.PixelHeight / 3;
            paddingLeft = 0;
            paddingTop = 0;
            var hintRect = new Int32Rect();

            if (pixelWidth < pixelHeight)
            {
                ratio = pixelWidth;
                paddingTop = (bitmap.PixelHeight - bitmap.PixelWidth) / 2;
                hintRect.X = 0;
                hintRect.Y = paddingTop;
                hintRect.Width = bitmap.PixelWidth;
                hintRect.Height = bitmap.PixelHeight - 2*paddingTop;
            }
            else
            {
                ratio = pixelHeight;
                paddingLeft = (bitmap.PixelWidth - bitmap.PixelHeight) / 2;
                hintRect.X = paddingLeft;
                hintRect.Y = 0;
                hintRect.Height = bitmap.PixelHeight;
                hintRect.Width = bitmap.PixelWidth - 2 * paddingLeft;
            }
            imageWidth = 120;
            imageHeight = 120;// (pixelHeight * imageWidth / pixelWidth);

            hint_img.Source = new CroppedBitmap(bitmap, hintRect);

            emptyPiece = new Tuple<int, int>(2, 2);
            for (int i = 0; i < 9; i++)
            {
                if (i != 8)
                {
                    var index = listorder[i];
                    var index_i = index / 3;
                    var index_j = index % 3;
                    int axisY = (int)(index_i * ratio + paddingTop);
                    int axisX = (int)(index_j * ratio + paddingLeft);

                    var cropped = new CroppedBitmap(bitmap, new Int32Rect(axisX, axisY, ratio, ratio));

                    var imageView = new Image();

                    imageView.Source = cropped;
                    imageView.Width = imageWidth;
                    imageView.Height = imageHeight;
                    Canvas_Game.Children.Add(imageView);

                    Canvas.SetTop(imageView, (i / 3) * (imageHeight));
                    Canvas.SetLeft(imageView, (i % 3) * (imageWidth));

                    images[i / 3, i % 3] = imageView;
                }
                if (i == 8)
                {
                    int axisX = 2 * ratio;
                    int axisY = 2 * ratio;

                    var imageView = new Image();

                    imageView.Width = imageWidth;
                    imageView.Height = imageHeight;
                    Canvas_Game.Children.Add(imageView);

                    Canvas.SetLeft(imageView, 2 * imageWidth);
                    Canvas.SetTop(imageView, 2 * imageHeight);

                    images[2, 2] = imageView;
                }
            }
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            PlayGame();
        }
        private bool CheckOrderPieces(List<int> list)
        {
            for (int i = 0; i < 9; i++)
            {
                if (list[i] != i)
                {
                    return false;
                }
            }
            return true;
        }
        private void _btnFileBrowser_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            _timer.Stop();
            if (openFileDialog.ShowDialog() == true)
            {
                ImgSource = openFileDialog.FileName;
                PlayGame();
                return;
            }
            _timer.Start();
        }
        #endregion

        #region Game Finish
        private void TimeOut()
        {
            _timer.Stop();
            isEnded = true;
            isStarted = false;
            MessageBox.Show("Time Out!\nStep: " + Step);
        }
        private void DrawLastPiece()
        {
            var bitmap = new BitmapImage(new Uri(imgSource));
            var cropped = new CroppedBitmap(bitmap, new Int32Rect(2 * ratio + paddingLeft, 2 * ratio+ paddingTop, ratio, ratio));
            images[2, 2].Source = cropped;
        }
        private void Wining()
        {
            _timer.Stop();
            isEnded = true;
            isStarted = false;
            DrawLastPiece();
            MessageBox.Show("You WIN!\nStep: " + Step + "\nRemain Time: " + TimeSpan.FromSeconds(CountTime).ToString());
        }
        #endregion

        #region random img
        private void genarateRandomOrder()
        {
            listorder = CreateRandomImg();
            while (!IsPuzzleCanSolve(listorder))
            {
                listorder = CreateRandomImg();
            }
        }
        public List<int> CreateRandomImg()
        {
            List<int> rnglist = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            List<int> reslist = new List<int>();
            var rng = new Random();
            while (rnglist.Count > 0)
            {
                var num = rng.Next(rnglist.Count);
                reslist.Add(rnglist[num]);
                rnglist.Remove(rnglist[num]);
            }
            reslist.Add(8);
            return reslist;
        }
        private bool IsPuzzleCanSolve(List<int> list)
        {
            int invCount = getInvCount(list);
            return invCount % 2 == 0;
        }
        private int getInvCount(List<int> list)
        {
            int inv_count = 0;
            for (int i = 0; i < 9 - 1; i++)
                for (int j = i + 1; j < 9; j++)
                    if (list[i] > list[j])
                        inv_count++;
            return inv_count;
        }
        #endregion

        #region Navigate
        private bool Check_Adjacent_Empty(int i, int j)
        {
            var empty_i = emptyPiece.Item1;
            var empty_j = emptyPiece.Item2;
            var distance = Math.Abs(i - empty_i) + Math.Abs(j - empty_j);
            if (distance == 1)
                return true;
            return false;
        }
        private void Canvas_Game_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isStarted && e.ClickCount < 2)
            {
                var pos = e.GetPosition(Canvas_Game);
                var j = (int)(pos.X) / imageWidth;
                var i = (int)(pos.Y) / imageHeight;
                if (i >= 0 && i < 3 && j >= 0 && j < 3)
                {
                    if (Check_Adjacent_Empty(i, j))
                    {
                        int i_empty = emptyPiece.Item1;
                        int j_empty = emptyPiece.Item2;
                        if (i == i_empty)
                        {
                            if (j - j_empty == 1)
                            {
                                game_keyLEFT(i_empty, j_empty);
                            }
                            else if (j - j_empty == -1)
                            {
                                game_keyRIGHT(i_empty, j_empty);
                            }
                        }
                        else if (j == j_empty)
                        {
                            if (i - i_empty == 1)
                            {
                                game_keyUP(i_empty, j_empty);
                            }
                            else if (i - i_empty == -1)
                            {
                                game_keyDOWN(i_empty, j_empty);
                            }
                        }
                        if (CheckOrderPieces(listorder))
                        {
                            Wining();
                        }
                    }
                }
            }
        }
        private void game_keyLEFT(int i, int j)
        {
            if (j + 1 > 2) return;
            var newPos = new Tuple<int, int>(i, j + 1);
            Swap_Img(new Tuple<int, int>(i, j), newPos);
            emptyPiece = newPos;
            Step++;
            if (CheckOrderPieces(listorder))
            {
                Wining();
            }
        }
        private void game_keyRIGHT(int i, int j)
        {
            if (j - 1 < 0) return;
            var newPos = new Tuple<int, int>(i, j - 1);
            Swap_Img(new Tuple<int, int>(i, j), newPos);
            emptyPiece = newPos;
            Step++;
            if (CheckOrderPieces(listorder))
            {
                Wining();
            }
        }
        private void game_keyUP(int i, int j)
        {
            if (i + 1 > 2) return;
            var newPos = new Tuple<int, int>(i + 1, j);
            Swap_Img(new Tuple<int, int>(i, j), newPos);
            emptyPiece = newPos;
            Step++;
            if (CheckOrderPieces(listorder))
            {
                Wining();
            }
        }
        private void game_keyDOWN(int i, int j)
        {
            if (i - 1 < 0) return;
            var newPos = new Tuple<int, int>(i - 1, j);
            Swap_Img(new Tuple<int, int>(i, j), newPos);
            emptyPiece = newPos;
            Step++;
            if (CheckOrderPieces(listorder))
            {
                Wining();
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isStarted)
            {
                if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
                {
                    int i = emptyPiece.Item1;
                    int j = emptyPiece.Item2;
                    if (e.Key == Key.Left)
                    {
                        _btn_left.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, _btn_left));
                    }
                    else if (e.Key == Key.Right)
                    {
                        _btn_right.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, _btn_right));
                    }
                    else if (e.Key == Key.Up)
                    {
                        _btn_up.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, _btn_up));
                    }
                    else if (e.Key == Key.Down)
                    {
                        _btn_down.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, _btn_down));
                    }
                }
            }
        }
        #endregion

        #region Handle Pieces
        private void RemovePieces()
        {
            while (Canvas_Game.Children.Count > 0)
            {
                Canvas_Game.Children.RemoveAt(0);
            }

        }
        private void Swap_Img(Tuple<int, int> src, Tuple<int, int> des)
        {
            var temp_order = listorder[src.Item1 * 3 + src.Item2];
            listorder[src.Item1 * 3 + src.Item2] = listorder[des.Item1 * 3 + des.Item2];
            listorder[des.Item1 * 3 + des.Item2] = temp_order;

            Canvas.SetLeft(images[src.Item1, src.Item2], des.Item2 * (imageWidth));
            Canvas.SetTop(images[src.Item1, src.Item2], des.Item1 * (imageHeight));

            Canvas.SetLeft(images[des.Item1, des.Item2], src.Item2 * (imageWidth));
            Canvas.SetTop(images[des.Item1, des.Item2], src.Item1 * (imageHeight));

            var temp = images[src.Item1, src.Item2];
            images[src.Item1, src.Item2] = images[des.Item1, des.Item2];
            images[des.Item1, des.Item2] = temp;
        }
        #endregion

        #region Save/Load Game
        private void BtnLoadGame_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            open.Title = "Select Game saved";
            if (open.ShowDialog() == true)
            {
                var doc = new XmlDocument();
                doc.Load(open.FileName);

                var root = doc.DocumentElement;

                var img = root.ChildNodes[1];
                ImgSource = img.Attributes["Source"].Value;
                if (!File.Exists(imgSource))
                {
                    MessageBox.Show("Image is not Exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _timer.Start();
                    return;
                }
                ratio = int.Parse(img.Attributes["Ratio"].Value);
                imageWidth = int.Parse(img.Attributes["Width"].Value);
                imageHeight = int.Parse(img.Attributes["Height"].Value);
                paddingLeft = int.Parse(img.Attributes["PaddingLeft"].Value);
                paddingTop = int.Parse(img.Attributes["PaddingTop"].Value);

                isStarted = bool.Parse(root.Attributes["IsStarted"].Value);
                isEnded = bool.Parse(root.Attributes["IsEnded"].Value);

                var time_counter = root.FirstChild.ChildNodes[0];
                var step_counter = root.FirstChild.ChildNodes[1];

                CountTime = int.Parse(time_counter.Attributes["CountTime"].Value);
                timerColor = time_counter.Attributes["Color"].Value;
                Step = int.Parse(step_counter.Attributes["Step"].Value);

                var state = root.ChildNodes[2];
                var empty = state.Attributes["CurrentEmpty"].Value;
                var tokens = empty.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                emptyPiece = new Tuple<int, int>(int.Parse(tokens[0]), int.Parse(tokens[1]));
                for (int i = 0; i < 3; i++)
                {
                    var line = state.ChildNodes[i].Attributes["Value"].Value;
                    tokens = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    listorder[i * 3] = int.Parse(tokens[0]);
                    listorder[i * 3 + 1] = int.Parse(tokens[1]);
                    listorder[i * 3 + 2] = int.Parse(tokens[2]);
                }
                RemovePieces();
                LoadImg(imgSource);
                CountTimer = TimeSpan.FromSeconds(CountTime).ToString();
                if (!isEnded) _timer.Start();
                if (CheckOrderPieces(listorder))
                {
                    Wining();
                }
            }
            else _timer.Start();
        }
        private void BtnSaveGame_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            if (save.ShowDialog() == true)
            {
                XmlDocument doc = new XmlDocument();
                var root = doc.CreateElement("Game");
                root.SetAttribute("IsStarted", isStarted.ToString());
                root.SetAttribute("IsEnded", isEnded.ToString());

                var counter = doc.CreateElement("Counter");

                var time_counter = doc.CreateElement("TimeCounter");
                time_counter.SetAttribute("CountTime", CountTime.ToString());
                time_counter.SetAttribute("Color", timerColor.ToString());

                var step_counter = doc.CreateElement("StepCounter");
                step_counter.SetAttribute("Step", Step.ToString());
                counter.AppendChild(time_counter);
                counter.AppendChild(step_counter);

                root.AppendChild(counter);

                var img = doc.CreateElement("Image");
                img.SetAttribute("Source", ImgSource.ToString());
                img.SetAttribute("Ratio", ratio.ToString());
                img.SetAttribute("Width", imageWidth.ToString());
                img.SetAttribute("Height", imageHeight.ToString());
                img.SetAttribute("PaddingLeft", paddingLeft.ToString());
                img.SetAttribute("PaddingTop", paddingTop.ToString());
                root.AppendChild(img);

                var state = doc.CreateElement("State");
                state.SetAttribute("CurrentEmpty", $"{emptyPiece.Item1} {emptyPiece.Item2}");
                for (int i = 0; i < 3; i++)
                {
                    var line = doc.CreateElement("Line");
                    line.SetAttribute("Value", $"{listorder[i * 3]} {listorder[i * 3 + 1]} {listorder[i * 3 + 2]}");
                    state.AppendChild(line);
                }
                root.AppendChild(state);

                doc.AppendChild(root);
                doc.Save(save.FileName);
            }
            _timer.Start();
        }
        #endregion

        #region KeyPress
        private async void Btn_keyUP(object sender, RoutedEventArgs e)
        {
            _btn_up.Background = Brushes.IndianRed;
            game_keyUP(emptyPiece.Item1, emptyPiece.Item2);
            await Task.Delay(50);
            _btn_up.Background = blue;
        }
        private async void Btn_keyRIGHT(object sender, RoutedEventArgs e)
        {
            _btn_right.Background = Brushes.IndianRed;
            game_keyRIGHT(emptyPiece.Item1, emptyPiece.Item2);
            await Task.Delay(50);
            _btn_right.Background = blue;
        }
        private async void Btn_keyDOWN(object sender, RoutedEventArgs e)
        {
            _btn_down.Background = Brushes.IndianRed;
            game_keyDOWN(emptyPiece.Item1, emptyPiece.Item2);
            await Task.Delay(50);
            _btn_down.Background = blue;
        }
        private async void Btn_keyLEFT(object sender, RoutedEventArgs e)
        {
            _btn_left.Background = Brushes.IndianRed;
            await Task.Delay(50);
            game_keyLEFT(emptyPiece.Item1, emptyPiece.Item2);
            _btn_left.Background = blue;
        }
        // SaveMaxPoint
        
        #endregion
    }
}