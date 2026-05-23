using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";

    public bool Execute()
    {
        string nomJoueur = args["user"].ToString();
        string cheminFichier = Path.Combine(DOSSIER_JOUEURS, $"{nomJoueur}.json");

        if (!File.Exists(cheminFichier))
        {
            CPH.SendMessage(nomJoueur + ", tu n'es pas encore inscrit ! Tape !rejoindre.");
            return true;
        }

        string json = File.ReadAllText(cheminFichier);

        if (LireValeur(json, "enCombat") != "true")
        {
            CPH.SendMessage(nomJoueur + ", tu n'es pas en combat !");
            return true;
        }

        string ennemNom  = LireValeur(json, "ennemiNom");
        int joueurPV     = int.Parse(LireValeur(json, "pvActuels"));
        int joueurPVMax  = int.Parse(LireValeur(json, "pvMax"));
        int joueurCA     = int.Parse(LireValeur(json, "classeArmure"));
        int joueurCADef  = joueurCA + 3; // CA augmentée ce tour
        int tour         = int.Parse(LireValeur(json, "tourCombat"));

        int[] ennemStats = GetEnnemiStats(ennemNom);
        int ennemDiceMax = ennemStats[1];

        Random rng = new Random();

        CPH.SendMessage(nomJoueur + " prend position défensive ! CA temporaire : " + joueurCADef + " (+" + 3 + " ce tour).");

        // === RIPOSTE DE L'ENNEMI (vs CA augmentée) ===
        int d20Ennemi = rng.Next(1, 21);
        bool ennemiTouche = d20Ennemi >= joueurCADef;
        string msgRiposte;
        if (ennemiTouche)
        {
            int degatsEnnemi = rng.Next(1, ennemDiceMax + 1);
            joueurPV = Math.Max(0, joueurPV - degatsEnnemi);
            json = ModifierValeur(json, "pvActuels", joueurPV.ToString(), false);
            msgRiposte = ennemNom + " perce la défense (d20: " + d20Ennemi + " vs CA " + joueurCADef + ") → TOUCHÉ ! -" + degatsEnnemi + " PV → " + nomJoueur + " : " + joueurPV + "/" + joueurPVMax + " PV";
        }
        else
        {
            msgRiposte = ennemNom + " attaque mais la défense tient (d20: " + d20Ennemi + " vs CA " + joueurCADef + ") → BLOQUÉ !";
        }

        // === DÉFAITE ? ===
        if (joueurPV <= 0)
        {
            json = ModifierValeur(json, "enCombat", "false", false);
            json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);
            File.WriteAllText(cheminFichier, json);
            CPH.SendMessage(msgRiposte);
            CPH.SendMessage(nomJoueur + " s'effondre à 0 PV malgré la défense !");
            return true;
        }

        json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);
        File.WriteAllText(cheminFichier, json);
        CPH.SendMessage(msgRiposte);
        return true;
    }

    private int[] GetEnnemiStats(string nom)
    {
        switch (nom)
        {
            case "Insecte-Bug":           return new int[] { 8,  4 };
            case "Corbeau-Daemon":        return new int[] { 14, 6 };
            case "Castor-Rootkit":        return new int[] { 16, 6 };
            case "Loup-Firewall":         return new int[] { 15, 8 };
            case "Gobelin corrompu":      return new int[] { 12, 6 };
            case "Sentinelle du Castor":  return new int[] { 14, 6 };
            case "Ombre de la mémoire":   return new int[] { 11, 8 };
            case "Drone-racine":          return new int[] { 10, 4 };
            case "Parasite de données":   return new int[] { 12, 4 };
            default:                      return new int[] { 12, 6 };
        }
    }

    private string LireValeur(string json, string cle)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut = json.IndexOf(marqueur);
        if (posDebut == -1) return "0";
        posDebut += marqueur.Length;
        int posFin = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        return json.Substring(posDebut, posFin - posDebut).Trim().Trim('"');
    }

    private string ModifierValeur(string json, string cle, string val, bool estTexte)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut = json.IndexOf(marqueur);
        if (posDebut == -1) return json;
        posDebut += marqueur.Length;
        int posFin = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        string ancienne = json.Substring(posDebut, posFin - posDebut);
        string nouvelle = estTexte ? "\"" + val + "\"" : val;
        return json.Substring(0, posDebut) + nouvelle + json.Substring(posDebut + ancienne.Length);
    }
}
