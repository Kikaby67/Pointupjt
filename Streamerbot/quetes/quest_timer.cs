using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";

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
                    // Défaite : quête annulée, 20 min de cooldown
                    long cooldownFin = maintenant + 20 * 60;
                    json = ModifierValeur(json, "enQuete", "false", false);
                    json = ModifierValeur(json, "enRencontre", "false", false);
                    json = ModifierValeur(json, "rencontreType", "", true);
                    json = ModifierValeur(json, "queteTicksRestants", "0", false);
                    json = ModifierValeur(json, "quetePauseDebut", "0", false);
                    json = ModifierValeur(json, "queteCooldownFin", cooldownFin.ToString(), false);
                    File.WriteAllText(chemin, json);
                    CPH.SendMessage(nomJoueur + ", tu t'es effondré face à l'ennemi... Quête abandonnée. Tu te réfugies dans l'Antre de Pointu — 20 minutes de récupération avant de repartir !");
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
                        string[] ennemis = { "Gobelin corrompu", "Sentinelle du Castor", "Ombre de la mémoire", "Drone-racine", "Parasite de données" };
                        int[] pvEnnemis = { 30, 25, 20, 15, 18 };
                        int idx = rng.Next(ennemis.Length);

                        json = ModifierValeur(json, "enRencontre", "true", false);
                        json = ModifierValeur(json, "rencontreType", "combat", true);
                        json = ModifierValeur(json, "quetePauseDebut", maintenant.ToString(), false);
                        json = ModifierValeur(json, "enCombat", "true", false);
                        json = ModifierValeur(json, "ennemiNom", ennemis[idx], true);
                        json = ModifierValeur(json, "ennemiPVActuels", pvEnnemis[idx].ToString(), false);
                        json = ModifierValeur(json, "tourCombat", "1", false);
                        json = ModifierValeur(json, "buffActif", "false", false);
                        json = ModifierValeur(json, "protectionActive", "false", false);
                        File.WriteAllText(chemin, json);
                        CPH.SendMessage(nomJoueur + ", un " + ennemis[idx] + " surgit sur ta route ! (" + pvEnnemis[idx] + " PV) Quête mise en pause — bats-toi !");
                        encounterLancee = true;
                    }
                    else if (typeRoll == 1)
                    {
                        // Événement narratif : auto-résolution, pas de pause
                        if (rng.Next(2) == 0)
                        {
                            int bonusXP = rng.Next(10, 30);
                            json = AjouterValeur(json, "experience", bonusXP);
                            File.WriteAllText(chemin, json);
                            CPH.SendMessage(nomJoueur + ", tu croises un vieux sage sur ta route. Il te transmet ses connaissances. +" + bonusXP + " XP !");
                        }
                        else
                        {
                            int ramActuel = int.Parse(LireValeur(json, "ram"));
                            int malus = Math.Min(rng.Next(5, 15), ramActuel);
                            int nouveauRam = ramActuel - malus;
                            json = ModifierValeur(json, "ram", nouveauRam.ToString(), false);
                            File.WriteAllText(chemin, json);
                            CPH.SendMessage(nomJoueur + ", tu tombes dans un piège tendu par les sbires du Castor ! Tu perds " + malus + " RAM...");
                        }
                        json = File.ReadAllText(chemin);
                    }
                    else
                    {
                        // Marchand : soin plafonné à pvMax, pas de pause
                        int pvActuels = int.Parse(LireValeur(json, "pvActuels"));
                        int pvMax = int.Parse(LireValeur(json, "pvMax"));
                        int soin = Math.Min(rng.Next(5, 15), pvMax - pvActuels);
                        if (soin > 0)
                        {
                            json = AjouterValeur(json, "pvActuels", soin);
                            File.WriteAllText(chemin, json);
                            CPH.SendMessage(nomJoueur + ", tu croises un marchand ambulant qui soigne tes blessures. +" + soin + " PV !");
                        }
                        else
                        {
                            File.WriteAllText(chemin, json);
                            CPH.SendMessage(nomJoueur + ", tu croises un marchand ambulant, mais tu n'as pas besoin de soins !");
                        }
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
                File.WriteAllText(chemin, json);
                CPH.SendMessage(nomJoueur + ", ta quête est terminée ! Succès ! Tu gagnes " + xp + " XP et " + ram + " RAM. Bien joué aventurier !");
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

    private string[] GetQueteData(string id)
    {
        switch (id)
        {
            case "artefact_01":  return new string[] { "", "6", "100", "10" };
            case "artefact_02":  return new string[] { "", "5", "80",  "8"  };
            case "artefact_03":  return new string[] { "", "3", "50",  "5"  };
            case "artefact_04":  return new string[] { "", "2", "30",  "3"  };
            case "artefact_05":  return new string[] { "", "1", "10",  "1"  };
            case "service_01":   return new string[] { "", "3", "50",  "5"  };
            case "service_02":   return new string[] { "", "4", "70",  "7"  };
            case "service_03":   return new string[] { "", "5", "90",  "9"  };
            case "service_04":   return new string[] { "", "6", "120", "12" };
            case "service_05":   return new string[] { "", "2", "40",  "4"  };
            case "entretien_01": return new string[] { "", "3", "50",  "5"  };
            case "entretien_02": return new string[] { "", "4", "70",  "7"  };
            case "entretien_03": return new string[] { "", "5", "90",  "9"  };
            default:             return new string[] { "", "1", "0",   "0"  };
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
