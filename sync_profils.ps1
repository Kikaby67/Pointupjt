# Synchronise les profils joueurs vers GitHub (pour que le bot Discord les lise).
# Ne pousse QUE Donnees/joueurs/ — n'entraîne pas le reste des modifs en cours.
# Lancer manuellement, ou via une Tâche planifiée toutes les ~10 min pendant le stream.

$repo = "C:\Users\Florian\pjt\Pointu-PJT"

git -C $repo add "Donnees/joueurs"

# Rien à committer ? on sort proprement.
git -C $repo diff --cached --quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "Profils déjà à jour, rien à pousser."
    exit 0
}

$horodatage = Get-Date -Format "yyyy-MM-dd HH:mm"
git -C $repo commit -m "sync profils joueurs ($horodatage)"
git -C $repo push
Write-Host "Profils synchronisés vers GitHub."
