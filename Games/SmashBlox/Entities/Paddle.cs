using System.IO;

using GameAPI;
using GameAPI.BudgetBoy;

namespace SmashBlox.Entities
{
    class Paddle : Entity
    {
        private Image _end;
        private Image _mid;

        private Sprite _leftEndSprite;
        private Sprite _rightEndSprite;
        private Sprite[] _midSprites;

        private SwatchIndex _swatch;

        private int _size;
        private bool _sizeChanged;

        public int Size
        {
            get { return _size; }
            set
            {
                _size = value;
                _sizeChanged = true;

                LocalBounds = new RectF(-Size * 4 - 2, 2, Size * 4 + 2, 0);
            }
        }

        public float NextX
        {
            get
            {
                var dx = Stage.Controls.CursorPosition.X - X;
                var maxDx = (float) (MoveSpeed * Stage.Timestep);

                if (dx < -maxDx) {
                    dx = -maxDx;
                } else if (dx > maxDx) {
                    dx = maxDx;
                }

                var margin = 8f + Size * 4f; 

                return Mathf.Clamp(X + dx, margin, Stage.Graphics.Width - margin);
            }
        }

        public float MoveSpeed { get; set; }

        public SwatchIndex SwatchIndex
        {
            get { return _leftEndSprite.SwatchIndex; }
            set
            {
                _swatch = value;

                _leftEndSprite.SwatchIndex = value;
                _rightEndSprite.SwatchIndex = value;

                foreach (var mid in _midSprites) {
                    mid.SwatchIndex = value;
                }
            }
        }

        public Paddle()
        {
            Size = 4;
            MoveSpeed = 192f;
        }

        private void Resize()
        {
            if (_leftEndSprite == null) return;

            _sizeChanged = false;

            SetSpriteOffset(_leftEndSprite, new Vector2i(-Size * 4 - _leftEndSprite.Width, -2));
            SetSpriteOffset(_rightEndSprite, new Vector2i(Size * 4, -2));

            if (_midSprites != null) {
                foreach (var sprite in _midSprites) {
                    Remove(sprite);
                }
            }

            _midSprites = new Sprite[Size];
            for (int i = 0; i < _midSprites.Length; ++i) {
                _midSprites[i] = Add(new Sprite(_mid, _swatch), new Vector2i(-Size * 4 + i * 8, -2));
            }
        }

        protected override void OnLoadGraphics(Graphics graphics)
        {
            base.OnLoadGraphics(graphics);

            _end = graphics.GetImage("Resources", "paddle", "end");
            _mid = graphics.GetImage("Resources", "paddle", "mid");

            _swatch = graphics.Palette.FindSwatch(0xffffff, 0xffffff, 0xffffff);

            _leftEndSprite = Add(new Sprite(_end, _swatch));
            _rightEndSprite = Add(new Sprite(_end, _swatch) { FlipX = true });
        }
        
        public void SaveState(BinaryWriter writer)
        {
            writer.Write(Position.X);
            writer.Write(Position.Y);

            writer.Write(_size);
        }

        public void LoadState(BinaryReader reader)
        {
            Position = new Vector2f(reader.ReadSingle(), reader.ReadSingle());

            _size = reader.ReadInt32();
            _sizeChanged = true;
        }

        protected override void OnUpdate(double dt)
        {
            base.OnUpdate(dt);

            X = NextX;
        }

        protected override void OnRender(Graphics graphics)
        {
            if (_sizeChanged) Resize();

            base.OnRender(graphics);
        }
    }
}
