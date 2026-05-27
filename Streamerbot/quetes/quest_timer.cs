using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_ENNEMIS  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_ennemis.json";
    private const string CONFIG_ITEMS    = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_items.json";
    private const string CONFIG_QUETES   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_quetes.json";
    private const string CONFIG_LEVEL    = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_level.json";

    public bool Execute()
    {
        string[] fichiers = Directory.GetFiles(DOSSIER_JOUEURS, "*.json");
        long maintenant = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Random rng = new Random();

        foreach (string chemin in fichiers)
        {
            string json = File.ReadAllText(chemin);
            if (LireValeur(json, "enQuete") != "true") continue;

            string nomJoueur = LireValeur(json, "nomJoueur");

            // === CAS 1 : rencontre combat en cours ===
            if (LireValeur(json, "enRencontre") == "true")
            {
                if (LireValeur(json, "enCombat") == "true") continue; // combat toujours actif

                int pvActuels = int.Parse(LireValeur(json, "pvActuels"));
                long pauseDebut = long.Parse(LireValeur(json, "quetePauseDebut"));
                long totalPause = long.Parse(LireValeur(json, "queteTotalPause"));

                if (pvActuels > 0)
                {
                    // Victoire : reprendre la quête, comptabiliser la pause
                    long dureePause = maintenant - pauseDebut;
                    json = ModifierValeur(json, "enRencontre", "false", false);
                    json = ModifierValeur(json, "rencontreType", "", true);
                    json = ModifierValeur(json, "quetePauseDebut", "0", false);
                    json = ModifierValeur(json, "queteTotalPause", (totalPause + dureePause).ToString(), false);
                    File.WriteAllText(chemin, json);
                    CPH.SendMessage(nomJoueur + ", tu as vaincu l'ennemi ! Ta quête reprend là où elle s'est arrêtée !");
                }
                else
                {
                    // Défaite : quête annulée, 10 min de cooldown
                    long cooldownFin = maintenant + 10 * 60;
                    json = ModifierValeur(json, "enQuete", "false", false);
                    json = ModifierValeur(json, "enRencontre", "false", false);
                    json = ModifierValeur(json, "rencontreType", "", true);
                    json = ModifierValeur(json, "queteTicksRestants", "0", false);
                    json = ModifierValeur(json, "quetePauseDebut", "0", false);
                    json = ModifierValeur(json, "queteCooldownFin", cooldownFin.ToString(), false);
                    File.WriteAllText(chemin, json);
                    CPH.SendMessage(nomJoueur + ", tu t'es effondré face à l'ennemi... Quête abandonnée. Tu te réfugies dans l'Antre de Pointu — 10 minutes de récupération avant de repartir !");
                }
                continue;
            }

            // === CAS 2 : check rencontre toutes les 3 minutes ===
            bool encounterLancee = false;
            long dernierCheck = long.Parse(LireValeur(json, "dernierCheckRencontre"));

            if (maintenant - dernierCheck >= 180)
            {
                json = ModifierValeur(json, "dernierCheckRencontre", maintenant.ToString(), false);

                if (rng.Next(100) < 40) // 40% de chance de rencontre
                {
                    int typeRoll = rng.Next(3); // 0 = combat, 1 = événement, 2 = marchand

                    if (typeRoll == 0)
                    {
                        // Rencontre combat : pause quête + déclencher combat
                        string[] ennemis = { "Martre-Trojan", "Sentinelle du Castor", "Ombre de la mémoire", "Drone-racine", "Parasite de données", "Sanglier-Crash", "Taupe-Malware" };
                        int idx = rng.Next(ennemis.Length);
                        string cfgE  = File.ReadAllText(CONFIG_ENNEMIS);
                        int pvEnnemi = int.Parse(LireValeur(cfgE, ennemis[idx] + "_pv"));
                        if (pvEnnemi == 0) pvEnnemi = 20;

                        json = ModifierValeur(json, "enRencontre", "true", false);
                        json = ModifierValeur(json, "rencontreType", "combat", true);
                        json = ModifierValeur(json, "quetePauseDebut", maintenant.ToString(), false);
                        json = ModifierValeur(json, "enCombat", "true", false);
                        json = ModifierValeur(json, "ennemiNom", ennemis[idx], true);
                        json = ModifierValeur(json, "ennemiPVActuels", pvEnnemi.ToString(), false);
                        json = ModifierValeur(json, "tourCombat", "1", false);
                        json = ModifierValeur(json, "buffActif", "false", false);
                        json = ModifierValeur(json, "protectionActive", "false", false);
                        File.WriteAllText(chemin, json);
                        CPH.SendMessage(nomJoueur + ", un " + ennemis[idx] + " surgit sur ta route ! (" + pvEnnemi + " PV) Quête mise en pause — bats-toi !");
                        encounterLancee = true;
                    }
                    else if (typeRoll == 1)
                    {
                        // Pool de 8 événements narratifs — bitmask dans queteEventsUsed
                        int eventsUsed = int.Parse(LireValeur(json, "queteEventsUsed"));
                        int nbEvents = 8;

                        // Compter les événements disponibles (bits à 0)
                        int disponibles = 0;
                        for (int i = 0; i < nbEvents; i++)
                            if ((eventsUsed & (1 << i)) == 0) disponibles++;

                        // Si tous utilisés : réinitialiser le pool pour ce cycle
                        if (disponibles == 0) { eventsUsed = 0; disponibles = nbEvents; }

                        // Choisir le N-ème événement disponible
                        int pick = rng.Next(disponibles);
                        int choix = 0;
                        int count = 0;
                        for (int i = 0; i < nbEvents; i++)
                        {
                            if ((eventsUsed & (1 << i)) == 0)
                            {
                                if (count == pick) { choix = i; break; }
                                count++;
                            }
                        }
                        eventsUsed |= (1 << choix);
                        json = ModifierValeur(json, "queteEventsUsed", eventsUsed.ToString(), false);

                        string msg;
                        switch (choix)
                        {
                            case 0:
                                int xp0 = rng.Next(10, 26);
                                json = AjouterValeur(json, "experience", xp0);
                                msg = nomJoueur + ", tu croises un Vieux Sage d'Arbonet sur ta route. Il te transmet ses connaissances. +" + xp0 + " XP !";
                                break;
                            case 1:
                                int ram1 = rng.Next(3, 9);
                                json = AjouterValeur(json, "ram", ram1);
                                msg = nomJoueur + ", tu découvres une Source de Données intacte au pied d'un chêne-serveur. +" + ram1 + " RAM !";
                                break;
                            case 2:
                                int xp2 = rng.Next(5, 16);
                                json = AjouterValeur(json, "experience", xp2);
                                msg = nomJoueur + ", tu trouves un Fragment de Carapace de Pointu. Le savoir qu'il contient t'illumine. +" + xp2 + " XP !";
                                break;
                            case 3:
                                int ram3 = rng.Next(5, 11);
                                json = AjouterValeur(json, "ram", ram3);
                                msg = nomJoueur + ", un chêne-serveur bienveillant t'offre sa résine. +" + ram3 + " RAM !";
                                break;
                            case 4:
                                int ramAct4 = int.Parse(LireValeur(json, "ram"));
                                int malus4 = Math.Min(rng.Next(5, 15), ramAct4);
                                json = AjouterValeur(json, "ram", -malus4);
                                msg = nomJoueur + ", les sbires du Castor t'ont tendu une embuscade ! Tu perds " + malus4 + " RAM...";
                                break;
                            case 5:
                                int pvAct5 = int.Parse(LireValeur(json, "pvActuels"));
                                int malus5 = rng.Next(3, 9);
                                int nvPV5 = Math.Max(1, pvAct5 - malus5);
                                json = ModifierValeur(json, "pvActuels", nvPV5.ToString(), false);
                                msg = nomJoueur + ", un glitch du réseau te traverse violemment ! -" + (pvAct5 - nvPV5) + " PV...";
                                break;
                            case 6:
                                int ramAct6 = int.Parse(LireValeur(json, "ram"));
                                int malus6 = Math.Min(rng.Next(3, 8), ramAct6);
                                json = AjouterValeur(json, "ram", -malus6);
                                msg = nomJoueur + ", une corruption de données ronge tes ressources. -" + malus6 + " RAM...";
                                break;
                            default:
                                int pvAct7 = int.Parse(LireValeur(json, "pvActuels"));
                                int pvMax7 = int.Parse(LireValeur(json, "pvMax"));
                                int soin7 = Math.Min(rng.Next(3, 9), pvMax7 - pvAct7);
                                if (soin7 > 0)
                                {
                                    json = AjouterValeur(json, "pvActuels", soin7);
                                    msg = nomJoueur + ", du lichen cicatrisant pousse sur les racines d'Arbonet. +" + soin7 + " PV !";
                                }
                                else
                                {
                                    msg = nomJoueur + ", du lichen cicatrisant pousse sur ta route, mais tu n'as pas besoin de soins !";
                                }
                                break;
                        }
                        json = VerifierMonteeNiveau(json, nomJoueur);
                        File.WriteAllText(chemin, json);
                        CPH.SendMessage(msg);
                        json = File.ReadAllText(chemin);
                    }
                    else
                    {
                        // Marchand : soin plafonné à pvMax, pas de pause
                        int pvActuels = int.Parse(LireValeur(json, "pvActuels"));
                        int pvMax = int.Parse(LireValeur(json, "pvMax"));
                        int soin = Math.Min(rng.Next(5, 15), pvMax - pvActuels);
                        string msgMarchand;
                        if (soin > 0)
                        {
                            json = AjouterValeur(json, "pvActuels", soin);
                            msgMarchand = nomJoueur + ", tu croises un marchand ambulant qui soigne tes blessures. +" + soin + " PV !";
                        }
                        else
                        {
                            msgMarchand = nomJoueur + ", tu croises un marchand ambulant, mais tu n'as pas besoin de soins !";
                        }

                        string invMarchand = LireValeurString(json, "inventaire");
                        int nbItemsMarchand = invMarchand == "" ? 0 : invMarchand.Split(',').Length;
                        int ramMarchand = int.Parse(LireValeur(json, "ram"));
                        if (nbItemsMarchand < 8 && ramMarchand >= 30)
                        {
                            json = AjouterValeur(json, "ram", -30);
                            string nouvInvMarchand = invMarchand == "" ? "Potion-Recharge" : invMarchand + ",Potion-Recharge";
                            json = ModifierValeurString(json, "inventaire", nouvInvMarchand);
                            msgMarchand += " Il te vend une Potion-Recharge pour 30 RAM !";
                        }

                        File.WriteAllText(chemin, json);
                        CPH.SendMessage(msgMarchand);
                        json = File.ReadAllText(chemin);
                    }
                }
                else
                {
                    // Pas de rencontre, sauvegarder le nouveau dernierCheck
                    File.WriteAllText(chemin, json);
                    json = File.ReadAllText(chemin);
                }
            }

            if (encounterLancee) continue;

            // === CAS 3 : vérifier si la quête est terminée (en soustrayant les pauses) ===
            string queteId = LireValeur(json, "queteId");
            int ticksRequis = int.Parse(LireValeur(json, "queteTicksRestants"));
            long debutTimestamp = long.Parse(LireValeur(json, "queteDernierTick"));
            long totalPauseFin = long.Parse(LireValeur(json, "queteTotalPause"));
            long secondesEcoulees = (maintenant - debutTimestamp) - totalPauseFin;
            long secondesRequises = ticksRequis * 5 * 60L;

            if (secondesEcoulees < secondesRequises) continue;

            // Résoudre la quête
            string[] data = GetQueteData(queteId);
            bool succes = rng.Next(100) >= 20;

            json = ModifierValeur(json, "enQuete", "false", false);
            json = ModifierValeur(json, "queteTicksRestants", "0", false);

            if (succes)
            {
                int xp = int.Parse(data[2]);
                int ram = int.Parse(data[3]);
                json = AjouterValeur(json, "experience", xp);
                json = AjouterValeur(json, "ram", ram);
                json = AjouterValeur(json, "quetesTerminees", 1);
                json = VerifierMonteeNiveau(json, nomJoueur);

                string lootMsg = "";
                if (queteId.StartsWith("artefact_") && rng.Next(100) < 70)
                {
                    string inventaire = LireValeurString(json, "inventaire");
                    int nbItems = inventaire == "" ? 0 : inventaire.Split(',').Length;
                    if (nbItems < 8)
                    {
                        string[] lootPool = { "Ligne-Reseau", "Morceau-Arbre-Serveur", "Potion-Recharge", "Bague-de-protection", "Armure-de-feuille", "Gants-de-force" };
                        string loot = lootPool[rng.Next(lootPool.Length)];
                        string nouvInventaire = inventaire == "" ? loot : inventaire + "," + loot;
                        json = ModifierValeurString(json, "inventaire", nouvInventaire);
                        lootMsg = " Tu as trouvé : " + loot + " !";
                    }
                }

                File.WriteAllText(chemin, json);
                CPH.SendMessage(nomJoueur + ", ta quête est terminée ! Succès ! Tu gagnes " + xp + " XP et " + ram + " RAM." + lootMsg + " Bien joué aventurier !");
            }
            else
            {
                File.WriteAllText(chemin, json);
                CPH.SendMessage(nomJoueur + ", ta quête est terminée... Échec. Le destin ne t'a pas souri cette fois. Retente ta chance bientôt !");
            }
        }

        // Désactiver le timer si plus aucune quête active
        bool encoreActive = false;
        foreach (string chemin in fichiers)
        {
            if (LireValeur(File.ReadAllText(chemin), "enQuete") == "true") { encoreActive = true; break; }
        }
        if (!encoreActive) CPH.DisableTimer("QuestCheck");

        return true;
    }

    private string VerifierMonteeNiveau(string json, string nomJoueur)
    {
        int niveauActuel  = int.Parse(LireValeur(json, "niveau"));
        int nouvelXP      = int.Parse(LireValeur(json, "experience"));
        int nouveauNiveau = CalculerNiveau(nouvelXP);
        if (nouveauNiveau > niveauActuel)
        {
            json = ModifierValeur(json, "niveau", nouveauNiveau.ToString(), false);
            json = AppliquerBonusNiveau(json, nouveauNiveau);
            CPH.SendMessage(MessageNiveau(nomJoueur, nouveauNiveau));
        }
        return json;
    }

    private int CalculerNiveau(int xp)
    {
        string cfg    = File.ReadAllText(CONFIG_LEVEL);
        int niveauMax = int.Parse(LireValeur(cfg, "niveauMax"));
        for (int i = niveauMax; i >= 2; i--)
            if (xp >= int.Parse(LireValeur(cfg, "niveau_" + i + "_xp"))) return i;
        return 1;
    }

    private string AppliquerBonusNiveau(string json, int niveau)
    {
        string cfg        = File.ReadAllText(CONFIG_LEVEL);
        int pvBonus       = int.Parse(LireValeur(cfg, "niveau_" + niveau + "_pvBonus"));
        int caBonus       = int.Parse(LireValeur(cfg, "niveau_" + niveau + "_caBonus"));
        int ramBonus      = int.Parse(LireValeur(cfg, "niveau_" + niveau + "_ramBonus"));
        int charismeBonus = int.Parse(LireValeur(cfg, "niveau_" + niveau + "_charismeBonus"));
        if (pvBonus > 0)
        {
            json = AjouterValeur(json, "pvMax",     pvBonus);
            json = AjouterValeur(json, "pvActuels", pvBonus);
        }
        if (caBonus       > 0) json = AjouterValeur(json, "classeArmure", caBonus);
        if (ramBonus      > 0) json = AjouterValeur(json, "ram",          ramBonus);
        if (charismeBonus > 0) json = AjouterValeur(json, "charisme",     charismeBonus);
        return json;
    }

    private string MessageNiveau(string nomJoueur, int niveau)
    {
        string cfg   = File.ReadAllText(CONFIG_LEVEL);
        string bonus = LireValeur(cfg, "niveau_" + niveau + "_message");
        return "🎉 " + nomJoueur + " passe au niveau " + niveau + " ! " + bonus;
    }

    private string[] GetQueteData(string id)
    {
        string cfg   = File.ReadAllText(CONFIG_QUETES);
        string ticks = LireValeur(cfg, id + "_ticks");
        if (ticks == "0") return new string[] { "", "1", "0", "0" };
        string xp  = LireValeur(cfg, id + "_xp");
        string ram = LireValeur(cfg, id + "_ram");
        return new string[] { "", ticks, xp, ram };
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

    private string LireValeurString(string json, string cle)
    {
        string marqueur = "\"" + cle + "\": \"";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return "";
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOf("\"", posDebut);
        if (posFin == -1) return "";
        return json.Substring(posDebut, posFin - posDebut);
    }

    private string ModifierValeurString(string json, string cle, string val)
    {
        string marqueur = "\"" + cle + "\": \"";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return json;
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOf("\"", posDebut);
        if (posFin == -1) return json;
        return json.Substring(0, posDebut) + val + json.Substring(posFin);
    }
}
