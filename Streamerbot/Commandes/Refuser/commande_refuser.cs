using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_ALLIES   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_allies.json";

    public bool Execute()
    {
        string nomJoueur     = args["user"].ToString();
        string cheminFichier = Path.Combine(DOSSIER_JOUEURS, nomJoueur.ToLower() + ".json");

        if (!File.Exists(cheminFichier))
        {
            CPH.SendMessage(nomJoueur + ", tape !rejoindre pour t'inscrire dans l'Antre de Pointu !");
            return true;
        }

        string json           = File.ReadAllText(cheminFichier);
        string offreEnAttente = LireValeurString(json, "offreEnAttente");

        if (offreEnAttente == "")
        {
            CPH.SendMessage(nomJoueur + ", tu n'as aucune offre en attente !");
            return true;
        }

        // Nettoyer l'offre (commun à tous les cas)
        json = ModifierValeur(json, "offreEnAttente", "", true);
        json = ModifierValeur(json, "offreValeur",    "0", false);
        json = ModifierValeur(json, "offreExpire",    "0", false);

        if (offreEnAttente == "vieux_sage")
        {
            string cfgA        = File.ReadAllText(CONFIG_ALLIES);
            int    chanceCombat = int.Parse(LireValeur(cfgA, "vieux_sage_chance_combat"));
            int    pvSage      = int.Parse(LireValeur(cfgA, "vieux_sage_pv"));
            Random rng         = new Random();

            // Un seul roll : soit combat, soit le sage disparaît
            bool combat = rng.Next(100) < chanceCombat;

            if (combat)
            {
                // Lancer le combat comme une rencontre normale
                // Si en quête → pauser la quête (enRencontre = true)
                bool enQuete = LireValeur(json, "enQuete") == "true";
                if (enQuete)
                {
                    long maintenant = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    json = ModifierValeur(json, "enRencontre",    "true",                true);  // pause quête
                    json = ModifierValeur(json, "rencontreType",  "combat",              true);
                    json = ModifierValeur(json, "quetePauseDebut", maintenant.ToString(), false);
                }

                json = ModifierValeur(json, "enCombat",        "true",         false);
                json = ModifierValeur(json, "ennemiNom",       "Vieux-Sage",   true);
                json = ModifierValeur(json, "ennemiPVActuels", pvSage.ToString(), false);
                json = ModifierValeur(json, "tourCombat",      "1",            false);
                json = ModifierValeur(json, "buffActif",       "false",        false);
                json = ModifierValeur(json, "protectionActive","false",        false);

                File.WriteAllText(cheminFichier, json);
                CPH.SendMessage(nomJoueur + ", tu refuses le marché — le Vieux Sage se lève, les yeux brillants de colère ! Bats-toi !");
            }
            else
            {
                // Le sage disparaît, rien ne se passe
                File.WriteAllText(cheminFichier, json);
                CPH.SendMessage(nomJoueur + ", tu refuses le marché — le Vieux Sage profite de ton inattention pour disparaître sans laisser de trace...");
            }
        }
        else if (offreEnAttente == "marchand_soin")
        {
            File.WriteAllText(cheminFichier, json);
            CPH.SendMessage(nomJoueur + ", tu refuses les soins du marchand. Il hausse les épaules et repart sur la route...");
        }
        else
        {
            File.WriteAllText(cheminFichier, json);
            CPH.SendMessage(nomJoueur + ", offre refusée.");
        }

        return true;
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
