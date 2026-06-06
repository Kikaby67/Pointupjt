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
            CPH.SendMessage(nomJoueur + ", tu n'es pas encore inscrit ! Tape !rejoindre.");
            return true;
        }

        string json          = File.ReadAllText(cheminFichier);
        string offre          = LireValeurString(json, "offreEnAttente");
        long   offreExpire    = long.Parse(LireValeur(json, "offreExpire"));
        long   maintenant     = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Le marchand n'est présent que tant que son offre est active
        if (offre != "marchand_soin")
        {
            CPH.SendMessage(nomJoueur + ", aucun marchand n'est là pour te vendre quoi que ce soit.");
            return true;
        }
        if (offreExpire > 0 && maintenant > offreExpire)
        {
            CPH.SendMessage(nomJoueur + ", le marchand a déjà repris la route...");
            return true;
        }

        int    prixPotion = int.Parse(LireValeur(File.ReadAllText(CONFIG_ALLIES), "marchand_prix_potion"));
        int    maxSac     = int.Parse(LireValeur(File.ReadAllText(CONFIG_GLOBAL), "max_sac"));
        int    ram        = int.Parse(LireValeur(json, "ram"));
        string inv        = LireValeurString(json, "inventaire");
        int    nbItems    = inv == "" ? 0 : inv.Split(',').Length;

        if (nbItems >= maxSac)
        {
            CPH.SendMessage(nomJoueur + ", ton sac est plein (" + nbItems + "/" + maxSac + ") — impossible d'acheter la Potion.");
            return true;
        }
        if (ram < prixPotion)
        {
            CPH.SendMessage(nomJoueur + ", il te faut " + prixPotion + " RAM pour la Potion (tu en as " + ram + ").");
            return true;
        }

        json = AjouterValeur(json, "ram", -prixPotion);
        string nouvInv = inv == "" ? "Potion" : inv + ",Potion";
        json = ModifierValeurString(json, "inventaire", nouvInv);
        File.WriteAllText(cheminFichier, json);

        CPH.SendMessage(nomJoueur + " achète une Potion au marchand pour " + prixPotion + " RAM (reste " + (ram - prixPotion) + " RAM). Tape !utiliser Potion pour t'en servir.");
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
}
