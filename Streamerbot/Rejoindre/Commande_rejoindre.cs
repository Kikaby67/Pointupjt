Créer une commande !rejoindre

Permet de rejoindre et de créer sa fiche perso et la stocker dans un fichier json.

Créer une action dans streamer.bot Bonjour>groupe_"La légende de pointu"
	-> Triggered>ADD>Core>Commands>Command Triggered
		-> !rejoindre

	-> Sub-Actions>ADD>Core>C#>Execute Code






using System;
using System.IO;

public class CPHInline
{
	public bool Execute()
	{
		string nomJoueur = args["user"].ToString();

		string cheminFichier = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs\" + nomJoueur + ".json";

		if (File.Exists(cheminFichier))
		{
			CPH.SendMessage(nomJoueur + " tu es déjà inscrit à l'Antre !");
			return true;
		}

		string profil = "{\n" +
						"  \"nomJoueur\": \"" + nomJoueur + "\",\n" +
						"  \"niveau\": 1,\n" +
						"  \"xp\": 0,\n" +
						"  \"ram\": 10,\n" +
						"  \"sac\": []\n" +
						"}";

		Directory.CreateDirectory(@"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs");
		File.WriteAllText(cheminFichier, profil);

		CPH.SendMessage(nomJoueur + " Bien, le bonjour Aventurier de l'Antre ! Ton fragment de Carapace est prêt. Niveau 1 | XP : 0 | Ram : 10");

		return true;
	}
}