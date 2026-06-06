using System;
using System.IO;

// DÉPRÉCIÉ — le combat tour par tour a été remplacé par les rencontres à choix unique.
// Le trigger !attaque peut être retiré côté Streamer.bot quand tu veux.
public class CPHInline
{
    public bool Execute()
    {
        string nomJoueur = args["user"].ToString();
        CPH.SendMessage(nomJoueur + ", le combat a changé ! Face à une rencontre, tape !combat, !discuter ou !fuir. (le soin est désormais hors combat : !soin / !repos / !utiliser Potion)");
        return true;
    }
}
