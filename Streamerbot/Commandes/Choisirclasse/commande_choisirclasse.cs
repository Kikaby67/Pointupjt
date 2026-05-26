using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_CLASSES  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_classes.json";

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
        string typeArme  = LireValeur(cfg, classeNom + "_typeArme");

        // ── Jets de dés ───────────────────────────────────────
        Random rng = new Random();

        int jetPV        = rng.Next(1, 7);  // 1d6 → 1 à 6
        int jetCA        = rng.Next(1, 5);  // 1d4 → 1 à 4
        int bonusAttaque = rng.Next(1, 5);  // 1d4 → 1 à 4

        int pvFinal  = pvBase  + jetPV;
        int caFinale = caBase  + jetCA;

        // ── Sauvegarde dans le JSON ───────────────────────────
        json = ModifierValeur(json, "classeChoisie", "true",                  false);
        json = ModifierValeur(json, "classe",         classeNom,              true);
        json = ModifierValeur(json, "typeArme",       typeArme,               true);
        json = ModifierValeur(json, "pvMax",          pvFinal.ToString(),     false);
        json = ModifierValeur(json, "pvActuels",      pvFinal.ToString(),     false);
        json = ModifierValeur(json, "classeArmure",   caFinale.ToString(),    false);
        json = ModifierValeur(json, "bonusAttaque",   bonusAttaque.ToString(),false);
        json = ModifierValeur(json, "manaMax",        manaBase.ToString(),    false);
        json = ModifierValeur(json, "manaActuels",    manaBase.ToString(),    false);
        json = ModifierValeur(json, "charisme",       charismeBase.ToString(),false);

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