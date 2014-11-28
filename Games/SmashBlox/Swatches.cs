using GameAPI.BudgetBoy;

namespace SmashBlox
{
    public class Swatches
    {
        public Swatches(PaletteBuilder palette)
        {
            palette.Add(0x0000FC, 0x0078F8, 0x3CBCFC);
            palette.Add(0x940084, 0xD800CC, 0xF878F8);
            palette.Add(0xA81000, 0xF83800, 0xF87858);
            palette.Add(0x503000, 0xAC7C00, 0xF8B800);
            palette.Add(0x007800, 0x00B800, 0xB8F818);

            var rainbow = new[] {
                0x0000FC, 0x0078F8, 0x3CBCFC,
                0x940084, 0xD800CC, 0xF878F8,
                0xA81000, 0xF83800, 0xF87858,
                0x503000, 0xAC7C00, 0xF8B800,
                0x007800, 0x00B800, 0xB8F818
            };

            foreach (var clr in rainbow) {
                palette.Add(clr, clr, clr);
            }
        }
    }
}
