using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_CLASSES  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_classes.json";
    private const string CONFIG_GLOBAL   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_global.json";

    public bool Execute()
    {
        // ── Nom du viewer et classe tapée ─────────────────────
        // rawInput = ce que le joueur a écrit après la commande
        // Ex : "!choisirclasse Cryptolame" → rawInput = "cryptolame"
        string nomJoueur     = args["user"].ToString();
        string rawInput      = args["rawInput"].ToString().Trim().ToLower();
        string cheminFichier = Path.Combine(DOSSIER_JOUEURS, nomJoueur.ToLower() + ".json");

        // ── Vérifications ─────────────────────────────────────
        if (!File.Exists(cheminFichier))
        {
            CPH.SendMessage(nomJoueur + ", tape !rejoindre d'abord !");
            return true;
        }

        string json            = File.ReadAllText(cheminFichier);
        bool classeDejaChoisie = LireValeur(json, "classeChoisie") == "true";

        if (classeDejaChoisie)
        {
            CPH.SendMessage(nomJoueur + ", tu as déjà une classe ! " +
                "Rencontre un Marchand de Classe pour en changer.");
            return true;
        }

        // ── Normalisation du nom de classe ────────────────────
        string classeNom;
        switch (rawInput)
        {
            case "hexadécimeur":
            case "hexadecimeur":    classeNom = "Hexadécimeur";    break;
            case "cryptolame":      classeNom = "Cryptolame";      break;
            case "hackmancien":     classeNom = "Hackmancien";     break;
            case "firewaller":      classeNom = "Firewaller";      break;
            case "algorythmancien": classeNom = "Algorythmancien"; break;
            default:
                CPH.SendMessage(nomJoueur + ", classe inconnue ! Choisis parmi : " +
                    "Hexadécimeur · Cryptolame · Hackmancien · Firewaller · Algorythmancien");
                return true;
        }

        // ── Chargement des stats depuis la config ─────────────
        string cfg       = File.ReadAllText(CONFIG_CLASSES);
        int pvBase       = int.Parse(LireValeur(cfg, classeNom + "_pvBase"));
        int caBase       = int.Parse(LireValeur(cfg, classeNom + "_caBase"));
        int manaBase     = int.Parse(LireValeur(cfg, classeNom + "_manaBase"));
        int charismeBase = int.Parse(LireValeur(cfg, classeNom + "_charisme"));
        int agiliteBase  = int.Parse(LireValeur(cfg, classeNom + "_agilite"));
        string typeArme  = LireValeur(cfg, classeNom + "_typeArme");

        // ── Jets de dés (faces définies dans config_global) ───
        string cfgG = File.ReadAllText(CONFIG_GLOBAL);
        Random rng = new Random();

        int jetPV        = rng.Next(1, int.Parse(LireValeur(cfgG, "creation_pv_de"))  + 1);  // 1dN PV
        int jetCA        = rng.Next(1, int.Parse(LireValeur(cfgG, "creation_ca_de"))  + 1);  // 1dN CA
        int bonusAttaque = rng.Next(1, int.Parse(LireValeur(cfgG, "creation_atq_de")) + 1);  // 1dN Atq

        int pvFinal  = pvBase  + jetPV;
        int caFinale = caBase  + jetCA;

        // ── Sauvegarde dans le JSON ───────────────────────────
        json = EnsureChamp(json, "agilite",        "0", false);   // champs récents (anciens profils)
        json = EnsureChamp(json, "compagnonActif", "",  true);
        json = EnsureChamp(json, "rencontreExpire", "0", false);
        json = ModifierValeur(json, "classeChoisie", "true",                  false);
        json = ModifierValeur(json, "classe",         classeNom,              true);
        json = ModifierValeur(json, "typeArme",       typeArme,               true);
        json = ModifierValeur(json, "armeEquipee",    typeArme,               true);  // arme de classe équipée d'office
        json = ModifierValeur(json, "pvMax",          pvFinal.ToString(),     false);
        json = ModifierValeur(json, "pvActuels",      pvFinal.ToString(),     false);
        json = ModifierValeur(json, "classeArmure",   caFinale.ToString(),    false);
        json = ModifierValeur(json, "bonusAttaque",   bonusAttaque.ToString(),false);
        json = ModifierValeur(json, "manaMax",        manaBase.ToString(),    false);
        json = ModifierValeur(json, "manaActuels",    manaBase.ToString(),    false);
        json = ModifierValeur(json, "charisme",       charismeBase.ToString(),false);
        json = ModifierValeur(json, "agilite",        agiliteBase.ToString(), false);

        File.WriteAllText(cheminFichier, json);

        // ── Message dans le chat ──────────────────────────────
        // On formate le signe du bonus pour afficher +2 ou -1
        string signCA  = jetCA        >= 0 ? "+" + jetCA        : jetCA.ToString();
        string signAtq = bonusAttaque >= 0 ? "+" + bonusAttaque : bonusAttaque.ToString();

        CPH.SendMessage(nomJoueur + " entre dans l'Antre en tant que " + classeNom + " ! " +
            "PV : " + pvBase + "+" + jetPV + "=" + pvFinal + " | " +
            "CA : " + caBase + signCA + "=" + caFinale + " | " +
            "Atq : " + signAtq + " | " +
            "Arme : " + typeArme);

        return true;
    }

    // Insère un champ s'il est absent du JSON (anciens profils)
    private string EnsureChamp(string json, string cle, string valeurDefaut, bool estTexte)
    {
        if (json.Contains("\"" + cle + "\"")) return json;
        int    pos = json.LastIndexOf('}');
        string val = estTexte ? "\"" + valeurDefaut + "\"" : valeurDefaut;
        return json.Substring(0, pos) + ",\n  \"" + cle + "\": " + val + "\n}";
    }

    // ── LireValeur ────────────────────────────────────────────
    private string LireValeur(string json, string cle)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return "";
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        return json.Substring(posDebut, posFin - posDebut).Trim().Trim('"');
    }

    // ── ModifierValeur ────────────────────────────────────────
    // estTexte = true  → entoure la valeur de guillemets  "Cryptolame"
    // estTexte = false → écrit la valeur brute             16, true, false
    private string ModifierValeur(string json, string cle, string nouvelleValeur, bool estTexte)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return json;
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        string ancienne = json.Substring(posDebut, posFin - posDebut);
        string nouvelle = estTexte ? "\"" + nouvelleValeur + "\"" : nouvelleValeur;
        return json.Substring(0, posDebut) + nouvelle + json.Substring(posDebut + ancienne.Length);
    }
}