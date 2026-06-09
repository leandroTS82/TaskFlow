# docker-helper.ps1
# generic helper run docker commands

function Show-Menu {
    Clear-Host
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host "   TaskFlow — Docker Helper     " -ForegroundColor Cyan
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  [1] Start containers" -ForegroundColor Green
    Write-Host "  [2] Stop and remove containers" -ForegroundColor Yellow
    Write-Host "  [3] Restart (down -v + up)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  [4] Init Replica Set" -ForegroundColor Magenta
    Write-Host "  [5] Check Replica Set status" -ForegroundColor Magenta
    Write-Host ""
    Write-Host "  [6] Logs — MongoDB" -ForegroundColor Gray
    Write-Host "  [7] Logs — API" -ForegroundColor Gray
    Write-Host "  [8] Logs — Mongo Init" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  [0] Exit" -ForegroundColor Red
    Write-Host ""
    Write-Host "================================" -ForegroundColor Cyan
}

do {
    Show-Menu
    $choice = Read-Host "Select an option"

    switch ($choice) {
        "1" {
            Write-Host "`nStarting TaskFlow containers..." -ForegroundColor Cyan
            docker-compose up -d
            Read-Host "`nPress Enter to continue"
        }
        "2" {
            Write-Host "`nStopping and removing containers and volumes..." -ForegroundColor Yellow
            docker-compose down -v
            Read-Host "`nPress Enter to continue"
        }
        "3" {
            Write-Host "`nRestarting TaskFlow containers..." -ForegroundColor Yellow
            docker-compose down -v
            Write-Host "`nStarting containers..." -ForegroundColor Cyan
            docker-compose up -d
            Read-Host "`nPress Enter to continue"
        }
        "4" {
            Write-Host "`nInitializing MongoDB Replica Set..." -ForegroundColor Magenta
            docker exec -it taskflow-mongo mongosh -u root -p password --authenticationDatabase admin --eval "rs.initiate({ _id: 'rs0', members: [{ _id: 0, host: 'localhost:27017' }] })"
            Read-Host "`nPress Enter to continue"
        }
        "5" {
            Write-Host "`nReplica Set status (1 = Primary):" -ForegroundColor Magenta
            docker exec -it taskflow-mongo mongosh -u root -p password --authenticationDatabase admin --eval "rs.status().myState"
            Read-Host "`nPress Enter to continue"
        }
        "6" {
            Write-Host "`nMongoDB logs (Ctrl+C to exit):`n" -ForegroundColor Gray
            docker logs taskflow-mongo --follow
        }
        "7" {
            Write-Host "`nAPI logs (Ctrl+C to exit):`n" -ForegroundColor Gray
            docker logs taskflow-api --follow
        }
        "8" {
            Write-Host "`nMongo Init logs (Ctrl+C to exit):`n" -ForegroundColor Gray
            docker logs taskflow-mongo-init --follow
        }
        "0" {
            Write-Host "`nBye!" -ForegroundColor Red
        }
        default {
            Write-Host "`nInvalid option." -ForegroundColor Red
            Start-Sleep -Seconds 1
        }
    }
} while ($choice -ne "0")