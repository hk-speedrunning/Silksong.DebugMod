using System.Diagnostics.CodeAnalysis;

namespace DebugMod.VersionInterop;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class Extensions {
    extension(PlayerData playerData)
    {
#if SILKSONG
        public int manaMax
        {
            get => playerData.silkMax;
            set => playerData.silkMax = value;
        }
        public int mana
        {
            get => playerData.silk;
            set => playerData.silk = value;
        }
        public int CurrentManaMax => playerData.CurrentSilkMaxBasic;
        public int manaFragments
        {
            get => playerData.silkSpoolParts;
            set => playerData.silkSpoolParts = value;
        }
#else
        public int manaMax
        {
            get => playerData.maxMP;
            set => playerData.maxMP = value;
        }
        public int mana
        {
            get => playerData.MPCharge;
            set => playerData.MPCharge = value;
        }
        public int CurrentManaMax => playerData.maxMP;
        public int manaFragments
        {
            get => playerData.vesselFragments;
            set => playerData.vesselFragments = value;
        }
#endif
    }

    extension(HeroController hero)
    {
#if SILKSONG
        public void AddToMaxMana(int amount) => hero.AddToMaxSilk(amount);
        public void TakeMana(int amount) => hero.TakeSilk(amount);
        public void AddMana(int amount, bool heroEffect) => hero.AddSilk(amount, heroEffect);
#else
        public void AddToMaxMana(int amount) => hero.AddToMaxMPReserve(amount);
        public void TakeMana(int amount) => hero.TakeMP(amount);
        public void AddMana(int amount, bool heroEffect) => hero.AddMPCharge(amount);
#endif
    }
}
