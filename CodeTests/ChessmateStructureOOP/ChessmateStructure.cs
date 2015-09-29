/*
Implement classes to model a classic game of chess. Required functionality:

1) The playing board (8 x 8 tiles)
	a) the board must be able to provide a list of all figures on it
2) Figures 
	a) each figure must be able to provide a list of possible moves (and attacks) given its 
	   position on the board and positions of other figures.

Design of board and figures classes is enough for this task, implementation is voluntary.
Do not implement AI, rounds/turns or any other game mechanics.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace ChessmateStructure
{
    // one Tile on board, with coordinates and can contain 1 figure.
    sealed class Tile
    {
        public uint x { private set; get; }
        public uint y { private set; get; }
        internal Figure figOnTile = null;

        public Tile(uint _x, uint _y)
        {
            x = _x;
            y = _y;
        }
    }
    // Board is class with two-dimensional array of Tiles and output data
    sealed class Board
    {
        internal Tile[,] board; // internal - because our classes must use this array and must be invisible for executable code
        public Board()
        {

            board = new Tile [8, 8];
            for (uint column = 0; column < 8; column++)
                for (uint row = 0; row < 8; row++)
                    board[row, column] = new Tile(row, column);                                    
        }
        // get output board with coordinates and figures
        // will return list of figures on board
        public List<Figure> ShowGrid()
        {
            List<Figure> figuresOnBoard = new List<Figure>();
            for (uint column = 0; column < 8; column++)
            {
                for (uint row = 0; row < 8; row++)
                {
                    Console.Write("{0},{1}:{2}\t", board[row, column].x, board[row, column].y, board[row, column].figOnTile);
                    if (board[row, column].figOnTile != null)
                        figuresOnBoard.Add(board[row, column].figOnTile);
                }
                Console.WriteLine();
            }
            return figuresOnBoard;
        }
    }
    // base class for figures, all heritable classes are overriding specific methods
    abstract class Figure
    {
        protected List<Tile> possibleMoves;
        public Tile currentTile { protected set; get; }
        private String colour;

        public Figure(String _colour, Board _board, Tile _tile)
        {            
            colour = _colour;
            _board.board[_tile.x, _tile.y].figOnTile = this;    // Add this figure on board
            currentTile = _tile;
        }

        abstract protected List<Tile> SetPossibleMoves(Board _board);    // creating and changing list of possible moves for specific figure

        public void Move(Board _board, Tile _moveTile)  // move for all figures will the same, but possible moves will be different
        {
            //if possibleMoves contain _moveTile then            
            _board.board[_moveTile.x, _moveTile.y].figOnTile = this;
            _board.board[currentTile.x, currentTile.y].figOnTile = null;
            currentTile = _moveTile;
            SetPossibleMoves(_board);
        }

        public void GetFigureColour()
        {
            Console.WriteLine(this.colour);
        }

        public List<Tile> GetPossibleMoves(Board _board)
        {
            return possibleMoves;
        }
    }
    // Heritable class for specific figure
    sealed class Knight : Figure
    {
        // default constructor 
        public Knight(String _colour, Board _board, Tile _tile) : base (_colour, _board, _tile)
        {
            SetPossibleMoves(_board);
        }

        // when the player creates or moves figure, will calculate possible moves for specific figure, checking the borders etc. 
        // will return list of Tiles for future move
        protected override List<Tile> SetPossibleMoves(Board _board)
        {            
            //possibleMoves.Add(possibleTile);
            return possibleMoves;
        }
    }
    // Heritable class for specific figure
    sealed class Pawn : Figure
    {
        // default constructor 
        public Pawn(String _colour, Board _board, Tile _tile) : base(_colour, _board, _tile)
        {
            SetPossibleMoves(_board);
        }

        // calculate possible moves for specific figure, checking the borders etc.
        // will return list of Tiles for future move
        protected override List<Tile> SetPossibleMoves(Board _board)
        {
            //possibleMoves.Add(possibleTile);
            return possibleMoves;
        }
    }
    // Heritable class for specific figure
    sealed class King : Figure
    {
        // default constructor 
        public King(String _colour, Board _board, Tile _tile) : base(_colour, _board, _tile)
        {
            SetPossibleMoves(_board);
        }

        // calculate possible moves for specific figure, checking the borders etc.
        // will return list of Tiles for future move
        protected override List<Tile> SetPossibleMoves(Board _board)
        {
            //possibleMoves.Add(possibleTile);
            return possibleMoves;
        }
    }
    // Heritable class for specific figure
    sealed class Castle : Figure
    {
        // default constructor 
        public Castle(String _colour, Board _board, Tile _tile) : base(_colour, _board, _tile)
        {
            SetPossibleMoves(_board);
        }

        // calculate possible moves for specific figure, checking the borders etc.
        // will return list of Tiles for future move
        protected override List<Tile> SetPossibleMoves(Board _board)
        {
            //possibleMoves.Add(possibleTile);
            return possibleMoves;
        }
    }
    // Heritable class for specific figure
    sealed class Bishop : Figure
    {
        // default constructor 
        public Bishop(String _colour, Board _board, Tile _tile) : base(_colour, _board, _tile)
        {
            SetPossibleMoves(_board);
        }

        // calculate possible moves for specific figure, checking the borders etc.
        // will return list of Tiles for future move
        protected override List<Tile> SetPossibleMoves(Board _board)
        {
            //possibleMoves.Add(possibleTile);
            return possibleMoves;
        }
    }
    // Heritable class for specific figure
    sealed class Queen : Figure
    {
        // default constructor 
        public Queen(String _colour, Board _board, Tile _tile) : base(_colour, _board, _tile)
        {
            SetPossibleMoves(_board);
        }

        // calculate possible moves for specific figure, checking the borders etc.
        // will return list of Tiles for future move
        protected override List<Tile> SetPossibleMoves(Board _board)
        {
            //possibleMoves.Add(possibleTile);
            return possibleMoves;
        }
    }
}
