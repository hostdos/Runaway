using System;
using System.Collections;
using System.IO;
using System.Linq;

using GameAPI;
using GameAPI.BudgetBoy;

namespace SmashBlox.Stages
{
    class EnterScoreStage : SmashBloxStage
    {
        private bool _completed;
        private int _score;

        private Sprite _header;
        private Sprite _newHighscore;

        private char[] _initialChars;

        private Text[] _chars;
        private int _curChar = 0;
        private int _curIndex = 0;

        public EnterScoreStage(Main game, bool completed, int score)
            : base(game)
        {
            _completed = completed;
            _score = score;

            _initialChars = new[] {
                'A', 'A', 'A'
            };

            Setup();
        }

        public EnterScoreStage(Main game, BinaryReader reader)
            : base(game)
        {
            LoadState(reader);
            Setup();
        }

        private void Setup()
        {
            var headerImage = _completed ? Graphics.GetImage("Resources", "yourewinner") : Graphics.GetImage("Resources", "gameover");

            _header = Add(new Sprite(headerImage, CurrentSwatchIndex) {
                X = (Graphics.Width - headerImage.Width) / 2,
                Y = Graphics.Height - headerImage.Height - 8
            }, 0);

            var font = Graphics.GetImage("Resources", "font");
            var swatch = Graphics.Palette.FindSwatch(0xffffff, 0xffffff, 0xffffff);

            var text = Add(new Text(font, swatch) {
                Value = String.Format("FINAL SCORE: {0}", _score)
            }, 0);

            text.X = (Graphics.Width - text.Width) / 2;
            text.Y = _header.Y - text.Height - 16;

            if (Game.IsScoreHighscore(_score)) {
                StartCoroutine(() => EnterInitialsCoroutine(text.Y));
            } else {
                StartCoroutine(WaitForContinueCoroutine);
            }

            StartCoroutine(EliminateEpileptics);
        }

        protected override void OnSaveState(BinaryWriter writer)
        {
            bool isHighscore = Game.IsScoreHighscore(_score);

            writer.Write(_completed);
            writer.Write(_score);

            writer.Write(isHighscore);

            writer.Write(_curChar);
            writer.Write(_curIndex);

            // these are only needed if it's a highscore
            if (isHighscore) {
                writer.Write(_chars[0].Value[0]);
                writer.Write(_chars[1].Value[0]);
                writer.Write(_chars[2].Value[0]);
            }
        }

        protected override void OnLoadState(BinaryReader reader)
        {
            _completed = reader.ReadBoolean();
            _score = reader.ReadInt32();

            bool isHighscore = reader.ReadBoolean();

            _curChar = reader.ReadInt32();
            _curIndex = reader.ReadInt32();

            // these are only needed if it's a highscore
            if (isHighscore) {
                _initialChars = new char[] {
                reader.ReadChar(), reader.ReadChar(), reader.ReadChar()
                };
            }
        }

        private static readonly char[] _sValidChars =
            new[] { '_' }
                .Concat(Enumerable.Range(0, 26)
                    .Select(x => (char) ('A' + x)))
                .Concat(Enumerable.Range(0, 10)
                    .Select(x => x.ToString()[0]))
                .ToArray();

        private IEnumerator EnterInitialsCoroutine(int y)
        {
            var font = Graphics.GetImage("Resources", "font");
            var swatch = Graphics.Palette.FindSwatch(0xffffff, 0xffffff, 0xffffff);

            var newHighscoreImage = Graphics.GetImage("Resources", "newhighscore");

            _newHighscore = Add(new Sprite(newHighscoreImage, CurrentSwatchIndex) {
                X = (Graphics.Width - newHighscoreImage.Width) / 2,
                Y = y - newHighscoreImage.Height - 16
            }, 0);

            var text = Add(new Text(font, swatch) {
                Value = "ENTER YOUR INITIALS"
            }, 0);

            text.X = (Graphics.Width - text.Width) / 2;
            text.Y = _newHighscore.Y - text.Height - 8;
            
            y = text.Y - text.CharSize.Y - 8;

            int dx = text.CharSize.X + 4;
            int x = (Graphics.Width - dx * 3) / 2 + 2;

            _chars = new Text[] {
                Add(new Text(font, swatch) { Value = _initialChars[0].ToString(), X = (x += dx) - dx, Y = y }, 0),
                Add(new Text(font, swatch) { Value = _initialChars[1].ToString(), X = (x += dx) - dx, Y = y }, 0),
                Add(new Text(font, swatch) { Value = _initialChars[2].ToString(), X = (x += dx) - dx, Y = y }, 0)
            };

            yield return FadeInCoroutine(FadeDuration);

            yield return While(() => {
                _chars[_curChar].SwatchIndex = CurrentSwatchIndex;
                _curIndex = Array.IndexOf(_sValidChars, _chars[_curChar].Value[0]);

                if (Controls.A.JustPressed || (_curChar < 2 && Controls.Analog.X.JustBecamePositive)) {
                    _chars[_curChar].SwatchIndex = swatch;
                    ++_curChar;
                } else if (_curChar > 0 && (Controls.B.JustPressed || Controls.Analog.X.JustBecameNegative)) {
                    _chars[_curChar].SwatchIndex = swatch;
                    --_curChar;
                } else if (Controls.Analog.Y.JustBecameNegative) {
                    _curIndex = (_curIndex + 1) % _sValidChars.Length;
                    _chars[_curChar].Value = _sValidChars[_curIndex].ToString();
                } else if (Controls.Analog.Y.JustBecamePositive) {
                    _curIndex = (_curIndex + _sValidChars.Length - 1) % _sValidChars.Length;
                    _chars[_curChar].Value = _sValidChars[_curIndex].ToString();
                } else if (Controls.Start.JustPressed) {
                    _curChar = 3;
                }

                return _curChar < 3;
            });

            yield return FadeOutCoroutine(FadeDuration);

            Game.ShowHighscores(new Highscore(String.Join("", _chars.Select(c => c.Value).ToArray()), _score));
        }

        private IEnumerator WaitForContinueCoroutine()
        {
            yield return FadeInCoroutine(FadeDuration);
            yield return Until(() => Controls.A.JustPressed || Controls.Start.JustPressed);
            yield return FadeOutCoroutine(FadeDuration);

            Game.ShowHighscores();
        }

        protected override void OnSwatchChanged(SwatchIndex swatchIndex)
        {
            _header.SwatchIndex = swatchIndex;

            if (_newHighscore != null) {
                _newHighscore.SwatchIndex = swatchIndex;
            }
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
