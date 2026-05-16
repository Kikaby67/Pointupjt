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
			return true;
		}
		
		string classe = LireValeur(json, "classe");
		int[] baseClasse = GetClasseBase(classe);
		int caBase = baseClasse[0];
		int resultat = new Random().Next(1, 5); //1D4
		int nouvelleCa = caBase + resultat;

		json = ModifierValeur(json, "classeArmure",     nouvelleCa.ToString(),    false);
		string nomStat = "CA";

		File.WriteAllText(cheminFichier, json);
		CPH.SendMessage(nomJoueur + " À fait un Jet sur " + nomStat + " : +" + resultat + " !");
		return true;
	}

 	private int[] GetClasseBase(string classe)
    {
        switch (classe)
        {
            case "Hexadécimeur": return new int[] { 25, 14,  5,  8 };
            case "Cryptolame":   return new int[] { 16, 13,  5, 11 };
            case "Hackmancien":  return new int[] { 14, 10, 30, 10 };
            case "Firewaller":   return new int[] { 22, 15, 25, 13 };
            case "Algorythmien": return new int[] { 16, 11, 20, 16 };
            default:             return new int[] { 10, 10,  0,  0 };
        }
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
