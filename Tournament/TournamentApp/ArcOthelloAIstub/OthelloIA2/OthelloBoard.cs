using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAJeannetAlfred
{

    // Tile states
    public enum TileState
    {
        EMPTY = -1,
        WHITE = 0,
        BLACK = 1
    }

    public class OthelloBoard : IPlayable.IPlayable
    {
        const int BOARDSIZE_X = 9;
        const int BOARDSIZE_Y = 7;

        int[,] theBoard = new int[BOARDSIZE_X, BOARDSIZE_Y];
        int whiteScore = 0;
        int blackScore = 0;
        public bool GameFinish { get; set; }

        public bool GameGhostFinish { get; set; }

        private Random rnd = new Random();

        public OthelloBoard()
        {
            initBoard();
        }


        public void DrawBoard()
        {
            Console.WriteLine("REFERENCE" + "\tBLACK [X]:" + blackScore + "\tWHITE [O]:" + whiteScore);
            Console.WriteLine("  A B C D E F G H I");
            for (int line = 0; line < BOARDSIZE_Y; line++)
            {
                Console.Write($"{(line + 1)}");
                for (int col = 0; col < BOARDSIZE_X; col++)
                {
                    Console.Write((theBoard[col, line] == (int)IAJeannetAlfred.TileState.EMPTY) ? " -" : (theBoard[col, line] == (int)IAJeannetAlfred.TileState.WHITE) ? " O" : " X");
                }
                Console.Write("\n");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Returns the board game as a 2D array of int
        /// with following values
        /// -1: empty
        ///  0: white
        ///  1: black
        /// </summary>
        /// <returns></returns>
        public int[,] GetBoard()
        {
            return (int[,])theBoard;
        }

        #region IPlayable
        public int GetWhiteScore() { return whiteScore; }
        public int GetBlackScore() { return blackScore; }
        public string GetName() { return "IAlfredino"; }
        public bool isWhite = true;
        public int turn = 0;

        /// <summary>
        /// plays randomly amon the possible moves
        /// </summary>
        /// <param name="game"></param>
        /// <param name="level"></param>
        /// <param name="whiteTurn"></param>
        /// <returns>The move it will play, will return {P,0} if it has to PASS its turn (no move is possible)</returns>
        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {
            isWhite = whiteTurn;
            List<Tuple<int, int>> possibleMoves = GetPossibleMove(isWhite);
            if (possibleMoves.Count == 0)
                return new Tuple<int, int>(-1, -1);
            else
            {
                turn++;
                return alphabeta(possibleMoves, game, level);
            }

        }

        /// <summary>
        /// compute alphabeta algorithm on current game state. Main method of alphabeta
        /// </summary>
        /// <param name="possibleMoves"></param>
        /// <param name="game"></param>
        /// <param name="depth"></param>
        /// <returns>The best move to play according to alphabeta algorithm</returns>
        private Tuple<int, int> alphabeta(List<Tuple<int, int>> possibleMoves, int[,] game, int depth)
        {
            var board = game.Clone() as int[,];
            Tuple<int, Tuple<int, int>> result = Ab_max(board, possibleMoves, 1000000, depth);
            return result.Item2;
        }

        /// <summary>
        /// Minimum method of alphabeta algorithm. Compute the "opponent" move
        /// </summary>
        /// <param name="board"></param>
        /// <param name="childrens"></param>
        /// <param name="parentMax"></param>
        /// <param name="depth"></param>
        /// <returns>return tuple composed of the minimum score and the move to obtain this score</returns>
        public Tuple<int, Tuple<int, int>> Ab_min(int[,] board, List<Tuple<int, int>> childrens, int parentMax, int depth)
        {
            if (depth == 0 || GameGhostFinish)
            {
                return Eval(board);
            }
            int minVal = 5000;
            Tuple<int, int> minOp = null;
            foreach (var move in childrens)
            {
                var childrenBoard = board.Clone() as int[,];
                PlayGhostMove(childrenBoard, move.Item1, move.Item2, !isWhite);
                List<Tuple<int, int>> possibleMoves = GetPossibleGhostMove(childrenBoard, !isWhite);
                Tuple<int, Tuple<int, int>> result = Ab_max(childrenBoard, possibleMoves, minVal, depth - 1);
                int score = result.Item1;
                if (score < minVal)
                {
                    minVal = score;
                    minOp = move;
                    if (minVal > parentMax)
                        break;
                }
            }
            return new Tuple<int, Tuple<int, int>>(minVal, minOp);
        }

        /// <summary>
        /// Maximum method of alphabeta algorithm. Compute the "player"'s move
        /// </summary>
        /// <param name="board"></param>
        /// <param name="childrens"></param>
        /// <param name="parentMin"></param>
        /// <param name="depth"></param>
        /// <returns>return tuple composed of the maximum score and the move to obtain this score</returns>
        public Tuple<int, Tuple<int, int>> Ab_max(int[,] board, List<Tuple<int, int>> childrens, int parentMin, int depth)
        {
            if (depth == 0 || GameGhostFinish)
            {
                return Eval(board);
            }
            int maxVal = -5000;
            Tuple<int, int> maxOp = null;
            foreach (var move in childrens)
            {
                var childrenBoard = board.Clone() as int[,];
                PlayGhostMove(childrenBoard, move.Item1, move.Item2, isWhite);
                List<Tuple<int, int>> possibleMoves = GetPossibleGhostMove(childrenBoard, isWhite);
                Tuple<int, Tuple<int, int>> result = Ab_min(childrenBoard, possibleMoves, maxVal, depth - 1);
                int score = result.Item1;
                if (score > maxVal)
                {
                    maxVal = score;
                    maxOp = move;
                    if (maxVal > parentMin)
                        break;
                }
            }
            return new Tuple<int, Tuple<int, int>>(maxVal, maxOp);
        }

        private int[,] evaluationArray = new int[9, 7]
        {
            {100,-50, 30, 10, 30,-50, 100},
            {-50,-60,  0,  0,  0,-60, -50},
            { 30,  0, 10, 10, 10,  0,  30},
            { 10,  0, 10, 10, 10,  0,  10},
            { 30,  0, 10, 10, 10,  0,  30},
            { 10,  0, 10, 10, 10,  0,  10},
            { 30,  0, 10, 10, 10,  0,  30},
            {-50,-60,  0,  0,  0,-60, -50},
            {100,-50, 30, 10, 30,-50, 100},
        };

        /// <summary>
        /// Evaluation function of alphabeta algorithm. Evaluate the score of the state of the board passed in params
        /// </summary>
        /// <param name="board"></param>
        /// <returns>the score of the board</returns>
        public Tuple<int, Tuple<int, int>> Eval(int[,] board)
        {
            int score = 0;
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;

            int ownScore = 0;
            int opponentScore = 0;
            //Compute the score (number of disc) for each player
            foreach (var v in board)
            {
                if (v == (int)ownColor)
                    ownScore++;
                else if (v == (int)opponent)
                    opponentScore++;
            }

            if (turn <= 25) // Heuristic for the First 25 turn
            {
                var evalTab = evaluationArray.Clone() as int[,];
                ApplyCornerStrategy(board, evalTab, opponent, ownColor);

                //apply weighted array 
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        if (board[i, j] == (int)ownColor)
                            score += evalTab[i, j];
                    }
                }
                //Minimize number of disc during 25 first turn of the game
                score -= ownScore;
            }
            else // after 25 turn, heuristic function becom only the number of discs
            {
                //Maximise the number of disc
                score = ownScore;
            }

            return new Tuple<int, Tuple<int, int>>((int)score, new Tuple<int, int>(0, 0));
        }

        /// <summary>
        /// Called in evaluation method. Update the weighted array according to disc positions in the corners
        /// </summary>
        /// <param name="board"></param>
        /// <param name="evaluationArray"></param>
        /// <param name="opponent"></param>
        /// <param name="ownColor"></param>
        public void ApplyCornerStrategy(int[,] board, int[,] evaluationArray, TileState opponent, TileState ownColor)
        {
            //IF...ELSE : update top left zone depending which disc is in the corner
            if (board[0, 0] == (int)ownColor)
            {
                evaluationArray[0, 1] = 100;
                evaluationArray[1, 0] = 100;
                evaluationArray[1, 1] = 50;
            }
            else if (board[0, 0] == (int)opponent)
            {
                evaluationArray[0, 1] = 0;
                evaluationArray[1, 0] = 0;
                evaluationArray[1, 1] = 0;
                evaluationArray[0, 2] = 0;
                evaluationArray[2, 0] = 0;
            }
            //IF...ELSE : update bottom right zone depending which disc is in the corner
            if (board[8, 6] == (int)ownColor)
            {
                evaluationArray[7, 6] = 100;
                evaluationArray[8, 5] = 100;
                evaluationArray[7, 5] = 50;
            }
            else if (board[8, 6] == (int)opponent)
            {
                evaluationArray[7, 6] = 0;
                evaluationArray[8, 5] = 0;
                evaluationArray[7, 5] = 0;
                evaluationArray[6, 6] = 0;
                evaluationArray[8, 4] = 0;
            }
            //IF...ELSE : update bottom left zone depending which disc is in the corner
            if (board[0, 6] == (int)ownColor)
            {
                evaluationArray[1, 6] = 100;
                evaluationArray[0, 5] = 100;
                evaluationArray[1, 5] = 50;
            }
            else if (board[0, 6] == (int)opponent)
            {
                evaluationArray[1, 6] = 0;
                evaluationArray[0, 5] = 0;
                evaluationArray[1, 5] = 0;
                evaluationArray[2, 6] = 0;
                evaluationArray[1, 4] = 0;
            }
            //IF...ELSE : update top right zone depending which disc is in the corner
            if (board[8, 0] == (int)ownColor)
            {
                evaluationArray[8, 1] = 100;
                evaluationArray[7, 0] = 100;
                evaluationArray[7, 1] = 50;
            }
            else if (board[8, 0] == (int)opponent)
            {
                evaluationArray[8, 1] = 0;
                evaluationArray[7, 0] = 0;
                evaluationArray[7, 1] = 0;
                evaluationArray[8, 2] = 0;
                evaluationArray[6, 0] = 0;
            }
        }

        /// <summary>
        /// Simulate a player's move without acutally making it on the real game
        /// </summary>
        /// <param name="board"></param>
        /// <param name="column"></param>
        /// <param name="line"></param>
        /// <param name="isWhite"></param>
        /// <returns>Return true if no error</returns>
        public bool PlayGhostMove(int[,] board, int column, int line, bool isWhite)
        {
            //0. Verify if indices are valid
            if ((column < 0) || (column >= BOARDSIZE_X) || (line < 0) || (line >= BOARDSIZE_Y))
                return false;
            //1. Verify if it is playable
            if (IsGhostPlayable(board, column, line, isWhite) == false)
                return false;

            //2. Create a list of directions {dx,dy,length} where tiles are flipped
            int c = column, l = line;
            bool playable = false;
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();

            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (board[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0))
                                   && (board[c, l] == (int)opponent)) // pour éviter les trous
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (board[c, l] == (int)ownColor)
                            {
                                playable = true;
                                board[column, line] = (int)ownColor;
                                catchDirections.Add(new Tuple<int, int, int>(dCol, dLine, counter));
                            }
                        }
                    }
                }
            }
            // 3. Flip ennemy tiles
            foreach (var v in catchDirections)
            {
                int counter = 0;
                l = line;
                c = column;
                while (counter++ < v.Item3)
                {
                    c += v.Item1;
                    l += v.Item2;
                    board[c, l] = (int)ownColor;
                }
            }
            computeGhostScore(board);
            return playable;
        }

        /// <summary>
        /// Compute simulated score. update de gameghostfinish value
        /// </summary>
        /// <param name="board"></param>
        private void computeGhostScore(int[,] board)
        {
            whiteScore = 0;
            blackScore = 0;
            foreach (var v in board)
            {
                if (v == (int)TileState.WHITE)
                    whiteScore++;
                else if (v == (int)TileState.BLACK)
                    blackScore++;
            }
            GameGhostFinish = ((whiteScore == 0) || (blackScore == 0) ||
                        (whiteScore + blackScore == 63));
        }


        /// <summary>
        /// Returns all the playable moves in the simulation in computer readable way
        /// </summary>
        /// <param name="board"></param>
        /// <param name="whiteTurn"></param>
        /// <param name="show"></param>
        /// <returns></returns>
        public List<Tuple<int, int>> GetPossibleGhostMove(int[,] board, bool whiteTurn, bool show = false)
        {
            char[] colonnes = "ABCDEFGHIJKL".ToCharArray();
            List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++)
                {
                    if (IsGhostPlayable(board, i, j, whiteTurn))
                    {
                        possibleMoves.Add(new Tuple<int, int>(i, j));
                        if (show == true)
                            Console.Write((colonnes[i]).ToString() + (j + 1).ToString() + ", ");
                    }
                }
            return possibleMoves;
        }
        /// <summary>
        /// Check if a move is playable or not in the simulation.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="column"></param>
        /// <param name="line"></param>
        /// <param name="isWhite"></param>
        /// <returns>true if move is playable</returns>
        public bool IsGhostPlayable(int[,] board, int column, int line, bool isWhite)
        {
            //1. Verify if the tile is empty !
            if (board[column, line] != (int)TileState.EMPTY)
                return false;
            //2. Verify if at least one adjacent tile has an opponent tile
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            int c = column, l = line;
            bool playable = false;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();
            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (board[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0)))
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (board[c, l] == (int)ownColor)
                            {
                                playable = true;
                                break;
                            }
                            else if (board[c, l] == (int)opponent)
                                continue;
                            else if (board[c, l] == (int)TileState.EMPTY)
                                break;  //empty slot ends the search
                        }
                    }
                }
            }
            return playable;
        }

        public bool PlayMove(int column, int line, bool isWhite)
        {
            //0. Verify if indices are valid
            if ((column < 0) || (column >= BOARDSIZE_X) || (line < 0) || (line >= BOARDSIZE_Y))
                return false;
            //1. Verify if it is playable
            if (IsPlayable(column, line, isWhite) == false)
                return false;

            //2. Create a list of directions {dx,dy,length} where tiles are flipped
            int c = column, l = line;
            bool playable = false;
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();

            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (theBoard[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0))
                                   && (theBoard[c, l] == (int)opponent)) // pour éviter les trous
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (theBoard[c, l] == (int)ownColor)
                            {
                                playable = true;
                                theBoard[column, line] = (int)ownColor;
                                catchDirections.Add(new Tuple<int, int, int>(dCol, dLine, counter));
                            }
                        }
                    }
                }
            }
            // 3. Flip ennemy tiles
            foreach (var v in catchDirections)
            {
                int counter = 0;
                l = line;
                c = column;
                while (counter++ < v.Item3)
                {
                    c += v.Item1;
                    l += v.Item2;
                    theBoard[c, l] = (int)ownColor;
                }
            }
            //Console.WriteLine("CATCH DIRECTIONS:" + catchDirections.Count);
            computeScore();
            return playable;
        }

        /// <summary>
        /// More convenient overload to verify if a move is possible
        /// </summary>
        /// <param name=""></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool IsPlayable(Tuple<int, int> move, bool isWhite)
        {
            return IsPlayable(move.Item1, move.Item2, isWhite);
        }

        public bool IsPlayable(int column, int line, bool isWhite)
        {
            //1. Verify if the tile is empty !
            if (theBoard[column, line] != (int)TileState.EMPTY)
                return false;
            //2. Verify if at least one adjacent tile has an opponent tile
            TileState opponent = isWhite ? TileState.BLACK : TileState.WHITE;
            TileState ownColor = (!isWhite) ? TileState.BLACK : TileState.WHITE;
            int c = column, l = line;
            bool playable = false;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();
            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c < BOARDSIZE_X) && (c >= 0) && (l < BOARDSIZE_Y) && (l >= 0)
                        && (theBoard[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) < BOARDSIZE_X) && (c + dCol >= 0) &&
                                  ((l + dLine) < BOARDSIZE_Y) && ((l + dLine >= 0)))
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (theBoard[c, l] == (int)ownColor)
                            {
                                playable = true;
                                break;
                            }
                            else if (theBoard[c, l] == (int)opponent)
                                continue;
                            else if (theBoard[c, l] == (int)TileState.EMPTY)
                                break;  //empty slot ends the search
                        }
                    }
                }
            }
            return playable;
        }
        #endregion

        /// <summary>
        /// Returns all the playable moves in a human readable way (e.g. "G3")
        /// </summary>
        /// <param name="v"></param>
        /// <param name="whiteTurn"></param>
        /// <returns></returns>
        public List<Tuple<char, int>> GetPossibleMoves(bool whiteTurn, bool show = false)
        {
            char[] colonnes = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            List<Tuple<char, int>> possibleMoves = new List<Tuple<char, int>>();
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++)
                {
                    if (IsPlayable(i, j, whiteTurn))
                    {
                        possibleMoves.Add(new Tuple<char, int>(colonnes[i], j + 1));
                        if (show == true)
                            Console.Write((colonnes[i]).ToString() + (j + 1).ToString() + ", ");
                    }
                }
            return possibleMoves;
        }



        /// <summary>
        /// Returns all the playable moves in a computer readable way (e.g. "<3, 0>")
        /// </summary>
        /// <param name="v"></param>
        /// <param name="whiteTurn"></param>
        /// <returns></returns>
        public List<Tuple<int, int>> GetPossibleMove(bool whiteTurn, bool show = false)
        {
            char[] colonnes = "ABCDEFGHIJKL".ToCharArray();
            List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++)
                {
                    if (IsPlayable(i, j, whiteTurn))
                    {
                        possibleMoves.Add(new Tuple<int, int>(i, j));
                        if (show == true)
                            Console.Write((colonnes[i]).ToString() + (j + 1).ToString() + ", ");
                    }
                }
            return possibleMoves;
        }

        private void initBoard()
        {
            for (int i = 0; i < BOARDSIZE_X; i++)
                for (int j = 0; j < BOARDSIZE_Y; j++)
                    theBoard[i, j] = (int)TileState.EMPTY;

            theBoard[3, 3] = (int)TileState.WHITE;
            theBoard[4, 4] = (int)TileState.WHITE;
            theBoard[3, 4] = (int)TileState.BLACK;
            theBoard[4, 3] = (int)TileState.BLACK;

            computeScore();
        }

        private void computeScore()
        {
            whiteScore = 0;
            blackScore = 0;
            foreach (var v in theBoard)
            {
                if (v == (int)TileState.WHITE)
                    whiteScore++;
                else if (v == (int)TileState.BLACK)
                    blackScore++;
            }
            GameFinish = ((whiteScore == 0) || (blackScore == 0) ||
                        (whiteScore + blackScore == 63));
        }
    }

}