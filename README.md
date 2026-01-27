# PSMS (Monorepo)

This is a monorepo containing both the backend and frontend for the PSMS project.

## Project Structure

```
PSMS/
├── backend/          # ASP.NET Core backend
│   ├── src/         # Source code
│   ├── test/        # Tests
│   └── psms.sln     # Solution file
│
├── frontend/         # Next.js frontend
│   ├── src/         # Source code
│   ├── public/      # Static assets
│   └── package.json # Dependencies
│
└── README.md        # This file
```

## Getting Started

### Backend

```bash
cd backend
# Add backend setup instructions here
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

## Development

This project uses a monorepo structure to keep the backend and frontend code in a single repository for easier management and deployment.
