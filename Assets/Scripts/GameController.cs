using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    //Singleton
    static GameController main;

    //Public variables
    public Text scoreText;
    public Text gameOverText;
    public Text nextText;

    //Game variables
    private GameObject[,] gameCubes = new GameObject[10, 24];
    private int[,] gameBoard = new int[10, 24];
    private FallingTile fallingTile;
    private FallingTile nextTile;
    private int timer = 15;
    private bool speedUp = false;
    private bool gameOver = false;
    private int rowComplete = -1;
    private bool rowAnimation = false;
    private int score;
    private int scoreMultiplier;
    private int rowsCleared;
    private int rotator;

    //Sounds
    private AudioSource audio;
    public AudioClip placeSFX;
    public AudioClip clearSFX;
    public AudioClip loseSFX;

    //Initialization
    void Start ()
    {
        //Set static variable
        main = this;
        //Create cube primitives
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 24; j++)
            {
                gameCubes[i, 23-j] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gameCubes[i, 23-j].transform.position = new Vector3(-4.5F + i, 14F - j, -1);
                gameCubes[i, 23-j].transform.localScale = new Vector3(0.9F, 0.9F, 0.9F);
            }
        }
        audio = GetComponent<AudioSource>();
        rotator = 200;
        startGame();
    }
	
	// Update is called once per frame
	void Update ()
    {
        handleKeys();
        gameLogic();
        //Rotate the camera
        if (rotator > 0) transform.Translate(Vector3.left/10);
        else transform.Translate(Vector3.right/10);
        transform.LookAt(Vector3.zero);
        rotator++;
        if (rotator > 400) rotator = -399;
        //Change the color of the actual cubes
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 24; j++)
            {
                gameCubes[i, j].SetActive(getBlockColor(gameBoard[i, j]) != Color.clear);
                gameCubes[i, j].GetComponent<Renderer>().material.SetColor("_Color", getBlockColor(gameBoard[i, j]));
            }
        }
        //Update UI elements
        scoreText.text = score.ToString();
        if (gameOver)
            gameOverText.text = "Game Over";
        else
            gameOverText.text = "";
        nextText.text = nextTile.ToString();
    }

    //Handle keyboard input
    private void handleKeys()
    {
        if (Input.GetKeyDown("r"))
            startGame();
        if (Input.GetKeyDown("up"))
            if (!gameOver && !rowAnimation && !fallingTile.atBottom) fallingTile.rotate();
        if (Input.GetKeyDown("down"))
            if (!gameOver && !rowAnimation) speedUp = true;
        if (Input.GetKeyUp("down"))
                speedUp = false;
        if (Input.GetKeyDown("left"))
            if (!gameOver && !rowAnimation && !fallingTile.atBottom) fallingTile.moveLeft();
        if (Input.GetKeyDown("right"))
            if (!gameOver && !rowAnimation && !fallingTile.atBottom) fallingTile.moveRight();
    }

    //Start The Game
    private void startGame()
    {
        gameOver = false;
        //Prepare game board
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 24; j++)
            {
                gameBoard[i, j] = 0;
            }
        }
        fallingTile = newTile();
        nextTile = newTile();
        fallingTile.colorShape();
        speedUp = false;
        rowAnimation = false;
        rowComplete = -1;
        score = 0;
        scoreMultiplier = 40;
        rowsCleared = 0;
        audio.PlayOneShot(placeSFX);
    }

    //Game Logic
    private void gameLogic()
    {
        if (gameOver) return;
        if (timer > 0 && !speedUp)
        {
            timer--;
        }
        else
        {
            timer = Mathf.Max(1, 60 - rowsCleared*2);
            if (rowComplete != -1)
            {
                shiftTilesDown(rowComplete);
                rowComplete = -1;
                score += scoreMultiplier;
                if (scoreMultiplier == 40) scoreMultiplier = 60;
                else if (scoreMultiplier == 60) scoreMultiplier = 200;
                if (scoreMultiplier == 200) scoreMultiplier = 900;
                rowsCleared++;
                audio.PlayOneShot(clearSFX);
                return;
            }
            if (fallingTile.atBottom && fallingTile.isAtTop())
            {
                gameOver = true;
                audio.PlayOneShot(loseSFX);
            }
            else if (fallingTile.atBottom)
            {
                rowComplete = checkRows();
                if (rowComplete != -1)
                {
                    rowAnimation = true;
                    speedUp = false;
                    timer = 15;
                }
                else
                {
                    rowAnimation = false;
                    fallingTile = nextTile;
                    nextTile = newTile();
                    fallingTile.colorShape();
                    audio.PlayOneShot(placeSFX);
                    scoreMultiplier = 40;
                }
            }
            else if (!rowAnimation) fallingTile.moveDown();
        }
    }

    //Make a falling tile
    private FallingTile newTile()
    {
        return new FallingTile(3, 20, Random.Range(1, 8));
    }

    //Give each block a different color
    private Color getBlockColor(int v)
    {
        switch (v)
        {
            case 1: return Color.red;
            case 2: return Color.green;
            case 3: return Color.blue;
            case 4: return Color.yellow;
            case 5: return Color.cyan;
            case 6: return Color.magenta;
            case 7: return new Color(1, 0.5F, 0, 1); //Orange
            case -1: return Color.white;
            default: return Color.clear;
        }
    }

    //Check if any rows are complete, returns the complete row
    private int checkRows()
    {
        bool complete = false;
        for (int j = 0; j < 24; j++)
        {
            complete = true;
            for (int i = 0; i < 10; i++)
            {
                if (gameBoard[i, j] == 0) complete = false;
            }
            if (complete)
            {
                for (int i = 0; i < 10; i++)
                {
                    gameBoard[i, j] = -1;
                }
                return j;
            }
        }
        return -1;
    }

    //Shifts all tiles above the column down, erasing the column
    private void shiftTilesDown(int column)
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = column; j < 23; j++)
            {
                gameBoard[i, j] = gameBoard[i, j + 1];
            }
            gameBoard[i, 23] = 0;
        }
    }

    //Falling Tile
    class FallingTile
    {
        private int x;
        private int y;
        private int color;
        private bool[,] shape = new bool[4, 4];
        public bool atBottom = false;
        public FallingTile(int i, int j, int c)
        {
            x = i;
            y = j;
            color = c;
            selectShape();
        }

        public void selectShape()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    shape[i, j] = false;
                }
            }
            switch (color)
            {
                case 1:
                    shape[1, 1] = true;
                    shape[2, 1] = true;
                    shape[0, 2] = true;
                    shape[1, 2] = true;
                    return;
                case 2:
                    shape[0, 1] = true;
                    shape[1, 1] = true;
                    shape[1, 2] = true;
                    shape[2, 2] = true;
                    return;
                case 3:
                    shape[2, 1] = true;
                    shape[0, 2] = true;
                    shape[1, 2] = true;
                    shape[2, 2] = true;
                    return;
                case 4:
                    shape[1, 1] = true;
                    shape[2, 1] = true;
                    shape[1, 2] = true;
                    shape[2, 2] = true;
                    return;
                case 5:
                    shape[0, 1] = true;
                    shape[1, 1] = true;
                    shape[2, 1] = true;
                    shape[3, 1] = true;
                    return;
                case 6:
                    shape[1, 1] = true;
                    shape[0, 2] = true;
                    shape[1, 2] = true;
                    shape[2, 2] = true;
                    return;
                case 7:
                    shape[0, 1] = true;
                    shape[0, 2] = true;
                    shape[1, 2] = true;
                    shape[2, 2] = true;
                    return;
            }
        }

        public void colorShape()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (shape[i, j]) main.gameBoard[x + i, y + j] = color;
                }
            }
        }

        public void eraseShape()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (shape[i, j]) main.gameBoard[x + i, y + j] = 0;
                }
            }
        }

        public bool isOccupied()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (shape[i, j] && ((x + i < 0 || y + j < 0 || x + i > 9 || main.gameBoard[x + i, y + j] != 0))) return true;
                }
            }
            return false;
        }

        public bool isAtTop()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (shape[i, j] && y + j > 19) return true;
                }
            }
            return false;
        }

        public void moveLeft()
        {
            eraseShape();
            x--;
            if (isOccupied()) x++;
            colorShape();
        }

        public void moveRight()
        {
            eraseShape();
            x++;
            if (isOccupied()) x--;
            colorShape();
        }

        public void moveDown()
        {
            eraseShape();
            y--;
            if (isOccupied())
            {
                y++;
                atBottom = true;
            }
            colorShape();
        }

        public void rotate()
        {
            bool[,] oldShape = shape;
            bool[,] newShape = new bool[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    newShape[i, j] = oldShape[3 - j, i];
                }
            }
            eraseShape();
            shape = newShape;
            if (isOccupied()) shape = oldShape;
            colorShape();
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (shape[i, j]) s += "■";
                    else s += "□";
                }
                s += System.Environment.NewLine;
            }
            return s;
        }

    }

}
