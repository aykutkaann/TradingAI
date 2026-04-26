# Deploying Trendox AI

Recommended stack: **Railway** for backend + Postgres, **Vercel** for frontend. Both have free tiers and connect to GitHub directly.

## 0. Prerequisites

- Push the repo to GitHub (private is fine).
- Have your **Grok API key**, **JWT signing key**, and **Mailtrap (or other SMTP)** credentials ready.

## 1. Backend on Railway

### 1.1 Create the project
1. Sign in at [railway.app](https://railway.app) with your GitHub.
2. **New Project → Deploy from GitHub repo →** pick your repo.
3. Railway auto-detects the `Dockerfile` at the repo root. Build will start.

### 1.2 Add Postgres
1. In the same project: **+ New → Database → Postgres**.
2. Click the Postgres service → **Variables** tab → copy `DATABASE_URL`. It looks like:
   `postgresql://postgres:PASSWORD@host.railway.internal:5432/railway`

### 1.3 Set backend env vars
On the **API service** (not Postgres) → **Variables** tab, add:

| Variable | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | (paste the `DATABASE_URL` from step 1.2, but replace `postgresql://` with `Host=…;Port=5432;Database=…;Username=…;Password=…;Ssl Mode=Require;`) |
| `Cors__AllowedOrigins__0` | `https://your-frontend.vercel.app` (add later, after Vercel deploy) |
| `Jwt__Key` | a long random string (≥ 32 chars) |
| `Jwt__Issuer` | `https://your-backend.up.railway.app` |
| `Jwt__Audience` | `trendox-ai` |
| `Grok__ApiKey` | your xAI key |
| `Grok__BaseUrl` | `https://api.x.ai/v1` |
| `Grok__Model` | `grok-2-vision-1212` (or whatever you're using) |
| `Email__Host` | `sandbox.smtp.mailtrap.io` |
| `Email__Port` | `587` |
| `Email__UserName` | (Mailtrap user) |
| `Email__Password` | (Mailtrap pass) |
| `Email__FromEmail` | `noreply@trendox.ai` |
| `Email__FromName` | `Trendox AI` |
| `Email__UseSsl` | `false` |

> **Note** — env var keys use `__` (double underscore) instead of the `:` you'd write in `appsettings.json`. That's how .NET configuration maps env vars to nested keys.

> **Connection string format** — Railway gives you a `postgres://` URL but Npgsql wants the key=value form. Either convert by hand (above), or set `DATABASE_URL` and add a small parser at startup. The hand-conversion is simpler for a one-off.

### 1.4 Generate a public URL
Service settings → **Networking → Generate Domain**. Copy the URL (e.g. `https://trendox-api-production.up.railway.app`).

### 1.5 First deploy
Railway will redeploy automatically when you push env vars. Watch **Deploy Logs** — you should see EF Core run migrations, then `Now listening on: http://[::]:8080`.

Test: `https://your-backend.up.railway.app/health` → `Healthy`.

## 2. Frontend on Vercel

### 2.1 Import the repo
1. Sign in at [vercel.com](https://vercel.com) with GitHub.
2. **Add New → Project →** pick the same repo.
3. **Root Directory:** `tradingai-web`
4. **Framework preset:** Vite (auto-detected).
5. **Environment Variables:**
   - `VITE_API_URL` = `https://your-backend.up.railway.app`

### 2.2 Deploy
Click Deploy. First build takes ~1 minute. You get a URL like `https://trendox-ai.vercel.app`.

### 2.3 Wire it back into backend CORS
Go back to Railway → API service → Variables → update `Cors__AllowedOrigins__0` to the Vercel URL → service redeploys.

## 3. Verify end-to-end

1. Open the Vercel URL.
2. Register a new account → should redirect home and show your avatar in the navbar.
3. Run an analysis (asset or image).
4. Open `/feed`, `/plans`, `/profile` — all should work.

## 4. After-deploy housekeeping

- **Custom domain:** add it in Vercel (frontend) and Railway (backend) settings. Update `Cors__AllowedOrigins__0` and `Jwt__Issuer` to match.
- **Logs:** Railway → service → Logs tab. Vercel → Deployments → individual deployment.
- **Cost watch:** Railway free tier ≈ $5/month of usage. The .NET 10 image with EF Core is heavy at startup — set the service to **Sleep on Inactivity** to save credits.

## 5. Troubleshooting

| Symptom | Likely cause |
|---|---|
| `connection refused` on first deploy | Postgres not ready yet — wait 30s and re-deploy the API service |
| `CORS blocked` in browser | `Cors__AllowedOrigins__0` doesn't EXACTLY match the frontend URL (must include scheme, no trailing slash) |
| Frontend reaches backend but `401` everywhere | `Jwt__Key` mismatch between the issuing run and the validating run. Make sure it's set on Railway, not just locally. |
| `Cannot find connection string 'DefaultConnection'` | Use `ConnectionStrings__DefaultConnection` (note the plural + double underscore), not `ConnectionString__DefaultConnection`. |
| Routes like `/feed` 404 on Vercel | `vercel.json` rewrite missing — confirm it's at `tradingai-web/vercel.json`. |
| Migrations didn't run | Check Railway logs for the EF Core log lines on startup. If they're missing, the service didn't start; check `ASPNETCORE_URLS` and the connection string. |
