Créer une commande !bonjour

Permet de commencer le jeu et des connaître la prochaine commande !rejoindre

Créer une action dans streamer.bot Bonjour>groupe_"La légende de pointu"
	-> Triggered>ADD>Core>Commands>Command Triggered
		-> !bonjour

	-> Sub-Actions>ADD>Core>C#>Execute Code


using System;

public class CPHInline
{
    public bool Execute()
    {
        string nomViewer = args["user"].ToString();

		string message = "Salut " + nomViewer + " ! Je suis Pointu, Gardien de l'Antre. Tape !rejoindre pour commencer ton Aventure.";
		
		CPH.SendMessage (message);

        return true;
    }
}