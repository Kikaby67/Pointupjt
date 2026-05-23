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

        string classe      = LireValeur(json, "classe");
        string ennemNom    = LireValeur(json, "ennemiNom");
        int ennemPV        = int.Parse(LireValeur(json, "ennemiPVActuels"));
        int joueurPV       = int.Parse(LireValeur(json, "pvActuels"));
        int joueurPVMax    = int.Parse(LireValeur(json, "pvMax"));
        int joueurCA       = int.Parse(LireValeur(json, "classeArmure"));
        int bonusAttaque   = int.Parse(LireValeur(json, "bonusAttaque"));
        bool buffActif     = LireValeur(json, "buffActif") == "true";
        int bonusTotal     = bonusAttaque + (buffActif ? 2 : 0);
        int tour           = int.Parse(LireValeur(json, "tourCombat"));
        bool enRencontre   = LireValeur(json, "enRencontre") == "true";

        int[] ennemStats   = GetEnnemiStats(ennemNom);
        int ennemCA        = ennemStats[0];
        int ennemDiceMax   = ennemStats[1];

        Random rng = new Random();

        // === ATTAQUE DU JOUEUR ===
        string msgAttaque;
        int totalDegats = 0;

        if (classe == "Cryptolame")
        {
            // Deux attaques séparées
            int d20A = rng.Next(1, 21); bool hitA = (d20A + bonusTotal) >= ennemCA;
            int d20B = rng.Next(1, 21); bool hitB = (d20B + bonusTotal) >= ennemCA;
            int dA = hitA ? rng.Next(1, 7) : 0;
            int dB = hitB ? rng.Next(1, 7) : 0;
            totalDegats = dA + dB;
            string r1 = hitA ? "touché -" + dA : "raté";
            string r2 = hitB ? "touché -" + dB : "raté";
            msgAttaque = nomJoueur + " frappe x2 → [" + r1 + "] [" + r2 + "] = -" + totalDegats + " PV à " + ennemNom;
        }
        else
        {
            int d20 = rng.Next(1, 21);
            int roll = d20 + bonusTotal;
            bool touche = roll >= ennemCA;
            if (touche)
            {
                totalDegats = RollDegats(classe, rng);
                msgAttaque = nomJoueur + " attaque " + ennemNom + " (d20: " + d20 + "+" + bonusTotal + "=" + roll + " vs CA " + ennemCA + ") → TOUCHÉ ! -" + totalDegats + " PV";
            }
            else
            {
                msgAttaque = nomJoueur + " attaque " + ennemNom + " (d20: " + d20 + "+" + bonusTotal + "=" + roll + " vs CA " + ennemCA + ") → RATÉ !";
            }
        }

        ennemPV = Math.Max(0, ennemPV - totalDegats);
        json = ModifierValeur(json, "ennemiPVActuels", ennemPV.ToString(), false);

        // === VICTOIRE ? ===
        if (ennemPV <= 0)
        {
            int[] recompenses = GetRecompensesEnnemi(ennemNom);
            json = ModifierValeur(json, "enCombat", "false", false);
            json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);
            json = AjouterValeur(json, "experience", recompenses[0]);
            json = AjouterValeur(json, "ram", recompenses[1]);
            File.WriteAllText(cheminFichier, json);
            CPH.SendMessage(msgAttaque + ". " + ennemNom + " s'effondre !");
            string msgVictoire = nomJoueur + " remporte le combat ! +" + recompenses[0] + " XP, +" + recompenses[1] + " RAM.";
            if (enRencontre) msgVictoire += " Ta quête reprend automatiquement !";
            CPH.SendMessage(msgVictoire);
            return true;
        }

        // === RIPOSTE DE L'ENNEMI ===
        int d20Ennemi = rng.Next(1, 21);
        bool ennemiTouche = d20Ennemi >= joueurCA;
        string msgRiposte;
        if (ennemiTouche)
        {
            int degatsEnnemi = rng.Next(1, ennemDiceMax + 1);
            joueurPV = Math.Max(0, joueurPV - degatsEnnemi);
            json = ModifierValeur(json, "pvActuels", joueurPV.ToString(), false);
            msgRiposte = ennemNom + " riposte (d20: " + d20Ennemi + " vs CA " + joueurCA + ") → TOUCHÉ ! -" + degatsEnnemi + " PV → " + nomJoueur + " : " + joueurPV + "/" + joueurPVMax + " PV";
        }
        else
        {
            msgRiposte = ennemNom + " riposte (d20: " + d20Ennemi + " vs CA " + joueurCA + ") → RATÉ !";
        }

        // === DÉFAITE ? ===
        if (joueurPV <= 0)
        {
            json = ModifierValeur(json, "enCombat", "false", false);
            json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);
            File.WriteAllText(cheminFichier, json);
            CPH.SendMessage(msgAttaque + ". " + ennemNom + " : " + ennemPV + " PV restants.");
            CPH.SendMessage(msgRiposte);
            CPH.SendMessage(nomJoueur + " s'effondre à 0 PV !");
            return true;
        }

        // === COMBAT CONTINUE ===
        json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);
        File.WriteAllText(cheminFichier, json);
        CPH.SendMessage(msgAttaque + ". " + ennemNom + " : " + ennemPV + " PV restants.");
        CPH.SendMessage(msgRiposte);
        return true;
    }

    private int RollDegats(string classe, Random rng)
    {
        switch (classe)
        {
            case "Hexadécimeur":    return rng.Next(1, 9);
            case "Hackmancien":     return rng.Next(1, 11);
            case "Firewaller":      return rng.Next(1, 9);
            case "Algorythmancien": return rng.Next(1, 7);
            default:                return rng.Next(1, 7);
        }
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

    private int[] GetRecompensesEnnemi(string nom)
    {
        switch (nom)
        {
            case "Insecte-Bug":           return new int[] { 10, 2  };
            case "Corbeau-Daemon":        return new int[] { 25, 5  };
            case "Castor-Rootkit":        return new int[] { 40, 8  };
            case "Loup-Firewall":         return new int[] { 60, 12 };
            case "Gobelin corrompu":      return new int[] { 20, 4  };
            case "Sentinelle du Castor":  return new int[] { 30, 6  };
            case "Ombre de la mémoire":   return new int[] { 25, 5  };
            case "Drone-racine":          return new int[] { 15, 3  };
            case "Parasite de données":   return new int[] { 18, 4  };
            default:                      return new int[] { 15, 3  };
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

    private string AjouterValeur(string json, string cle, int montant)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut = json.IndexOf(marqueur);
        if (posDebut == -1) return json;
        posDebut += marqueur.Length;
        int posFin = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        string ancienneStr = json.Substring(posDebut, posFin - posDebut).Trim().Trim('"');
        int ancienne = int.TryParse(ancienneStr, out int v) ? v : 0;
        return json.Substring(0, posDebut) + (ancienne + montant).ToString() + json.Substring(posDebut + (posFin - posDebut));
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
