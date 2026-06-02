using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_ENNEMIS  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_ennemis.json";
    private const string CONFIG_ITEMS    = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_items.json";
    private const string CONFIG_GLOBAL   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_global.json";

    public bool Execute()
    {
        string nomJoueur     = args["user"].ToString();
        string cheminFichier = Path.Combine(DOSSIER_JOUEURS, nomJoueur.ToLower() + ".json");

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
        string ennemNom  = LireValeur(json, "ennemiNom");
        int joueurPV     = int.Parse(LireValeur(json, "pvActuels"));
        int joueurPVMax  = int.Parse(LireValeur(json, "pvMax"));
        int joueurCA     = int.Parse(LireValeur(json, "classeArmure"));
        int caItemBonus  = GetBonusItems(json, "caBonus");
        int tour         = int.Parse(LireValeur(json, "tourCombat"));
        bool enRencontre = LireValeur(json, "enRencontre") == "true";

        int[] ennemStats = GetEnnemiStats(ennemNom);
        int ennemDiceMax = ennemStats[1];

        string cfgG    = File.ReadAllText(CONFIG_GLOBAL);
        int seuilNormal = int.Parse(LireValeur(cfgG, "combat_fuite_seuil_normal"));
        int seuilCrypto = int.Parse(LireValeur(cfgG, "combat_fuite_seuil_cryptolame"));
        int seuilFuite  = (classe == "Cryptolame") ? seuilCrypto : seuilNormal;

        Random rng = new Random();
        int d20 = rng.Next(1, 21);

        // === FUITE RÉUSSIE ===
        if (d20 >= seuilFuite)
        {
            json = ModifierValeur(json, "enCombat", "false", false);
            json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);

            if (enRencontre)
            {
                long maintenant = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long pauseDebut = long.Parse(LireValeur(json, "quetePauseDebut"));
                long totalPause = long.Parse(LireValeur(json, "queteTotalPause"));
                long dureePause = maintenant - pauseDebut;

                json = ModifierValeur(json, "enRencontre", "false", false);
                json = ModifierValeur(json, "rencontreType", "", true);
                json = ModifierValeur(json, "quetePauseDebut", "0", false);
                json = ModifierValeur(json, "queteTotalPause", (totalPause + dureePause).ToString(), false);

                File.WriteAllText(cheminFichier, json);
                CPH.SendMessage(nomJoueur + " prend ses jambes à son cou ! (d20: " + d20 + " ≥ " + seuilFuite + ") → FUITE RÉUSSIE ! Ta quête reprend depuis où tu t'étais arrêté !");
            }
            else
            {
                File.WriteAllText(cheminFichier, json);
                CPH.SendMessage(nomJoueur + " s'échappe du combat ! (d20: " + d20 + " ≥ " + seuilFuite + ") → FUITE RÉUSSIE !");
            }
            return true;
        }

        // === FUITE ÉCHOUÉE → l'ennemi riposte ===
        int    joueurCAEff  = joueurCA + caItemBonus;
        int    d20Ennemi    = rng.Next(1, 21);
        bool   ennemiTouche = d20Ennemi >= joueurCAEff;
        string msgRiposte;
        if (ennemiTouche)
        {
            int degatsEnnemi = rng.Next(1, ennemDiceMax + 1);
            joueurPV = Math.Max(0, joueurPV - degatsEnnemi);
            json = ModifierValeur(json, "pvActuels", joueurPV.ToString(), false);
            msgRiposte = ennemNom + " te rattrape (d20: " + d20Ennemi + " vs CA " + joueurCAEff + ") → TOUCHÉ ! -" + degatsEnnemi + " PV → " + nomJoueur + " : " + joueurPV + "/" + joueurPVMax + " PV";
        }
        else
        {
            msgRiposte = ennemNom + " tente de te bloquer (d20: " + d20Ennemi + " vs CA " + joueurCAEff + ") → RATÉ ! Tu t'en sors de justesse !";
        }

        CPH.SendMessage(nomJoueur + " tente de fuir... (d20: " + d20 + " < " + seuilFuite + ") → ÉCHEC !");

        // === DÉFAITE ? ===
        if (joueurPV <= 0)
        {
            json = ModifierValeur(json, "enCombat", "false", false);
            json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);
            json = AjouterValeur(json, "combatsPerdus", 1);
            File.WriteAllText(cheminFichier, json);
            CPH.SendMessage(msgRiposte);
            CPH.SendMessage(nomJoueur + " s'effondre à 0 PV en tentant de fuir !");
            return true;
        }

        json = ModifierValeur(json, "tourCombat", (tour + 1).ToString(), false);
        File.WriteAllText(cheminFichier, json);
        CPH.SendMessage(msgRiposte);
        return true;
    }

    private int GetBonusItems(string json, string stat)
    {
        string   cfgItems = File.ReadAllText(CONFIG_ITEMS);
        string[] slots    = { "armeEquipee", "armureEquipee", "accessoireEquipe" };
        int total = 0;
        foreach (string slot in slots)
        {
            string item = LireValeur(json, slot);
            if (item != "" && item != "0")
                total += int.Parse(LireValeur(cfgItems, item + "_" + stat));
        }
        return total;
    }

    private int[] GetEnnemiStats(string nom)
    {
        string cfg  = File.ReadAllText(CONFIG_ENNEMIS);
        string cfgG = File.ReadAllText(CONFIG_GLOBAL);
        int ca        = int.Parse(LireValeur(cfg, nom + "_ca"));
        int degatsMax = int.Parse(LireValeur(cfg, nom + "_degatsMax"));
        return new int[] {
            ca        != 0 ? ca        : int.Parse(LireValeur(cfgG, "ennemi_ca_defaut")),
            degatsMax != 0 ? degatsMax : int.Parse(LireValeur(cfgG, "ennemi_degats_defaut"))
        };
    }

    private string AjouterValeur(string json, string cle, int montant)
    {
        int val = int.Parse(LireValeur(json, cle));
        return ModifierValeur(json, cle, (val + montant).ToString(), false);
    }

    private string LireValeur(string json, string cle)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return "0";
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        return json.Substring(posDebut, posFin - posDebut).Trim().Trim('"');
    }

    private string ModifierValeur(string json, string cle, string val, bool estTexte)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return json;
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        string ancienne = json.Substring(posDebut, posFin - posDebut);
        string nouvelle = estTexte ? "\"" + val + "\"" : val;
        return json.Substring(0, posDebut) + nouvelle + json.Substring(posDebut + ancienne.Length);
    }
}
