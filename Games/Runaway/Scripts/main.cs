using System.Collections.Generic;
using System.IO;
using System.Linq;

using GameAPI;
using GameAPI.BudgetBoy;
using ResourceLibrary;

namespace Games.Runaway
{
    [GameInfo(
        Title = "Runaway",
        AuthorName = "hostdos",
        AuthorContact = "dominiquehostettler@gmail.com",
        Description = "Run away from the guys",
        UpdateRate = 60
    )]
    [GraphicsInfo(Width = 256, Height = 192)]

    public class Main : Game
    {
        public Swatches Swatches { get; private set; }

        protected override void OnReset()
        {
            SetStage(new GameStage(this));
        }

        protected override void OnLoadPalette(PaletteBuilder builder)
        {
            Swatches = new Swatches(builder);
        }

    }



}
