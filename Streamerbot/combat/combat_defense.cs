using System;
using System.IO;

// DÉPRÉCIÉ — plus de posture défensive : les rencontres se résolvent en un choix.
// Le trigger !defense peut être retiré côté Streamer.bot quand tu veux.
public class CPHInline
{
    public bool Execute()
    {
        string nomJoueur = args["user"].ToString();
        CPH.SendMessage(nomJoueur + ", la défense n'existe plus : face à une rencontre, tape !combat, !discuter ou !fuir.");
        return true;
    }
}
