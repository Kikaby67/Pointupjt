using System;
using System.IO;

public class CPHInline
{
    public bool Execute()
    {
        CPH.SendMessage("🛡️ FIREWALLER — Paladin du réseau | Tank / Soin");
        CPH.SendMessage("Stats : PV 22+1d4 | CA 15+1d4 | Mana 25 | Charisme 13 | Arme : Marteau-Rune | Dégâts : 1d8 | Bonus Atq : 0+1d4");
        CPH.SendMessage("Actions : !attaque (1d8) · !soin (1d8+3 ⭐, 5 mana) · !defense (CA+3 ce tour) · !fuir (d20 ≥ 12)");
        CPH.SendMessage("Sous-classes niv.5 — Protocole-Sacré : aura -1 dégât pour alliés · Serment-Binaire : Smite +1d8 par attaque");
        return true;
    }
}
