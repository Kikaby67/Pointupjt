using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_ALLIES   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_allies.json";
    private const string CONFIG_GLOBAL   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_global.json";

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
            Random rng         = new Random();

            // Un seul roll : soit combat, soit le sage disparaît
            bool combat = rng.Next(100) < chanceCombat;

            if (combat)
            {
                // Pose une rencontre à choix unique (!combat / !discuter / !fuir)
                long maintenant = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                int  expireSecs = int.Parse(LireValeur(File.ReadAllText(CONFIG_GLOBAL), "rencontre_expire_secondes"));

                json = EnsureChamp(json, "rencontreExpire", "0", false);
                if (LireValeur(json, "enQuete") == "true")
                    json = ModifierValeur(json, "quetePauseDebut", maintenant.ToString(), false);  // pause quête

                json = ModifierValeur(json, "enRencontre",     "true",                          false);
                json = ModifierValeur(json, "rencontreType",   "combat",                        true);
                json = ModifierValeur(json, "enCombat",        "true",                          false);
                json = ModifierValeur(json, "ennemiNom",       "Vieux-Sage",                    true);
                json = ModifierValeur(json, "rencontreExpire", (maintenant + expireSecs).ToString(), false);

                File.WriteAllText(cheminFichier, json);
                CPH.SendMessage(nomJoueur + ", tu refuses le marché — le Vieux Sage se lève, les yeux brillants de colère ! Tape !combat pour te battre, !fuir pour lui échapper ou !discuter afin de tenter ta chance.");
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
