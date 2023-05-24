using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace SShouibMinesweeper
{
    public partial class MinesweeperForm : Form
    {
        Cell[,] grid;

        Random randomNumber = new Random();
        StatusStrip myStatusStrip = new StatusStrip();
        Timer myTimer = new Timer();

        List<string> fileStatsList = new List<string>(); //Index 0 and 1 are wins - losses. Everything else is game completion time
        string fileName = "MinesweeperStats.txt";
        int wins, losses;
        double averageTime = 0;

        int timeElapsed;

        int gridSize = 10;
        int gridOffset = 30;
        int numberOfMines = 10;
        int safeCells;

        /// <summary>
        /// Initialize and load the controls for the Minesweepre Form, and house the handlers
        /// </summary>
        public MinesweeperForm()
        {
            InitializeComponent();
            LoadStats();
            LoadGrid();
            LoadMines();
            LoadNumbers();
            LoadTimer();
            LoadStatusStrip();

            myTimer.Tick += UpdateTimer;

            statsMenuItem.Click += OnStatsClick;
            exitMenuItem.Click += OnExitClick;
            resetMenuItem.Click += OnResetClick;
            aboutMenuItem.Click += OnAboutClick;
            instructionsMenuItem.Click += OnInstructionsClick;
        }

        #region Load Form Controls
        /// <summary>
        /// Reads through the txt file and saves the first line to wins then the second to losses, then saves those values into the list
        /// Afterwards, goes through the rest of the file (which are completion times, if there even is previous data) and saves the times to the list
        /// </summary>
        private void LoadStats()
        {
            try
            {
                //Read MinesweeperStats.txt with reader
                using (StreamReader reader = new StreamReader(fileName))
                {
                    wins = int.Parse(reader.ReadLine());
                    losses = int.Parse(reader.ReadLine());
                    fileStatsList.Add(wins.ToString());
                    fileStatsList.Add(losses.ToString());

                    while (!reader.EndOfStream) //While MinesweeperStats.txt has not reached the end,
                    {
                        fileStatsList.Add(reader.ReadLine());
                    }
                    CalculateAverageTime();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Loads the minesweeper board by initializing a cell for each spot on the grid
        /// </summary>
        private void LoadGrid()
        {
            grid = new Cell[gridSize, gridSize];
            safeCells = 0;

            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(0); col++)
                {
                    safeCells++;
                    grid[col, row] = new Cell();
                    grid[col, row].Location = new Point(grid[col, row].CellSize * col, grid[col, row].CellSize * row + gridOffset);

                    //Save this info for OnCellClick
                    grid[col, row].ButtonCol = col;
                    grid[col, row].ButtonRow = row;
                    grid[col, row].CellHasBeenClicked += OnCellClick;

                    this.Controls.Add(grid[col, row]);
                }
            }
        }

        /// <summary>
        /// Randomly places x amount of mines on the board
        /// </summary>
        private void LoadMines()
        {
            for (int i = 0; i < numberOfMines; i++)
            {
                int mineRow = randomNumber.Next(0, gridSize);
                int mineCol = randomNumber.Next(0, gridSize);

                //If the cell label is empty, then place the mine there. Else, set counter back by 1 and try again
                if (grid[mineRow, mineCol].MyLabel.Text == "")
                {
                    safeCells--; //Subtract a "safe cell" for each mine added to the board
                    grid[mineRow, mineCol].MyLabel.Text = "X";
                    grid[mineRow, mineCol].MyLabel.BackColor = Color.Red;
                }
                else
                {
                    i--;
                }
            }
        }

        /// <summary>
        /// Goes through every cell and (if within bounds) checks every surrounding cell to see if it's a mine.
        /// Every time theres a mine surrounding the current cell, the counter increases.
        /// At the end, set the current cell's label to be the counters value (mines touching the cell).
        /// </summary>
        private void LoadNumbers()
        {
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(0); col++)
                {
                    int touchingMines = 0;
                    //If there is no mine on this space
                    if (!(grid[row, col].MyLabel.Text == "X"))
                    {
                        //Check up
                        touchingMines += (row != 0 && grid[row - 1, col].MyLabel.Text == "X") ? 1 : 0;

                        //Check down
                        touchingMines += (row != 9 && grid[row + 1, col].MyLabel.Text == "X") ? 1 : 0;

                        //Check left
                        touchingMines += (col != 0 && grid[row, col - 1].MyLabel.Text == "X") ? 1 : 0;

                        //Check right
                        touchingMines += (col != 9 && grid[row, col + 1].MyLabel.Text == "X") ? 1 : 0;

                        //Check up left
                        touchingMines += (row != 0 && col != 0 && grid[row - 1, col - 1].MyLabel.Text == "X") ? 1 : 0;

                        //Check up right
                        touchingMines += (row != 0 && col != 9 && grid[row - 1, col + 1].MyLabel.Text == "X") ? 1 : 0;

                        //Check down left
                        touchingMines += (row != 9 && col != 0 && grid[row + 1, col - 1].MyLabel.Text == "X") ? 1 : 0;

                        //Check down right
                        touchingMines += (row != 9 && col != 9 && grid[row + 1, col + 1].MyLabel.Text == "X") ? 1 : 0;

                        //Only set the text to be the number if there is at least 1 mine touching the cell
                        if (touchingMines != 0)
                        {
                            grid[row, col].MyLabel.Text = touchingMines.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the timer by setting its tick speed and initializes the timeElapsed counter to 0
        /// </summary>
        private void LoadTimer()
        {
            timeElapsed = 0;
            myTimer.Interval = 1000;
        }

        /// <summary>
        /// Loads the status strip to display the time elapsed
        /// </summary>
        private void LoadStatusStrip()
        {
            this.Controls.Add(myStatusStrip);
            myStatusStrip.Items.Add(timeElapsed.ToString() + " Seconds");
            myStatusStrip.BackColor = Color.Black;
            myStatusStrip.ForeColor = Color.Red;
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the txt file that contains the lifetime stats by completely rewriting it with the stats List
        /// </summary>
        private void UpdateStats()
        {
            File.WriteAllLinesAsync(fileName, fileStatsList);
        }

        /// <summary>
        /// If there fileStatsList has a completion time saved, then set the averageTime to 0 and calculate the averaege completion time
        /// (i starts at 2 because index 0 and 1 contain win/loss stats. That's also why average time is divided by count - 2)
        /// </summary>
        private void CalculateAverageTime()
        {
            if (fileStatsList.Count >= 2)
            {
                averageTime = 0;    //Reset averageTime before calculation, to prevent inaccuracies  
                for (int i = 2; i < fileStatsList.Count; i++)
                {
                    averageTime += double.Parse(fileStatsList[i]);
                }
                averageTime = averageTime / (fileStatsList.Count - 2);
            }
        }

        /// <summary>
        /// After every 1000 ticks (1 second), increase the timeElapsed counter and update the status strip
        /// </summary>
        private void UpdateTimer(object sender, EventArgs e)
        {
            timeElapsed++;
            myStatusStrip.Items.Clear();
            myStatusStrip.Items.Add(timeElapsed.ToString() + " Seconds");
        }
        #endregion

        #region OnCellClick and its helper methods
        /// <summary>
        /// After a button is clicked, checks to see if the user hit a mine, won, or activate all empty adjacent cells
        /// </summary>
        private void OnCellClick(object sender, EventArgs e)
        {
            myTimer.Start();
            Cell temp = (Cell)sender;
            Color clickedColor = temp.CellColor;
            MineHit(temp);
            safeCells--;   //The game isn't over yet so decrease the amount of safe spaces left
            CheckLeft(temp, clickedColor);
            CheckRight(temp, clickedColor);
            CheckUp(temp, clickedColor);
            CheckDown(temp, clickedColor);
            CheckWin();
        }

        /// <summary>
        /// If the user hits a mine, then lock all buttons on the board as a "Game Over"
        /// </summary>
        private void MineHit(Cell temp)
        {
            if (temp.MyLabel.Text == "X")
            {
                myTimer.Stop(); //Stop the timer now that the user lost
                losses = int.Parse(fileStatsList[1]) + 1;
                fileStatsList[1] = losses.ToString();
                UpdateStats();
                for (int row = 0; row < grid.GetLength(0); row++)
                {
                    for (int col = 0; col < grid.GetLength(0); col++)
                    {
                        if (grid[col, row].MyLabel.Text == "X")
                        {
                            grid[col, row].MyButton.Visible = false;
                        }
                        else
                        {
                            grid[col, row].MyButton.Enabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check all cells to the right and reveal them if they aren't a mine or a number next to a number
        /// </summary>
        private void CheckRight(Cell temp, Color clickedColor)
        {
            if (temp.MyLabel.Text == "" && temp.ButtonCol + 1 < grid.GetLength(0))
            {
                if (clickedColor.Equals(grid[temp.ButtonCol + 1, temp.ButtonRow].CellColor))
                {
                    grid[temp.ButtonCol + 1, temp.ButtonRow].MyButton.PerformClick();
                }
            }
        }

        /// <summary>
        /// Check all cells to the left and reveal them if they aren't a mine or a number next to a number
        /// </summary>
        private void CheckLeft(Cell temp, Color clickedColor)
        {
            if (temp.MyLabel.Text == "" && temp.ButtonCol - 1 >= 0)
            {
                if (clickedColor.Equals(grid[temp.ButtonCol - 1, temp.ButtonRow].CellColor))
                {
                    grid[temp.ButtonCol - 1, temp.ButtonRow].MyButton.PerformClick();
                }
            }
        }

        /// <summary>
        /// Check all cells above and reveal them if they aren't a mine or a number next to a number
        /// </summary>
        private void CheckUp(Cell temp, Color clickedColor)
        {
            if (temp.MyLabel.Text == "" && temp.ButtonRow - 1 >= 0)
            {
                if (clickedColor.Equals(grid[temp.ButtonCol, temp.ButtonRow - 1].CellColor))
                {
                    grid[temp.ButtonCol, temp.ButtonRow - 1].MyButton.PerformClick();
                }
            }
        }

        /// <summary>
        /// Check all cells below and reveal them if they aren't a mine or a number next to a number
        /// </summary>
        private void CheckDown(Cell temp, Color clickedColor)
        {
            if (temp.MyLabel.Text == "" && temp.ButtonRow + 1 < grid.GetLength(1))
            {
                if (clickedColor.Equals(grid[temp.ButtonCol, temp.ButtonRow + 1].CellColor))
                {
                    grid[temp.ButtonCol, temp.ButtonRow + 1].MyButton.PerformClick();
                }
            }
        }

        /// <summary>
        /// Check to see if the user has won the game
        /// If the user did win, stop the timer, update the win counter, update the win count to the file stats, 
        /// add a new completion time of the file stats, and calculate the new average time.
        /// Then reveal all the mines and finally update the text file
        /// </summary>
        private void CheckWin()
        {
            if (safeCells == 0)
            {
                myTimer.Stop();
                wins = int.Parse(fileStatsList[0]) + 1;
                fileStatsList[0] = wins.ToString();
                fileStatsList.Add(timeElapsed.ToString());
                CalculateAverageTime();
                UpdateStats();
                for (int row = 0; row < grid.GetLength(0); row++)
                {
                    for (int col = 0; col < grid.GetLength(0); col++)
                    {
                        if (grid[col, row].MyLabel.Text == "X")
                        {
                            grid[col, row].MyButton.Enabled = false;
                            grid[col, row].MyButton.BackColor = Color.Green;
                        }
                    }
                }
            }
        }
        #endregion

        #region Menu Strip Handlers
        /// <summary>
        /// Shows the lifetime statistics when the Stats button on the menustrip is clicked
        /// </summary>
        private void OnStatsClick(object sender, EventArgs e)
        {
            MessageBox.Show("Wins-Losses: " + wins + " - " + losses + "\nAverage Time: " + averageTime + " Seconds", "Stats");
        }

        /// <summary>
        /// Reset the form by clearing every control on the form and load brand new controls when Reset button on menustrip is clicked
        /// </summary>
        private void OnResetClick(object sender, EventArgs e)
        {
            myTimer.Stop();
            myStatusStrip.Items.Clear();
            this.Controls.Clear();
            fileStatsList.Clear();
            LoadStats();
            this.Controls.Add(myMenuStrip);
            LoadGrid();
            LoadMines();
            LoadNumbers();
            LoadTimer();
            LoadStatusStrip();
        }

        /// <summary>
        /// Shuts down the program when the Exit button on menustrip is clicked
        /// </summary>
        private void OnExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Displays a small "About me" message box when the About button on menustrip is clicked
        /// </summary>
        private void OnAboutClick(object sender, EventArgs e)
        {

            MessageBox.Show("Creator: Sully Shouib\nDate: 04/17/21\nClass: CS 3020", "About");
        }

        /// <summary>
        /// Displays instructions in a message box when the Instructions button on menustrip is clicked
        /// </summary>
        private void OnInstructionsClick(object snder, EventArgs e)
        {
            MessageBox.Show("Clear the board without setting off any mines!\n" +
                "A number on a square represents how many mines are touching that square, so the higher the number, the more mines surrounding that square!\n" +
                "Clear all safe squares to win the game, but if you set off one mine, then it's game over!\n" +
                "There are a total of " + numberOfMines + " mines on the board. Avoid them at all costs!\n" +
                "Good luck!", "Instructions");
        }
        #endregion
    }
}