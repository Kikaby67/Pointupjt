using System;
using System.IO;

public class CPHInline
{
    public bool Execute()
    {
        CPH.SendMessage("⚔️ HEXADÉCIMEUR — Guerrier du réseau | DPS corps à corps / Tank");
        CPH.SendMessage("Stats : PV 25+1d4 | CA 14+1d4 | Mana 5 | Charisme 8 | Arme : Épée | Dégâts : 1d8 | Bonus Atq : 0+1d4");
        CPH.SendMessage("Actions : !attaque (1d8) · !soin (1d4, 5 mana) · !defense (CA+3 ce tour) · !fuir (d20 ≥ 12)");
        CPH.SendMessage("Sous-classes niv.5 — Bloc-Hex : +8 PV, frappe immuable · Surcharge : 2 attaques 1d8 / -2 CA permanente");
        return true;
    }
}
