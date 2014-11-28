using System;
using System.Collections.Generic;
using System.IO;

using GameAPI;
using GameAPI.BudgetBoy;

namespace SmashBlox.Entities
{
    class BlockHitEventArgs : EventArgs
    {
        public int Column { get; private set; }

        public int Row { get; private set; }

        public int BlockPhase { get; private set; }

        public BlockHitEventArgs(int col, int row, int blockPhase)
        {
            Column = col;
            Row = row;

            BlockPhase = blockPhase;
        }
    }

    class BlockGrid : Entity
    {
        private Tilemap _tiles;

        private int[] _grid;

        private Image _blockImage;
        private SwatchIndex[] _blockSwatches;

        public int Phases { get { return _blockSwatches.Length; } }
        
        public int Columns { get { return _tiles.Columns; } }

        public int Rows { get { return _tiles.Rows; } }

        public event EventHandler<BlockHitEventArgs> BlockHit;

        public BlockGrid(int cols, int rows)
        {
            _tiles = new Tilemap(new Vector2i(16, 8), new Vector2i(cols, rows));
            _grid = new int[cols * rows];

            LocalBounds = new RectF(0, 0, _tiles.Width, _tiles.Height);
        }

        protected override void OnLoadGraphics(Graphics graphics)
        {
            base.OnLoadGraphics(graphics);

            _blockImage = graphics.GetImage("Resources", "block");

            _blockSwatches = new[] {
                graphics.Palette.FindSwatch(0x0000FC, 0x0078F8, 0x3CBCFC),
                graphics.Palette.FindSwatch(0x940084, 0xD800CC, 0xF878F8),
                graphics.Palette.FindSwatch(0xA81000, 0xF83800, 0xF87858),
                graphics.Palette.FindSwatch(0x503000, 0xAC7C00, 0xF8B800),
                graphics.Palette.FindSwatch(0x007800, 0x00B800, 0xB8F818)
            };
        }

        private bool IsColliding(Vector2f pos, out Vector2i block, ref int phase)
        {
            block = (Vector2i) pos;

            if (pos.X >= 0f && pos.X < Columns && pos.Y >= 0f && pos.Y <= Rows) {
                var p = this[(int) pos.X, (int) pos.Y];
            
                phase = Math.Max(p, phase);

                return p > 0;
            }

            return false;
        }

        public bool CheckForCollision(Ball ball, out Vector2f normal, out int phase)
        {
            normal = Vector2f.Zero;
            phase = 0;

            if (!ball.Bounds.Intersects(Bounds)) return false;
            
            var bounds = (ball.Bounds - Position) / new Vector2f(_tiles.TileWidth, _tiles.TileHeight);

            var collided = new List<Vector2i>();

            Vector2i pos;
            if (IsColliding(bounds.TopLeft, out pos, ref phase)) {
                normal += new Vector2f(1f, -1f);
                if (!collided.Contains(pos)) collided.Add(pos);
            }

            if (IsColliding(bounds.TopRight, out pos, ref phase)) {
                normal += new Vector2f(-1f, -1f);
                if (!collided.Contains(pos)) collided.Add(pos);
            }

            if (IsColliding(bounds.BottomLeft, out pos, ref phase)) {
                normal += new Vector2f(1f, 1f);
                if (!collided.Contains(pos)) collided.Add(pos);
            }

            if (IsColliding(bounds.BottomRight, out pos, ref phase)) {
                normal += new Vector2f(-1f, 1f);
                if (!collided.Contains(pos)) collided.Add(pos);
            }

            foreach (var tile in collided) {
                if (BlockHit != null) {
                    BlockHit(this, new BlockHitEventArgs(tile.X, tile.Y, this[tile.X, tile.Y]));
                }

                this[tile.X, tile.Y] -= 1;
            }

            if (normal.LengthSquared > 0) {
                normal = normal.Normalized;
                return true;
            }

            return false;
        }

        public int this[int col, int row]
        {
            get { return _grid[col + row * Columns]; }
            set
            {
                if (value < 0 || value > Phases) {
                    throw new ArgumentOutOfRangeException();
                }

                int index = col + row * Columns;
                if (_grid[index] == value) return;

                _grid[index] = value;

                if (value > 0) {
                    _tiles.SetTile(col, row, _blockImage, _blockSwatches[value - 1]);
                } else {
                    _tiles.ClearTile(col, row);
                }
            }
        }

        public void SaveState(BinaryWriter writer)
        {
            writer.Write(Position.X);
            writer.Write(Position.Y);

            for (int r = 0; r < Rows; ++r) {
                for (int c = 0; c < Columns; ++c) {
                    writer.Write((sbyte) this[c, r]);
                }
            }
        }

        public void LoadState(BinaryReader reader)
        {
            Position = new Vector2f(reader.ReadSingle(), reader.ReadSingle());

            for (int r = 0; r < Rows; ++r) {
                for (int c = 0; c < Columns; ++c) {
                    this[c, r] = reader.ReadSByte();
                }
            }
        }

        protected override void OnRender(Graphics graphics)
        {
            _tiles.X = Mathf.FloorToInt(X);
            _tiles.Y = Mathf.FloorToInt(Y);
            _tiles.Render(graphics);
        }
    }
}
