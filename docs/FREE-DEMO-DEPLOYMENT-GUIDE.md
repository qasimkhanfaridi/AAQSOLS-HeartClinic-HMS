# 100% Free Long-Term Demo Deployment Guide

**Stack:** Vercel (Frontend) + Render.com (Backend Docker) + Embedded SQLite (Demo Database)  
**Cost:** $0.00 / month forever  
**Expiration:** Never expires (no 30-day trial limits)

---

## Architecture Overview

```
Client Browser (Doctor / Reception / Admin)
                     │
                     ▼
       ┌───────────────────────────┐
       │   Vercel (100% Free)      │  <-- React 18 + Vite (Instant load)
       │   https://demo.vercel.app │
       └─────────────┬─────────────┘
                     │ (API calls via VITE_API_URL)
                     ▼
       ┌───────────────────────────┐
       │   Render.com (100% Free)  │  <-- Docker (.NET 10 Web API)
       │   https://api.onrender.com│
       │  ┌──────────────────────┐ │
       │  │  Embedded SQLite DB  │ │  <-- Auto-migrated & seeded demo database
       │  └──────────────────────┘ │
       └───────────────────────────┘
```

---

## Step 1: Push Changes to GitHub

From your local repository root:

```cmd
git add .
git commit -m "Configure 100% free long-term demo hosting (Render + Vercel + SQLite)"
git push origin qasimkhanfaridi-turbo-funicular
```

*(If you wish to merge into `main`, create a PR or merge it into `main` before deploying).*

---

## Step 2: Deploy Backend to Render.com (Free Web Service)

1. Sign up / log in to [Render.com](https://render.com) (Free account, no credit card required).
2. Click **New +** &rarr; **Web Service**.
3. Select **Build and deploy from a Git repository** and connect your GitHub repository:
   - **Repository:** `AAQSOLS-HeartClinic-HMS`
   - **Branch:** `main` (or `qasimkhanfaridi-turbo-funicular`)
4. Configure service details:
   - **Name:** `heartclinic-api` *(or your preferred name)*
   - **Region:** Choose the closest region (e.g. Frankfurt, Oregon, Singapore)
   - **Runtime:** **Docker** *(Render detects the root `Dockerfile` automatically)*
   - **Instance Type:** **Free** ($0 / month)
5. Under **Environment Variables**, verify or add:
   - `ASPNETCORE_ENVIRONMENT` = `Demo`
   - `UseSqlite` = `true`
   - `PORT` = `8080`
6. Click **Deploy Web Service**.
7. In ~3 minutes, your API is live! Copy the URL:
   `https://heartclinic-api.onrender.com`
8. Verify it by visiting:
   `https://heartclinic-api.onrender.com/health`  
   *(Expected response: `{"status":"healthy","product":"PulseCore Heart Clinic HMS","database":"SQLite (Demo)"}`)*

---

## Step 3: Deploy Frontend to Vercel (Free Static Hosting)

1. Sign up / log in to [Vercel.com](https://vercel.com) (Free hobby account).
2. Click **Add New...** &rarr; **Project**.
3. Select your GitHub repository (`AAQSOLS-HeartClinic-HMS`).
4. In the configuration screen:
   - **Root Directory:** Click **Edit** and choose `src/Web`.
   - **Framework Preset:** `Vite` (auto-detected).
   - **Build Command:** `npm run build` (auto-detected).
   - **Output Directory:** `dist` (auto-detected).
5. Expand the **Environment Variables** section and add:
   - **Key:** `VITE_API_URL`
   - **Value:** Your Render backend URL (e.g. `https://heartclinic-api.onrender.com` — *no trailing slash*).
6. Click **Deploy**.
7. In ~1 minute, your clinic portal is live at:
   `https://aaqsols-heartclinic-hms.vercel.app`

---

## Step 4: Keep the Demo Awake 24/7 (Prevent Cold Starts)

Render free instances sleep after 15 minutes of inactivity. When a client visits, the first request might take ~30 seconds to wake up.

To keep it **awake 24/7 with zero delay**:

1. Sign up for a free account at [UptimeRobot.com](https://uptimerobot.com).
2. Click **Add New Monitor**:
   - **Monitor Type:** `HTTP(s)`
   - **Friendly Name:** `HeartClinic API Health`
   - **URL:** `https://your-api-name.onrender.com/health`
   - **Monitoring Interval:** `Every 10 minutes`
3. Click **Create Monitor**.

Now UptimeRobot pings the API every 10 minutes, keeping the Render container warm around the clock!

---

## Demo Credentials for the Client

| Role | Username | Password |
|---|---|---|
| **Administrator** | `admin` | `Admin@123` |
| **Reception** | `reception` | `Reception@123` |
| **Doctor** | `doctor` | `Doctor@123` |

All patient vaults, challans, consultation queues, and cash flow reports will work seamlessly with the pre-seeded demo dataset.
