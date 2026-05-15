using System;
using System.IO;

public class CPHInline
{
	private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
	public bool Execute()
	{
		// Reward point Stat PV
		string nomJoueur	= args["user"].ToString();
		string cheminFichier = Path.Combine(DOSSIER_JOUEURS, nomJoueur.ToLower() + ".json");

		if (!File.Exists(cheminFichier))
		{
			CPH.SendMessage(nomJoueur + ", tape !rejoindre pour créer ton profil !");
			return true;
		}

		string json = File.ReadAllText(cheminFichier);

		if (LireValeur(json, "classeChoisie") != "true")
		{
			CPH.SendMessage(nomJoueur + ", choisis d'abord ta classe avec !choisirclasse !");
		}
		
		int resultat = new Random().Next(1, 7); //1D6
		int ancienPvMax = int.Parse(LireValeur(json, "pvMax")) + resultat;
		int ancienPvActuel = int.Parse(LireValeur(json, "pvActuels")) + resultat;
		json = ModifierValeur(json, "pvMax",     ancienPvMax.ToString(),    false);
		json = ModifierValeur(json, "pvActuels", ancienPvActuel.ToString(), false);
		string nomStat = "PV";

		File.WriteAllText(cheminFichier, json);
		CPH.SendMessage(nomJoueur + " Jet sur " + nomStat + " : +" + resultat + " !");
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