# Dev script for Windows PowerShell
param([string]$Command, [string]$Message = "")

switch ($Command) {
    "run"     { flask run --host=0.0.0.0 --port=5000 }
    "migrate" { flask db upgrade }
    "seed"    { flask seed-data }
    "worker"  { celery -A app.celery worker --pool=solo --loglevel=info }
    "test"    { pytest tests/ -v }
    "docker"  { docker compose -f docker-compose.dev.yml up -d }
    "shell"   { flask shell }

    "save"    {
                if ($Message -eq "") { Write-Host "Usage: .\dev.ps1 save 'commit message'"; return }
                git add .
                git commit -m $Message
                git push
                Write-Host "Pushed: $Message"
              }
    "sync"    { git pull origin develop }
    "status"  { git status }
    "log"     { git log --oneline -10 }

    default   {
        Write-Host "Dev Commands: run | migrate | seed | worker | test | docker | shell"
        Write-Host "Git Commands: save '<message>' | sync | status | log"
    }
}
