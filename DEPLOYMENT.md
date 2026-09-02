# Deploying Ghasele.API to 65.109.146.40

Manual deployment runbook: back up the running release, upload a fresh publish
from the local machine, restart, verify, and roll back if it goes wrong.

| | |
|---|---|
| Server | `root@65.109.146.40` |
| Live folder | `/root/www/ghasele-api` |
| Backup folder | `/root/www/ghasele-api-backup` |
| Local publish output | `D:\Ghaselak\ghasele-backend\Ghasele.API\bin\Release\net9.0\publish` |

Local commands are PowerShell, run from Windows. Remote commands run in an SSH
session on the server. Windows 10/11 ship both `ssh` and `scp`, so nothing extra
needs installing.

---

## 0. Fill in these two blanks first

The commands below assume a systemd service named `ghasele-api`. Confirm the real
name and the port it listens on before you start:

```bash
ssh root@65.109.146.40
systemctl list-units --type=service | grep -i ghasele
ss -tlnp | grep dotnet
```

Note the unit name and port. If the unit is called something else, substitute it
everywhere `ghasele-api.service` appears below.

---

## 1. Publish locally

The publish folder is **not** rebuilt when you edit a file, so publish again or
you will deploy stale DLLs and stale `appsettings.json`.

```powershell
cd D:\Ghaselak\ghasele-backend
dotnet publish Ghasele.API\Ghasele.API.csproj -c Release
```

Verify the settings that matter actually made it into the output:

```powershell
Select-String -Path "D:\Ghaselak\ghasele-backend\Ghasele.API\bin\Release\net9.0\publish\appsettings.json" -Pattern "ClientIds"
```

Expected: `"ClientIds": [ "com.kalbouneh.cleanyjo" ]`

> If this still says `com.kalbouneh.naqa`, the publish did not run. Apple sign-in
> will keep failing with `IDX10214: Audience validation failed`.

---

## 2. Package and upload

One compressed archive instead of a few hundred loose DLLs - a directory `scp`
of this app takes minutes and fails halfway often enough to be annoying.

```powershell
cd "D:\Ghaselak\ghasele-backend\Ghasele.API\bin\Release\net9.0\publish"
tar -czf "$env:TEMP\ghasele-api.tar.gz" .
scp "$env:TEMP\ghasele-api.tar.gz" root@65.109.146.40:/tmp/
```

Upload lands in `/tmp`, not on the live folder, so nothing is disturbed yet and a
failed transfer costs nothing.

---

## 3. Unpack into a staging folder

SSH in for the rest:

```bash
ssh root@65.109.146.40
```

```bash
rm -rf /root/www/ghasele-api-new
mkdir -p /root/www/ghasele-api-new
tar -xzf /tmp/ghasele-api.tar.gz -C /root/www/ghasele-api-new
ls /root/www/ghasele-api-new/Ghasele.API.dll
```

That last `ls` must print the path. If it errors, the archive is wrong - stop
here, the live app is still untouched.

---

## 4. Compare configuration before overwriting

If anyone has hand-edited `appsettings.json` on the server - a connection string,
a token, a `Maintenance` flag - this deployment silently reverts it. Check first:

```bash
diff /root/www/ghasele-api/appsettings.json /root/www/ghasele-api-new/appsettings.json
```

No output means identical, carry on. If it differs, decide deliberately which
version wins and fold the server-side changes back into the repo afterwards, or
they will be lost again on the next deploy.

---

## 5. Stop, back up, swap

```bash
systemctl stop ghasele-api.service

rm -rf /root/www/ghasele-api-backup
mv /root/www/ghasele-api /root/www/ghasele-api-backup

mv /root/www/ghasele-api-new /root/www/ghasele-api
```

`mv` rather than `cp` for the swap: it is a rename within one filesystem, so it
is instant and cannot leave a half-copied app behind. Downtime is the few seconds
between `stop` and `start`.

---

## 6. Start and verify

```bash
systemctl start ghasele-api.service
systemctl status ghasele-api.service --no-pager
```

Watch the live log for startup errors:

```bash
journalctl -u ghasele-api.service -n 50 --no-pager
```

Then confirm it is actually serving. Substitute the port from step 0:

```bash
curl -i http://localhost:5000/swagger/index.html
```

A `200` or a `301/302` means Kestrel is up. Connection refused means it crashed -
check `journalctl` and roll back.

### End-to-end check for the Apple fix

On the phone, tap **Sign in with Apple** on the build already installed. No
reinstall or new TestFlight build is needed: the app was always sending a valid
token, the server was rejecting it.

- Success -> deployment worked
- `IDX10214` again -> the running config still has the old bundle id, see below

---

## 7. If IDX10214 persists

Environment variables override `appsettings.json` in .NET configuration, so a
stale override on the unit beats the file every time:

```bash
systemctl cat ghasele-api.service | grep -i environment
```

If you see `AppleAuth__ClientIds__0=com.kalbouneh.naqa`, that is the real source.
Edit it with `systemctl edit ghasele-api.service`, then:

```bash
systemctl daemon-reload
systemctl restart ghasele-api.service
```

Also worth confirming the process really restarted - editing a config file
without a restart changes nothing, because .NET reads this at startup:

```bash
systemctl show ghasele-api.service --property=ActiveEnterTimestamp
```

---

## 8. Rollback

The previous release is intact in the backup folder:

```bash
systemctl stop ghasele-api.service
rm -rf /root/www/ghasele-api-failed
mv /root/www/ghasele-api /root/www/ghasele-api-failed
mv /root/www/ghasele-api-backup /root/www/ghasele-api
systemctl start ghasele-api.service
```

Keeping the failed build as `ghasele-api-failed` rather than deleting it means
the logs and binaries are still there to diagnose once the site is back up.

> After a rollback there is no backup folder any more - it became the live app.
> The next deployment recreates it at step 5.

---

## 9. Clean up

Once the app is confirmed healthy:

```bash
rm -f /tmp/ghasele-api.tar.gz
```

Leave `/root/www/ghasele-api-backup` in place. It is the rollback.

---

## Notes

- **Database migrations** are not covered here. If a release includes EF Core
  migrations, apply them between steps 5 and 6, while the service is stopped.
- **Secrets**: `appsettings.json` currently carries the JWT signing secret and the
  production Postgres password in plaintext, and this runbook copies it to the
  server on every deploy. Moving both to environment variables would mean config
  changes no longer require a redeploy, and would keep them out of the repo.
- **`ClientIds` is a list.** When a web or Android Apple sign-in flow is added,
  append the Services ID rather than replacing the bundle id:
  `"ClientIds": [ "com.kalbouneh.cleanyjo", "com.kalbouneh.cleanyjo.web" ]`
