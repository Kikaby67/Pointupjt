using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_ENNEMIS  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_ennemis.json";
    private const string CONFIG_ITEMS    = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_items.json";
    private const string CONFIG_QUETES   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_quetes.json";
    private const string CONFIG_LEVEL    = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_level.json";
    private const string CONFIG_ALLIES   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_allies.json";
    private const string CONFIG_GLOBAL   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_global.json";

    public bool Execute()
    {
        string[] fichiers = Directory.GetFiles(DOSSIER_JOUEURS, "*.json");
        long maintenant   = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Random rng        = new Random();
        string cfgAllies  = File.ReadAllText(CONFIG_ALLIES);
        string cfgG       = File.ReadAllText(CONFIG_GLOBAL);
        int maxSac               = int.Parse(LireValeur(cfgG, "max_sac"));
        int chanceRencontre      = int.Parse(LireValeur(cfgG, "quete_chance_rencontre"));
        int tauxEchec            = int.Parse(LireValeur(cfgG, "quete_taux_echec"));
        int chanceLootArtefact   = int.Parse(LireValeur(cfgG, "quete_chance_loot_artefact"));
        int chanceEcorce         = int.Parse(LireValeur(cfgG, "quete_chance_ecorce"));
        int cooldownDefaite      = int.Parse(LireValeur(cfgG, "quete_cooldown_defaite_secondes"));

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
                    long cooldownFin = maintenant + cooldownDefaite;
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

            // === CHECK OFFRE EXPIRÉE ===
            string offreEnCours = LireValeurString(json, "offreEnAttente");
            long   offreExp     = long.Parse(LireValeur(json, "offreExpire"));
            if (offreEnCours != "" && offreExp > 0 && maintenant > offreExp)
            {
                string offreNomMsg = offreEnCours == "vieux_sage" ? "du Vieux Sage" : "du marchand";
                json = ModifierValeur(json, "offreEnAttente", "", true);
                json = ModifierValeur(json, "offreValeur",    "0", false);
                json = ModifierValeur(json, "offreExpire",    "0", false);
                offreEnCours = "";
                File.WriteAllText(chemin, json);
                CPH.SendMessage(nomJoueur + ", l'offre " + offreNomMsg + " a expiré avant que tu ne répondes...");
                json = File.ReadAllText(chemin);
            }

            // === CAS 2 : check rencontre toutes les 3 minutes ===
            bool encounterLancee = false;
            long dernierCheck = long.Parse(LireValeur(json, "dernierCheckRencontre"));

            if (maintenant - dernierCheck >= 180 && offreEnCours == "")
            {
                json = ModifierValeur(json, "dernierCheckRencontre", maintenant.ToString(), false);

                if (rng.Next(100) < chanceRencontre)
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
                                int xp0 = rng.Next(int.Parse(LireValeur(cfgAllies, "vieux_sage_xp_min")), int.Parse(LireValeur(cfgAllies, "vieux_sage_xp_max")) + 1);
                                long expSage = long.Parse(LireValeur(cfgAllies, "vieux_sage_expiration"));
                                json = ModifierValeur(json, "offreEnAttente", "vieux_sage", true);
                                json = ModifierValeur(json, "offreValeur",    xp0.ToString(), false);
                                json = ModifierValeur(json, "offreExpire",    (maintenant + expSage).ToString(), false);
                                string[] scenariosSage = {
                                    nomJoueur + ", un Vieux Sage t'interpelle et te propose son marché — sagesse contre passage. !accepter ou !refuser (2 min) !",
                                    nomJoueur + ", un Vieux Sage surgit de la brume et engage la conversation. Il semble avoir quelque chose à t'offrir. !accepter ou !refuser (2 min) !",
                                    nomJoueur + ", un Vieux Sage apparaît sur ta route, silencieux. Il tend la main vers toi. !accepter ou !refuser (2 min) !"
                                };
                                msg = scenariosSage[rng.Next(scenariosSage.Length)];
                                break;
                            case 1:
                                int ram1 = rng.Next(int.Parse(LireValeur(cfgAllies, "source_ram_min")), int.Parse(LireValeur(cfgAllies, "source_ram_max")) + 1);
                                json = AjouterValeur(json, "ram", ram1);
                                msg = nomJoueur + ", tu découvres une Source de Données intacte au pied d'un chêne-serveur. +" + ram1 + " RAM !";
                                break;
                            case 2:
                                int xp2 = rng.Next(int.Parse(LireValeur(cfgAllies, "fragment_xp_min")), int.Parse(LireValeur(cfgAllies, "fragment_xp_max")) + 1);
                                json = AjouterValeur(json, "experience", xp2);
                                msg = nomJoueur + ", tu trouves un Fragment de Carapace de Pointu. Le savoir qu'il contient t'illumine. +" + xp2 + " XP !";
                                break;
                            case 3:
                                int ram3 = rng.Next(int.Parse(LireValeur(cfgAllies, "chene_ram_min")), int.Parse(LireValeur(cfgAllies, "chene_ram_max")) + 1);
                                json = AjouterValeur(json, "ram", ram3);
                                msg = nomJoueur + ", un chêne-serveur bienveillant t'offre sa résine. +" + ram3 + " RAM !";
                                break;
                            case 4:
                                int ramAct4 = int.Parse(LireValeur(json, "ram"));
                                int malus4 = Math.Min(rng.Next(int.Parse(LireValeur(cfgAllies, "sbires_ram_min")), int.Parse(LireValeur(cfgAllies, "sbires_ram_max")) + 1), ramAct4);
                                json = AjouterValeur(json, "ram", -malus4);
                                msg = nomJoueur + ", les sbires du Castor t'ont tendu une embuscade ! Tu perds " + malus4 + " RAM...";
                                break;
                            case 5:
                                int pvAct5 = int.Parse(LireValeur(json, "pvActuels"));
                                int malus5 = rng.Next(int.Parse(LireValeur(cfgAllies, "glitch_pv_min")), int.Parse(LireValeur(cfgAllies, "glitch_pv_max")) + 1);
                                int nvPV5 = Math.Max(1, pvAct5 - malus5);
                                json = ModifierValeur(json, "pvActuels", nvPV5.ToString(), false);
                                msg = nomJoueur + ", un glitch du réseau te traverse violemment ! -" + (pvAct5 - nvPV5) + " PV...";
                                break;
                            case 6:
                                int ramAct6 = int.Parse(LireValeur(json, "ram"));
                                int malus6 = Math.Min(rng.Next(int.Parse(LireValeur(cfgAllies, "corruption_ram_min")), int.Parse(LireValeur(cfgAllies, "corruption_ram_max")) + 1), ramAct6);
                                json = AjouterValeur(json, "ram", -malus6);
                                msg = nomJoueur + ", une corruption de données ronge tes ressources. -" + malus6 + " RAM...";
                                break;
                            default:
                                int pvAct7 = int.Parse(LireValeur(json, "pvActuels"));
                                int pvMax7 = int.Parse(LireValeur(json, "pvMax"));
                                int soin7 = Math.Min(rng.Next(int.Parse(LireValeur(cfgAllies, "lichen_pv_min")), int.Parse(LireValeur(cfgAllies, "lichen_pv_max")) + 1), pvMax7 - pvAct7);
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
                        // Marchand : soin interactif, vente de potion automatique
                        int pvActuelsMarchand = int.Parse(LireValeur(json, "pvActuels"));
                        int pvMaxMarchand     = int.Parse(LireValeur(json, "pvMax"));
                        int soinMin  = int.Parse(LireValeur(cfgAllies, "marchand_pv_min"));
                        int soinMaxV = int.Parse(LireValeur(cfgAllies, "marchand_pv_max"));
                        int soin = Math.Min(rng.Next(soinMin, soinMaxV + 1), pvMaxMarchand - pvActuelsMarchand);
                        long expMarchand = long.Parse(LireValeur(cfgAllies, "marchand_expiration"));
                        string msgMarchand;

                        if (soin > 0)
                        {
                            json = ModifierValeur(json, "offreEnAttente", "marchand_soin", true);
                            json = ModifierValeur(json, "offreValeur",    soin.ToString(), false);
                            json = ModifierValeur(json, "offreExpire",    (maintenant + expMarchand).ToString(), false);
                            msgMarchand = nomJoueur + ", un marchand ambulant t'aborde ! Il propose de te soigner (" + soin + " PV). Tape !accepter ou !refuser (2 min) !";
                        }
                        else
                        {
                            msgMarchand = nomJoueur + ", tu croises un marchand ambulant, mais tu n'as pas besoin de soins !";
                        }

                        string invMarchand = LireValeurString(json, "inventaire");
                        int nbItemsMarchand = invMarchand == "" ? 0 : invMarchand.Split(',').Length;
                        int ramMarchand  = int.Parse(LireValeur(json, "ram"));
                        int prixPotion   = int.Parse(LireValeur(cfgAllies, "marchand_prix_potion"));
                        if (nbItemsMarchand < maxSac && ramMarchand >= prixPotion)
                        {
                            json = AjouterValeur(json, "ram", -prixPotion);
                            string nouvInvMarchand = invMarchand == "" ? "Potion" : invMarchand + ",Potion";
                            json = ModifierValeurString(json, "inventaire", nouvInvMarchand);
                            msgMarchand += " Il te vend aussi une Potion pour " + prixPotion + " RAM !";
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
            bool succes = rng.Next(100) >= tauxEchec;

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
                if (data[5] == "artefact" && rng.Next(100) < chanceLootArtefact)
                {
                    string inventaire = LireValeurString(json, "inventaire");
                    int nbItems = inventaire == "" ? 0 : inventaire.Split(',').Length;
                    if (nbItems < maxSac)
                    {
                        string   lootRaw  = LireValeurString(File.ReadAllText(CONFIG_QUETES), "loot_commun");
                        string[] lootPool = lootRaw != "" ? lootRaw.Split(',') : new string[] { "Potion" };
                        string   loot     = lootPool[rng.Next(lootPool.Length)].Trim();
                        string nouvInventaire = inventaire == "" ? loot : inventaire + "," + loot;
                        json = ModifierValeurString(json, "inventaire", nouvInventaire);
                        lootMsg = " Tu as trouvé : " + loot + " !";
                    }
                }

                // Loot secret : morceau d'écorce gravé (20% — seulement les lettres manquantes)
                if (rng.Next(100) < chanceEcorce)
                {
                    string invEcorce = LireValeurString(json, "inventaire");
                    int nbEcorce = invEcorce == "" ? 0 : invEcorce.Split(',').Length;
                    if (nbEcorce < maxSac)
                    {
                        string[] pieces = { "Ecorce-R", "Ecorce-A", "Ecorce-C", "Ecorce-I", "Ecorce-N", "Ecorce-E" };
                        string invAvecVirgules = "," + invEcorce + ",";
                        int nbManquantes = 0;
                        for (int k = 0; k < pieces.Length; k++)
                            if (!invAvecVirgules.Contains("," + pieces[k] + ",")) nbManquantes++;

                        if (nbManquantes > 0)
                        {
                            int pickPiece = rng.Next(nbManquantes);
                            int cntPiece  = 0;
                            string ecorceLoot = "";
                            for (int k = 0; k < pieces.Length; k++)
                            {
                                if (!invAvecVirgules.Contains("," + pieces[k] + ","))
                                {
                                    if (cntPiece == pickPiece) { ecorceLoot = pieces[k]; break; }
                                    cntPiece++;
                                }
                            }
                            string nouvInvEcorce = invEcorce == "" ? ecorceLoot : invEcorce + "," + ecorceLoot;
                            json = ModifierValeurString(json, "inventaire", nouvInvEcorce);
                            lootMsg += " Un morceau d'écorce gravé tombe de ta besace... (" + ecorceLoot + ")";
                        }
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

    // [0]=nom [1]=ticks [2]=xp [3]=ram [4]=demandeur [5]=type
    private string[] GetQueteData(string id)
    {
        string cfg = File.ReadAllText(CONFIG_QUETES);
        for (int i = 1; i <= 99; i++)
        {
            string key = QueteKey(i);
            string qid = LireValeurString(cfg, key + "_id");
            if (qid == "") break;
            if (qid != id) continue;
            return new string[] {
                LireValeurString(cfg, key + "_nom"),
                LireValeur(cfg,       key + "_ticks"),
                LireValeur(cfg,       key + "_xp"),
                LireValeur(cfg,       key + "_ram"),
                LireValeurString(cfg, key + "_demandeur"),
                LireValeurString(cfg, key + "_type")
            };
        }
        return new string[] { "", "1", "0", "0", "Arbonet", "service" };
    }

    private string QueteKey(int i)
    {
        if (i < 10)  return "quete00" + i;
        if (i < 100) return "quete0"  + i;
        return "quete" + i;
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
