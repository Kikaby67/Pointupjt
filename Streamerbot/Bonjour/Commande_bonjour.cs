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