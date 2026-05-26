using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_ENNEMIS  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_ennemis.json";
    private const string CONFIG_CLASSES  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_classes.json";

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
        string sousClasse  = LireValeur(json, "sousClasse");
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

        string cfgCls = File.ReadAllText(CONFIG_CLASSES);
        int nbAtq     = (sousClasse != "") ? int.Parse(LireValeur(cfgCls, sousClasse + "_nbAttaques")) : 0;
        if (nbAtq    == 0) nbAtq = int.Parse(LireValeur(cfgCls, classe + "_nbAttaques"));
        if (nbAtq    == 0) nbAtq = 1;

        if (nbAtq > 1)
        {
            string[] res = new string[nbAtq];
            for (int i = 0; i < nbAtq; i++)
            {
                int d20 = rng.Next(1, 21);
                if ((d20 + bonusTotal) >= ennemCA)
                {
                    int d = RollDegats(classe, sousClasse, rng);
                    totalDegats += d;
                    res[i] = "touché -" + d;
                }
                else
                {
                    res[i] = "raté";
                }
            }
            string label = (sousClasse != "") ? sousClasse : classe;
            string resStr = "[" + string.Join("] [", res) + "]";
            msgAttaque = nomJoueur + " frappe x" + nbAtq + " (" + label + ") → " + resStr + " = -" + totalDegats + " PV à " + ennemNom;
        }
        else
        {
            int d20  = rng.Next(1, 21);
            int roll = d20 + bonusTotal;
            bool touche = roll >= ennemCA;
            if (touche)
            {
                totalDegats = RollDegats(classe, sousClasse, rng);
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
            json = AjouterValeur(json, "combatsGagnes", 1);
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
            json = AjouterValeur(json, "combatsPerdus", 1);
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

    private int RollDegats(string classe, string sousClasse, Random rng)
    {
        string cfg = File.ReadAllText(CONFIG_CLASSES);
        string key = sousClasse != "" && LireValeur(cfg, sousClasse + "_degatsMax") != "0"
                     ? sousClasse : classe;
        int degatsMax = int.Parse(LireValeur(cfg, key + "_degatsMax"));
        int nbDes     = int.Parse(LireValeur(cfg, key + "_nbDes"));
        if (degatsMax == 0) degatsMax = 8;
        if (nbDes     == 0) nbDes     = 1;
        int total = 0;
        for (int i = 0; i < nbDes; i++) total += rng.Next(1, degatsMax + 1);
        return total;
    }

    private int[] GetEnnemiStats(string nom)
    {
        string cfg    = File.ReadAllText(CONFIG_ENNEMIS);
        int ca        = int.Parse(LireValeur(cfg, nom + "_ca"));
        int degatsMax = int.Parse(LireValeur(cfg, nom + "_degatsMax"));
        return new int[] { ca != 0 ? ca : 12, degatsMax != 0 ? degatsMax : 6 };
    }

    private int[] GetRecompensesEnnemi(string nom)
    {
        string cfg = File.ReadAllText(CONFIG_ENNEMIS);
        int xp  = int.Parse(LireValeur(cfg, nom + "_xp"));
        int ram = int.Parse(LireValeur(cfg, nom + "_ram"));
        return new int[] { xp != 0 ? xp : 15, ram != 0 ? ram : 3 };
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
