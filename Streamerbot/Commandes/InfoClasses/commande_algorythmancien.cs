using System;
using System.IO;

public class CPHInline
{
    public bool Execute()
    {
        CPH.SendMessage("🎵 ALGORYTHMANCIEN — Barde du code | Buff collectif / Soin");
        CPH.SendMessage("Stats : PV 16+1d6 | CA 11+1d4 | Mana 20 | Charisme 16 ⭐ | Arme : Luth-Code | Dégâts : 1d8 | Bonus Atq : 0+1d4");
        CPH.SendMessage("Actions : !attaque (1d8) · !soin (1d6, 5 mana) · !defense (CA+3 ce tour) · !fuir (d20 ≥ 12)");
        CPH.SendMessage("Sous-classes niv.5 — Barde-Binaire : 1d10 + buff TOUS alliés · Patch-Mélodique : soin 1d8+3");
        return true;
    }
}
