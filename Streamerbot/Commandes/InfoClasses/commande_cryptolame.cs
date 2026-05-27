using System;
using System.IO;

public class CPHInline
{
    public bool Execute()
    {
        CPH.SendMessage("🗡️ CRYPTOLAME — Assassin chiffré | DPS / Fuite");
        CPH.SendMessage("Stats : PV 16+1d6 | CA 13+1d4 | Mana 5 | Charisme 11 | Arme : Double-Dagues | Dégâts : 1d6+1d6 | Bonus Atq : 0+1d4");
        CPH.SendMessage("Actions : !attaque (2x1d6) · !soin (1d4, 5 mana) · !defense (CA+3 ce tour) · !fuir (d20 ≥ 8 ⭐)");
        CPH.SendMessage("Sous-classes niv.5 — Byte-Fantôme : 3 attaques 1d6 · Pointeur-Null : Arc-Binaire, 1d10 (1 attaque)");
        return true;
    }
}
