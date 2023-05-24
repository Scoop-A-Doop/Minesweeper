using System;
using System.Drawing;
using System.Windows.Forms;

namespace SShouibMinesweeper
{
    public partial class Cell : UserControl
    {
        private int cellSize = 24;
        private int labelSize;
        private int buttonRow, buttonCol;
        private Label myLabel = new Label();
        private Button myButton = new Button();
        private Panel myPanel = new Panel();
        public EventHandler CellHasBeenClicked;

        public Cell()
        {
            InitializeComponent();

            this.Size = new Size(CellSize, CellSize);

            //Create panel, label and button
            MyPanel.BackColor = Color.LightGray;

            MyLabel.BackColor = Color.LightSteelBlue;
            LabelSize = CellSize - 1;
            MyLabel.Size = new Size(LabelSize, LabelSize);

            MyButton.Size = new Size(CellSize, CellSize);
            MyButton.Click += OnButtonClickHandler;

            //Add panel label and button
            this.Controls.Add(MyButton);
            this.Controls.Add(MyLabel);
            this.Controls.Add(MyPanel);
        }

        /// <summary>
        /// Property for the size of the cell
        /// </summary>
        public int CellSize { get => cellSize; }

        /// <summary>
        /// Property for the size of the label on the cell
        /// </summary>
        public int LabelSize { get => labelSize; set => labelSize = value; }

        /// <summary>
        /// Property for the button row value in Grid in MinesweeperForm
        /// </summary>
        public int ButtonRow { get => buttonRow; set => buttonRow = value; }

        /// <summary>
        /// Property for the Button column value in Grid in MinesweeperForm
        /// </summary>
        public int ButtonCol { get => buttonCol; set => buttonCol = value; }

        /// <summary>
        /// Property for the Label's color
        /// </summary>
        public Color CellColor { get => MyLabel.BackColor; set => MyLabel.BackColor = value; }

        /// <summary>
        /// Property for the Panel in the cell
        /// </summary>
        public Panel MyPanel { get => myPanel; set => myPanel = value; }

        /// <summary>
        /// Property for the Button in the cell
        /// </summary>
        public Button MyButton { get => myButton; set => myButton = value; }

        /// <summary>
        /// Property for the Label in the cell
        /// </summary>
        public Label MyLabel { get => myLabel; set => myLabel = value; }

        /// <summary>
        /// Handler that activates when a button is clicked
        /// </summary>
        private void OnButtonClickHandler(object sender, EventArgs e)
        {
            MyButton.Visible = false;
            if (CellHasBeenClicked != null)
            {
                this.CellHasBeenClicked(this, e);
            }
        }
    }
}