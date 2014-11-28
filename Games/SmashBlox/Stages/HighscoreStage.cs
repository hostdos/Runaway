using System;
using System.Collections;
using System.IO;
using System.Linq;

using GameAPI;
using GameAPI.BudgetBoy;

namespace SmashBlox.Stages
{
    class HighscoreStage : SmashBloxStage
    {
        private class Entry : Entity
        {
            private Text _rankText;
            private Text _nameText;
            private Text _scoreText;

            public int Rank { get; private set; }

            public Highscore Highscore { get; private set; }

            public SwatchIndex SwatchIndex
            {
                get { return _rankText.SwatchIndex; }
                set
                {
                    _rankText.SwatchIndex = _nameText.SwatchIndex = _scoreText.SwatchIndex = value;
                }
            }

            public Entry(int rank, Highscore score)
            {
                Rank = rank;
                Highscore = score;
            }

            protected override void OnLoadGraphics(Graphics graphics)
            {
                base.OnLoadGraphics(graphics);

                var font = graphics.GetImage("Resources", "font");
                var swatch = SwatchIndex.White;

                var width = graphics.Width * 0.6f;

                LocalBounds = new RectF(0f, 0f, width, font.Height / 16);

                _rankText = new Text(font, swatch) {
                    Value = Rank.ToString()
                };

                Add(_rankText, Vector2i.UnitX * (3 * _rankText.CharSize.X - _rankText.Width) / 2);

                _nameText = Add(new Text(font, swatch) {
                    Value = Highscore.Initials
                }, Vector2i.UnitX * 5 * _rankText.CharSize.X);

                _scoreText = new Text(font, swatch) {
                    Value = Highscore.Score.ToString("0")
                };

                Add(_scoreText, Vector2i.UnitX * Mathf.RoundToInt(width - _scoreText.Width));
            }
        }

        private Sprite _title;

        private Entry[] _entries;
        private Entry _newEntry;

        public HighscoreStage(Main game)
            : base(game)
        {
            var titleImage = Graphics.GetImage("Resources", "highscores");

            _title = Add(new Sprite(titleImage, CurrentSwatchIndex) {
                X = (Graphics.Width - titleImage.Width) / 2,
                Y = Graphics.Height - titleImage.Height - 4
            }, 0);

            int scoreCount = 10;

            int i = 0;
            _entries = Game.Highscores.Take(scoreCount).Select(x => Add(new Entry(++i, x) {
                Position = new Vector2f(Graphics.Width * 0.2f, Graphics.Height - 20 - i * 12)
            }, 0)).ToArray();

            StartCoroutine(EliminateEpileptics);
            StartCoroutine(WaitForContinueCoroutine);
        }

        public HighscoreStage(Main game, Highscore score)
            : this(game)
        {
            _newEntry = _entries.FirstOrDefault(x => x.Highscore == score);
        }

        public HighscoreStage(Main game, BinaryReader reader)
            : this(game)
        {
            LoadState(reader);
        }

        protected override void OnSaveState(BinaryWriter writer)
        {
            writer.Write(_newEntry == null ? -1 : Array.IndexOf(_entries, _newEntry));
        }

        protected override void OnLoadState(BinaryReader reader)
        {
            var entryIndex = reader.ReadInt32();

            if (entryIndex != -1) {
                _newEntry = _entries[entryIndex];
            }
        }

        protected override void OnSwatchChanged(SwatchIndex swatchIndex)
        {
            _title.SwatchIndex = swatchIndex;

            if (_newEntry != null) {
                _newEntry.SwatchIndex = swatchIndex;
            }
        }

        private IEnumerator WaitForContinueCoroutine()
        {
            yield return FadeInCoroutine(FadeDuration);
            yield return Until(() => Controls.A.JustPressed || Controls.Start.JustPressed);
            yield return FadeOutCoroutine(FadeDuration);

            Game.Reset();
        }

        private IEnumerator EliminateEpileptics()
        {
            for (;;) {
                yield return Delay(1 / 16d);
                NextSwatch();
            }
        }
    }
}
