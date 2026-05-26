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

        string classe    = LireValeur(json, "classe");
        string sousClasse = LireValeur(json, "sousClasse");
        string ennemNom  = LireValeur(json, "ennemiNom");
        int joueurPV     = int.Parse(LireValeur(json, "pvActuels"));
        int joueurPVMax  = int.Parse(LireValeur(json, "pvMax"));
        int joueurCA     = int.Parse(LireValeur(json, "classeArmure"));
        int mana         = int.Parse(LireValeur(json, "manaActuels"));
        int tour         = int.Parse(LireValeur(json, "tourCombat"));

        int[] ennemStats = GetEnnemiStats(ennemNom);
        int ennemDiceMax = ennemStats[1];

        Random rng = new Random();
        string msgSoin;

        // === SOIN ===
        if (mana < 5)
        {
            msgSoin = nomJoueur + ", pas assez de mana (" + mana + "/5 requis) ! Le soin échoue.";
        }
        else
        {
            int soinRoll = RollSoin(classe, sousClasse, rng);
            int nouveauPV = Math.Min(joueurPV + soinRoll, joueurPVMax);
            int soinsEffectifs = nouveauPV - joueurPV;
            json = ModifierValeur(json, "pvActuels", nouveauPV.ToString(), false);
            json = ModifierValeur(json, "manaActuels", (mana - 5).ToString(), false);
            joueurPV = nouveauPV;
            msgSoin = nomJoueur + " se soigne +" + soinsEffectifs + " PV → " + joueurPV + "/" + joueurPVMax + " PV (" + (mana - 5) + " mana restant)";
        }

        // === RIPOSTE DE L'ENNEMI (toujours, même si soin raté) ===
        int d20Ennemi = rng.Next(1, 21);
        bool ennemiTouche = d20Ennemi >= joueurCA;
        string msgRiposte;
        if (ennemiTouche)
        {
            int degatsEnnemi = rng.Next(1, ennemDiceMax + 1);
            joueurPV = Math.Max(0, joueurPV - degatsEnnemi);
            json = ModifierValeur(json, "pvActuels", joueurPV.ToString(), false);
            msgRiposte = ennemNom + " profite de l'ouverture (d20: " + d20Ennemi + " vs CA " + joueurCA + ") → TOUCHÉ ! -" + degatsEnnemi + " PV → " + nomJoueur + " : " + joueurPV + "/" + joueurPVMax + " PV";
        }
        else
        {
            msgRiposte = ennemNom + " tente de frapper pendant le soin (d20: " + d20Ennemi + " vs CA " + joueurCA + ") → RATÉ !";
        }

        // === DÉFAITE ? ===
        if (joueurPV <= 0)
        {
            json = ModifierValeur(json, "enCombat", "false", false);
            json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);
            json = AjouterValeur(json, "combatsPerdus", 1);
            File.WriteAllText(cheminFichier, json);
            CPH.SendMessage(msgSoin);
            CPH.SendMessage(msgRiposte);
            CPH.SendMessage(nomJoueur + " s'effondre à 0 PV !");
            return true;
        }

        json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);
        File.WriteAllText(cheminFichier, json);
        CPH.SendMessage(msgSoin);
        CPH.SendMessage(msgRiposte);
        return true;
    }

    private int RollSoin(string classe, string sousClasse, Random rng)
    {
        string cfg    = File.ReadAllText(CONFIG_CLASSES);
        string key    = (sousClasse != "" && LireValeur(cfg, sousClasse + "_soinMax") != "0") ? sousClasse : classe;
        int soinMax   = int.Parse(LireValeur(cfg, key + "_soinMax"));
        int soinBonus = int.Parse(LireValeur(cfg, key + "_soinBonus"));
        if (soinMax == 0) soinMax = 4;
        return rng.Next(1, soinMax + 1) + soinBonus;
    }

    private int[] GetEnnemiStats(string nom)
    {
        string cfg    = File.ReadAllText(CONFIG_ENNEMIS);
        int ca        = int.Parse(LireValeur(cfg, nom + "_ca"));
        int degatsMax = int.Parse(LireValeur(cfg, nom + "_degatsMax"));
        return new int[] { ca != 0 ? ca : 12, degatsMax != 0 ? degatsMax : 6 };
    }

    private string AjouterValeur(string json, string cle, int montant)
    {
        int val = int.Parse(LireValeur(json, cle));
        return ModifierValeur(json, cle, (val + montant).ToString(), false);
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
