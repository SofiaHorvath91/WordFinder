using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WordFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> chosenWords = new List<string>();
        List<string> words = new List<string>();
        List<string> foundWords = new List<string>();
        int rowCount = 20;
        int colCount = 20;
        int foundWordsCount;
        int yourTries;
        bool[,] wordLetters;
        Label[,] table;
        Random generator = new Random();
        HashSet<Label> selectedGreens = new HashSet<Label>();
        HashSet<Label> selectedBlacks = new HashSet<Label>();
        List<Label> selectedLabels = new List<Label>();
        List<Label> helpLabels = new List<Label>();

        public MainWindow()
        {
            InitializeComponent();
            mainGrid.Background = new ImageBrush(new BitmapImage(new Uri(System.IO.Path.GetFullPath("background.png"))));

        }

        void FillLabelsWithRandomLetters()
        {
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    table[i, j].Content = (char)generator.Next(65, 91);
                }
            }
        }
        
        void CreateLabels()
        {
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    table[i, j] = new Label()
                    {
                        Content = 'x',
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(60 + 20 * i, 100 + 20 * j, 0, 0),
                        Width = 20,
                        Height = 20,
                        FontSize = 12,
                        Padding = new Thickness(0),
                        Foreground = Brushes.Black,
                        Background = Brushes.White,
                        BorderThickness = new Thickness(1),
                        BorderBrush = Brushes.Black,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                    };

                    table[i, j].MouseMove += new MouseEventHandler(label_MouseMove);

                    ((Grid)(FindName("mainGrid"))).Children.Add(table[i, j]);
                }
            }
        }

        void StartGame()
        {
            List<Control> toRemove = new List<Control>();
            foreach (Control l in mainGrid.Children)
            {
                if (! l.Name.Contains("_static"))
                {
                    toRemove.Add(l);
                }
            }
            foreach (Control l in toRemove)
            {
                mainGrid.Children.Remove(l);
            }

            words.Clear();
            chosenWords.Clear();
            foundWords.Clear();
            selectedBlacks.Clear();
            selectedGreens.Clear();
            selectedLabels.Clear();

            table = new Label[rowCount, colCount];
            wordLetters = new bool[rowCount, colCount];

            ReadInWords("words.txt");

            CreateLabels();
            FillLabelsWithRandomLetters();
            PlaceWords();

            yourTries = 0;
            foundWordsCount = chosenWords.Count;
            roundsToGoLabel_static.Content = "WORDS : " + foundWordsCount.ToString() + " / " + chosenWords.Count.ToString();
            yourTriesLabel_static.Content = "TRIES : " + yourTries.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        void PlaceWords()
        {
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    if (PossibleToPlaceWordHere(i, j))
                    {
                        TryToPutDown(i, j);
                    }
                }
            }
        }

        void TryToPutDown(int x, int y)
        {
            bool placed = false;
            for (int i = 0; i < 50 && !placed; i++)
            {
                int index = generator.Next(0, words.Count);
                string word = words[index].ToUpper();
                if (!chosenWords.Contains(word))
                {
                    if (WordWasPlaced(x, y, word))
                    {
                        placed = true;
                    }
                }
            }
        }

        bool PossibleToPlaceWordHere(int x, int y)
        {
            return !wordLetters[x, y] && generator.Next(101) <= 10;
        }

        bool WordWasPlaced(int x, int y, string word)
        {
            return WordWasPlacedHorizontally(x, y, word)
                || WordWasPlacedVertically(x, y, word)
                || WordWasPlacedDiagonally(x, y, word);
        }

        bool WordWasPlacedHorizontally(int x, int y, string word)
        {
            if (WordCanBePlacedHorizontally(x, y, word))
            {
                for (int j = y; j - y < word.Length; j++)
                {
                    table[x, j].Content = word[j - y];
                    wordLetters[x, j] = true;
                }

                chosenWords.Add(word);

                return true;
            }
            return false;
        }

        bool LetterCanBePlaced(int x, int y, char letter)
        {
            return !wordLetters[x, y] || table[x, y].Content.ToString() == letter.ToString();

            //if (!wordLetters[x, y])
            //{
            //    return true;
            //}
            //else if(table[x,y].Content.ToString() == letter.ToString())
            //{
            //    return true;
            //}
            //return false;
        }

        bool WordCanBePlacedHorizontally(int x, int y, string word)
        {
            int j = y;
            while (j < colCount && j - y < word.Length && LetterCanBePlaced(x, j, word[j - y]))
            {
                j++;
            }

            return j - y == word.Length;
        }

        bool WordWasPlacedVertically(int x, int y, string word)
        {
            if (WordCanBePlacedVertically(x, y, word))
            {
                for (int i = x; i - x < word.Length; i++)
                {
                    table[i, y].Content = word[i - x];
                    wordLetters[i, y] = true;
                }

                chosenWords.Add(word);

                return true;
            }
            return false;
        }

        bool WordCanBePlacedVertically(int x, int y, string word)
        {
            int i = x;
            while (i < rowCount && i - x < word.Length && LetterCanBePlaced(i, y, word[i - x]))
            {
                i++;
            }

            return i - x == word.Length;
        }

        bool WordWasPlacedDiagonally(int x, int y, string word)
        {
            if (WordCanBePlacedDiagonally(x, y, word))
            {
                for (int s = 0; s < word.Length; s++)
                {
                    table[s + x, s + y].Content = word[s];
                    wordLetters[s + x, s + y] = true;
                }

                chosenWords.Add(word);

                return true;
            }
            return false;
        }

        bool WordCanBePlacedDiagonally(int x, int y, string word)
        {
            int s = 0;
            while (s + x < rowCount && s + y < colCount && s < word.Length && LetterCanBePlaced(s + x, s + y, word[s]))
            {
                s++;
            }

            return s == word.Length;
        }

        void ReadInWords(string list)
        {
            StreamReader reader = new StreamReader(list);
            string text = reader.ReadToEnd();
            string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                words.Add(line);
            }
        }

        private void label_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition((IInputElement)sender);
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseInTheCircle((Label)sender, mousePos))
            {
                Label l = (Label)sender;
                if (l.Background == Brushes.LightGreen)
                {
                    selectedGreens.Add(l);
                }
                else if (l.BorderBrush == Brushes.Black)
                {
                    selectedBlacks.Add(l);
                }
                l.BorderBrush = Brushes.Red;

                if (!selectedLabels.Contains(l))
                {
                    selectedLabels.Add(l);
                }
            }
        }

        bool IsMouseInTheCircle(Label l, Point mousePos)
        {
            double x = mousePos.X;
            double y = mousePos.Y;
            double r = l.Width / 2.5;
            double u = l.Width / 2;
            double v = l.Width / 2;

            return Math.Pow(x - u, 2) + Math.Pow(y - v, 2) < Math.Pow(r, 2);
        }

        void ColorBackToOriginal()
        {
            selectedGreens.ToList().ForEach(x => x.BorderBrush = Brushes.Black);
            selectedBlacks.ToList().ForEach(x => x.BorderBrush = Brushes.Black);
        }

        private void MouseLeftButtonReleased(object sender, MouseEventArgs e)
        {
            if (selectedLabels.Count == 0)
            {
                return;
            }

            yourTries++;
            yourTriesLabel_static.Content = "TRIES : " + yourTries.ToString();

            if (IsValidSelection())
            {
                string choosenLabelsString = ChoosenLabelsToString();

                if (IsWordFromList(choosenLabelsString))
                {
                    selectedLabels.ForEach(x => x.Background = Brushes.LightGreen);
                    selectedLabels.ForEach(x => x.BorderBrush = Brushes.Black);

                    foundWords.Add(choosenLabelsString);
                    foundWordsCount--;
                    roundsToGoLabel_static.Content = "WORDS : " + foundWordsCount.ToString() + " / " + chosenWords.Count.ToString();

                    if(foundWordsCount == 0)
                    {
                        MessageBox.Show("CONGRATULATIONS, YOU WON!");
                    }
                }
                else
                {
                    ColorBackToOriginal();
                }
            }
            else
            {
                MessageBox.Show("Invalid choice.");
                ColorBackToOriginal();
            }

            selectedLabels.Clear();
            selectedGreens.Clear();
            selectedBlacks.Clear();
        }

        private void helpMeButton_Click(object sender, EventArgs e)
        {
            foreach(Label l in helpLabels)
            {
                l.Content = null;
            }

            List<string> wordsToFind = chosenWords.Except(foundWords).ToList();
            
            List<string> firstGroupWordsToFind = wordsToFind.Take(15).ToList();
            for (int i = 0; i < firstGroupWordsToFind.Count; i++)
            {
                mainGrid.Children.Add(new Label()
                {
                    Name = firstGroupWordsToFind[i] + "_helpWordsLabel",
                    Height = 30,
                    Width = 120,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    FontFamily = new FontFamily("Footlight MT Light"),
                    FontSize = 13,
                    FontWeight = FontWeights.ExtraBold,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(510, 255 + i * 20, 0, 0),
                    Content = firstGroupWordsToFind[i],
                });
            }
            List<string> secondGroupWordsToFind = wordsToFind.Skip(15).ToList();
            for (int i = 0; i < secondGroupWordsToFind.Count; i++)
            {
                mainGrid.Children.Add(new Label()
                {
                    Name = secondGroupWordsToFind[i] + "_helpWordsLabel",
                    Height = 30,
                    Width = 120,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    FontFamily = new FontFamily("Footlight MT Light"),
                    FontSize = 13,
                    FontWeight = FontWeights.ExtraBold,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(630, 255 + i * 20, 0, 0),
                    Content = secondGroupWordsToFind[i],
                });;
            }
            foreach(Control l in mainGrid.Children)
            {
                if(l.Name.Contains("_helpWordsLabel"))
                {
                    helpLabels.Add((Label)l);
                }
            }
        }
        
        bool IsValidSelection()
        {
            return IsValidHorizontalSelection() || IsValidVerticalSelection() || IsValidDiagonalSelection();
        }

        bool IsValidHorizontalSelection()
        {
            int i = 1;
            while (i < selectedLabels.Count && selectedLabels[i].Margin.Top == selectedLabels[0].Margin.Top)
            {
                i++;
            }

            return i == selectedLabels.Count;
        }

        bool IsValidVerticalSelection()
        {
            int i = 1;
            while (i < selectedLabels.Count && selectedLabels[i].Margin.Left == selectedLabels[0].Margin.Left)
            {
                i++;
            }

            return i == selectedLabels.Count;
        }

        bool IsValidDiagonalSelection()
        {
            if (selectedLabels.Count == 1)
            {
                return true;
            }

            Direction firstDirection = GetDirection(selectedLabels[0], selectedLabels[1]);

            if (firstDirection == Direction.Other)
            {
                return false;
            }

            int i = 1;
            while (i < selectedLabels.Count - 1 && GetDirection(selectedLabels[i], selectedLabels[i + 1]) == firstDirection)
            {
                i++;
            }

            return i == selectedLabels.Count - 1;
        }

        Direction GetDirection(Label l1, Label l2)
        {
            if (l1.Margin.Top - l2.Margin.Top == 20 && l1.Margin.Left == l2.Margin.Left - 20)
            {
                return Direction.NorthEast;
            }
            else if (l1.Margin.Top - l2.Margin.Top == 20 && l1.Margin.Left == l2.Margin.Left + 20)
            {
                return Direction.NorthWest;
            }
            else if (l1.Margin.Top - l2.Margin.Top == -20 && l1.Margin.Left == l2.Margin.Left + 20)
            {
                return Direction.SouthEast;
            }
            else if (l1.Margin.Top - l2.Margin.Top == -20 && l1.Margin.Left == l2.Margin.Left - 20)
            {
                return Direction.SouthWest;
            }
            else
            {
                return Direction.Other;
            }
        }

        bool IsWordFromList(string word)
        {
            string wordReverse = word.Reverse().ToString();

            return chosenWords.IndexOf(word) != -1 || chosenWords.IndexOf(wordReverse) != -1;
        }

        string ChoosenLabelsToString()
        {
            return selectedLabels.Select(x => x.Content.ToString()).Aggregate((x, y) => x + y);
        }

        private void newGameButton_Click(object sender, EventArgs e)
        {
            StartGame();
        }
    }
}
