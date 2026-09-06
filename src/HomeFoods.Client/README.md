# HomeFoods Client

React + TypeScript front-end application for HomeFoods grocery store.

## Features

- 🏠 Home page with featured products
- 🛒 Product catalog with search and filtering
- 📦 Product details page
- 🛍️ Shopping cart functionality
- 📱 Responsive design with Material-UI

## Tech Stack

- **React 18** with TypeScript
- **Material-UI (MUI)** for UI components
- **React Router** for navigation
- **Axios** for API calls
- **Vite** for fast development and building

## Getting Started

### Prerequisites

- Node.js 18+ 
- npm or yarn

### Installation

```bash
# Install dependencies
npm install
```

### Development

```bash
# Start development server (runs on http://localhost:3000)
npm run dev
```

### Build for Production

```bash
# Create production build
npm run build

# Preview production build
npm run preview
```

## Project Structure

```
src/
├── components/       # Reusable UI components
│   ├── Header.tsx
│   └── ProductCard.tsx
├── context/         # React context providers
│   └── CartContext.tsx
├── pages/           # Page components
│   ├── HomePage.tsx
│   ├── CatalogPage.tsx
│   ├── ProductDetailPage.tsx
│   └── CartPage.tsx
├── services/        # API service layer
│   └── api.ts
├── types/           # TypeScript type definitions
│   └── index.ts
├── App.tsx          # Main app component
└── main.tsx         # Entry point
```

## API Configuration

Update the API URL in `.env` file:

```
VITE_API_URL=http://localhost:5000/api
```

## Available Routes

- `/` - Home page
- `/catalog` - Product catalog
- `/product/:id` - Product details
- `/cart` - Shopping cart
