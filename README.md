# FaithFlow 🕊️

FaithFlow is a modern, full-fledged church community platform built with React, Vite, Tailwind CSS (Glassmorphism UI), and an ASP.NET Core 8 backend.

## 🌟 Features

- **Auth System**: Secure JWT authentication with role-based access (Member, Admin).
- **Prayer Wall**: Real-time prayer wall using SignalR. Includes an AI Guided Prayer feature powered by Google Gemini.
- **Sermon Notes**: Distraction-free sermon note-taking with automatic Bible Verse lookup via bible-api.com.
- **Community Groups**: Discover, join, and manage small groups.
- **Volunteer Opportunities**: Browse and sign up for serving opportunities with visual capacity tracking.
- **Admin Dashboard**: Analytics and management interface for church staff.

## 🏗 Architecture

- **Frontend**: React 18, Vite, TypeScript, Tailwind CSS v4, React Query, Zustand, React Router.
- **Backend**: ASP.NET Core 8 Web API, Entity Framework Core (InMemory for dev, ready for PostgreSQL), SignalR, BCrypt.

## 🚀 Local Development Setup

### Prerequisites
- Node.js 20+
- .NET 8 SDK
- Docker (Optional)

### Using Docker Compose (Easiest)
1. Rename `.env.example` to `.env` and fill in your Gemini API key (optional).
2. Run:
```bash
docker compose up --build -d
```
3. Open `http://localhost:5173` in your browser.

### Manual Setup
**Backend:**
```bash
cd backend/src/FaithFlow.API
dotnet run
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```

### 🔑 Test Accounts
The database automatically seeds with sample data on startup.
- **Admin User**: `admin@faithflow.com` / `Admin123!`

## ☁️ Deployment

### Deploying the Backend to Railway
1. Push this repository to GitHub.
2. Go to [Railway.app](https://railway.app/) and create a new project.
3. Choose "Deploy from GitHub repo" and select this repository.
4. Set the Root Directory to `/backend`.
5. Add the following Environment Variables in Railway:
   - `JwtSettings__Secret`
   - `GoogleAI__ApiKey`
6. Railway will automatically detect the Dockerfile and deploy the .NET 8 API.

### Deploying the Frontend to Vercel
1. Go to [Vercel](https://vercel.com/) and import the repository.
2. Set the Framework Preset to `Vite`.
3. Set the Root Directory to `frontend`.
4. Add the Environment Variable:
   - `VITE_API_URL`: Your deployed Railway backend URL (e.g., `https://your-railway-app.up.railway.app/api`)
5. Click Deploy!
