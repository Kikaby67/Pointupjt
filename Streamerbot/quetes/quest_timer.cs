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
        int expireSecs           = int.Parse(LireValeur(cfgG, "rencontre_expire_secondes"));
        int intervalleRenc       = int.Parse(LireValeur(cfgG, "quete_rencontre_intervalle_secondes"));
        int miniBossNivMin       = int.Parse(LireValeur(cfgG, "mini_boss_niveau_min"));
        int miniBossChance       = int.Parse(LireValeur(cfgG, "mini_boss_chance"));

        foreach (string chemin in fichiers)
        {
            string json = File.ReadAllText(chemin);
            if (LireValeur(json, "enQuete") != "true") continue;

            string nomJoueur = LireValeur(json, "nomJoueur");
            json = EnsureChamp(json, "rencontreExpire", "0", false);
            json = EnsureChamp(json, "compagnonActif", "", true);

            // === CAS 1 : rencontre en attente (résolue par !combat/!discuter/!fuir) ===
            if (LireValeur(json, "enRencontre") == "true")
            {
                long expire = long.Parse(LireValeur(json, "rencontreExpire"));
                if (expire > 0 && maintenant > expire)
                {
                    // Le joueur a ignoré la rencontre → fuite automatique, la quête reprend
                    string ennemiIgnore = LireValeur(json, "ennemiNom");
                    long pauseDebut = long.Parse(LireValeur(json, "quetePauseDebut"));
                    long totalPause = long.Parse(LireValeur(json, "queteTotalPause"));
                    if (pauseDebut > 0) totalPause += maintenant - pauseDebut;

                    json = ModifierValeur(json, "enCombat", "false", false);
                    json = ModifierValeur(json, "enRencontre", "false", false);
                    json = ModifierValeur(json, "rencontreType", "", true);
                    json = ModifierValeur(json, "rencontreExpire", "0", false);
                    json = ModifierValeur(json, "quetePauseDebut", "0", false);
                    json = ModifierValeur(json, "queteTotalPause", totalPause.ToString(), false);
                    File.WriteAllText(chemin, json);
                    CPH.SendMessage(nomJoueur + ", " + ennemiIgnore + " se lasse de t'attendre et s'éloigne. Ta quête reprend.");
                }
                continue; // sinon : on attend le choix du joueur (!combat/!discuter/!fuir)
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

            if (maintenant - dernierCheck >= intervalleRenc && offreEnCours == "")
            {
                json = ModifierValeur(json, "dernierCheckRencontre", maintenant.ToString(), false);

                if (rng.Next(100) < chanceRencontre)
                {
                    int typeRoll = rng.Next(3); // 0 = combat, 1 = événement, 2 = marchand

                    if (typeRoll == 0)
                    {
                        // Rencontre : pause quête + propose les 3 choix (résolus par commande)
                        // Sous-tirage mini-boss : seulement à partir du niveau requis
                        int niveauJoueur = int.Parse(LireValeur(json, "niveau"));
                        bool estMiniBoss = niveauJoueur >= miniBossNivMin && rng.Next(100) < miniBossChance;
                        string poolRenc = estMiniBoss ? "rencontre_mini_boss" : "rencontre_ennemis";
                        string[] ennemis = LireValeurString(cfgG, poolRenc).Split(',');
                        string ennemiChoisi = ennemis[rng.Next(ennemis.Length)].Trim();

                        json = ModifierValeur(json, "enRencontre", "true", false);
                        json = ModifierValeur(json, "rencontreType", "combat", true);
                        json = ModifierValeur(json, "quetePauseDebut", maintenant.ToString(), false);
                        json = ModifierValeur(json, "enCombat", "true", false);
                        json = ModifierValeur(json, "ennemiNom", ennemiChoisi, true);
                        json = ModifierValeur(json, "rencontreExpire", (maintenant + expireSecs).ToString(), false);
                        File.WriteAllText(chemin, json);
                        if (estMiniBoss)
                            CPH.SendMessage(nomJoueur + ", ⚠️ MINI-BOSS ! " + ennemiChoisi + " te barre la route ! Quête en pause. Tape !combat pour l'affronter, !fuir pour tenter de l'éviter ou !discuter. (" + (expireSecs / 60) + " min)");
                        else
                            CPH.SendMessage(nomJoueur + ", un " + ennemiChoisi + " surgit sur ta route ! Quête en pause. Tape !combat pour te battre, !fuir pour lui échapper ou !discuter afin de tenter ta chance. (" + (expireSecs / 60) + " min)");
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
                        // Marchand : soin (!accepter) ET potion à l'achat (!acheter) — deux choix séparés, rien de forcé
                        int pvActuelsMarchand = int.Parse(LireValeur(json, "pvActuels"));
                        int pvMaxMarchand     = int.Parse(LireValeur(json, "pvMax"));
                        int soinMin  = int.Parse(LireValeur(cfgAllies, "marchand_pv_min"));
                        int soinMaxV = int.Parse(LireValeur(cfgAllies, "marchand_pv_max"));
                        int soin = Math.Min(rng.Next(soinMin, soinMaxV + 1), pvMaxMarchand - pvActuelsMarchand);
                        long expMarchand = long.Parse(LireValeur(cfgAllies, "marchand_expiration"));
                        int prixPotion   = int.Parse(LireValeur(cfgAllies, "marchand_prix_potion"));

                        // L'offre reste active (acceptée/refusée/expirée) : autorise !accepter (soin) ET !acheter (potion)
                        json = ModifierValeur(json, "offreEnAttente", "marchand_soin", true);
                        json = ModifierValeur(json, "offreValeur",    soin.ToString(), false);
                        json = ModifierValeur(json, "offreExpire",    (maintenant + expMarchand).ToString(), false);

                        string msgMarchand = nomJoueur + ", un marchand ambulant t'aborde ! ";
                        if (soin > 0) msgMarchand += "Soin : !accepter (+" + soin + " PV) ou !refuser. ";
                        else          msgMarchand += "(tu as déjà tous tes PV) ";
                        msgMarchand += "Potion : !acheter (" + prixPotion + " RAM). (" + (expMarchand / 60) + " min)";

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
            json = ModifierValeur(json, "compagnonActif", "", true);  // le compagnon ne suit que sur une quête

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
                        // Tirage de rareté : légendaire / épique / rare / commun (chances dans config_global)
                        int rar  = rng.Next(100);
                        int cLeg = int.Parse(LireValeur(cfgG, "loot_chance_legendaire"));
                        int cEpi = int.Parse(LireValeur(cfgG, "loot_chance_epique"));
                        int cRar = int.Parse(LireValeur(cfgG, "loot_chance_rare"));
                        string pool;
                        if      (rar < cLeg)               pool = "loot_legendaire";
                        else if (rar < cLeg + cEpi)        pool = "loot_epique";
                        else if (rar < cLeg + cEpi + cRar) pool = "loot_rare";
                        else                               pool = "loot_commun";

                        string   cfgLoot  = File.ReadAllText(CONFIG_QUETES);
                        string   lootRaw  = LireValeurString(cfgLoot, pool);
                        if (lootRaw == "") lootRaw = LireValeurString(cfgLoot, "loot_commun");
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

    // Insère un champ s'il est absent du JSON (migration des anciens profils)
    private string EnsureChamp(string json, string cle, string valeurDefaut, bool estTexte)
    {
        if (json.Contains("\"" + cle + "\"")) return json;
        int    pos = json.LastIndexOf('}');
        string val = estTexte ? "\"" + valeurDefaut + "\"" : valeurDefaut;
        return json.Substring(0, pos) + ",\n  \"" + cle + "\": " + val + "\n}";
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
